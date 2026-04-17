using BugPro;

namespace BugTests;

[TestClass]
public sealed class UnitTest1
{
    [TestMethod]
    public void InitialState_IsNew()
    {
        var bug = new Bug("A");
        Assert.AreEqual(Bug.State.New, bug.CurrentState);
    }

    [TestMethod]
    public void EmptyTitle_ReplacedWithFallback()
    {
        var bug = new Bug("   ");
        Assert.AreEqual("Untitled bug", bug.Title);
    }

    [TestMethod]
    public void StartTriage_FromNew_GoesToTriaged()
    {
        var bug = new Bug("A");
        bug.Fire(Bug.Trigger.StartTriage);
        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void RequestInfo_FromTriaged_GoesToNeedInfo()
    {
        var bug = MoveToTriaged();
        bug.Fire(Bug.Trigger.RequestInfo);
        Assert.AreEqual(Bug.State.NeedInfo, bug.CurrentState);
    }

    [TestMethod]
    public void ProvideInfo_FromNeedInfo_ReturnsToTriaged()
    {
        var bug = MoveToNeedInfo();
        bug.Fire(Bug.Trigger.ProvideInfo);
        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void AcceptAsBug_FromTriaged_GoesToInProgress()
    {
        var bug = MoveToTriaged();
        bug.Fire(Bug.Trigger.AcceptAsBug);
        Assert.AreEqual(Bug.State.InProgress, bug.CurrentState);
    }

    [TestMethod]
    public void RejectAsNotBug_FromTriaged_GoesToReturned()
    {
        var bug = MoveToTriaged();
        bug.Fire(Bug.Trigger.RejectAsNotBug);
        Assert.AreEqual(Bug.State.Returned, bug.CurrentState);
    }

    [TestMethod]
    public void MarkDuplicate_FromTriaged_GoesToReturned()
    {
        var bug = MoveToTriaged();
        bug.Fire(Bug.Trigger.MarkDuplicate);
        Assert.AreEqual(Bug.State.Returned, bug.CurrentState);
    }

    [TestMethod]
    public void CannotReproduce_FromTriaged_GoesToReturned()
    {
        var bug = MoveToTriaged();
        bug.Fire(Bug.Trigger.MarkAsCannotReproduce);
        Assert.AreEqual(Bug.State.Returned, bug.CurrentState);
    }

    [TestMethod]
    public void CompleteFix_FromInProgress_GoesToWaitingVerification()
    {
        var bug = MoveToInProgress();
        bug.Fire(Bug.Trigger.CompleteFix);
        Assert.AreEqual(Bug.State.WaitingVerification, bug.CurrentState);
    }

    [TestMethod]
    public void RejectFix_FromWaitingVerification_GoesToInProgress()
    {
        var bug = MoveToWaitingVerification();
        bug.Fire(Bug.Trigger.RejectFix);
        Assert.AreEqual(Bug.State.InProgress, bug.CurrentState);
    }

    [TestMethod]
    public void ApproveFix_FromWaitingVerification_GoesToResolved()
    {
        var bug = MoveToWaitingVerification();
        bug.Fire(Bug.Trigger.ApproveFix);
        Assert.AreEqual(Bug.State.Resolved, bug.CurrentState);
    }

    [TestMethod]
    public void ApproveFix_FromResolved_GoesToClosed()
    {
        var bug = MoveToResolved();
        bug.Fire(Bug.Trigger.ApproveFix);
        Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
    }

    [TestMethod]
    public void Reopen_FromClosed_GoesToReopened()
    {
        var bug = MoveToClosed();
        bug.Fire(Bug.Trigger.Reopen);
        Assert.AreEqual(Bug.State.Reopened, bug.CurrentState);
    }

    [TestMethod]
    public void StartFix_FromReopened_GoesToInProgress()
    {
        var bug = MoveToClosed();
        bug.Fire(Bug.Trigger.Reopen);
        bug.Fire(Bug.Trigger.StartFix);
        Assert.AreEqual(Bug.State.InProgress, bug.CurrentState);
    }

    [TestMethod]
    public void ReturnToQueue_FromInProgress_GoesToReturned()
    {
        var bug = MoveToInProgress();
        bug.Fire(Bug.Trigger.ReturnToQueue);
        Assert.AreEqual(Bug.State.Returned, bug.CurrentState);
    }

    [TestMethod]
    public void ReturnToQueue_FromNeedInfo_GoesToReturned()
    {
        var bug = MoveToNeedInfo();
        bug.Fire(Bug.Trigger.ReturnToQueue);
        Assert.AreEqual(Bug.State.Returned, bug.CurrentState);
    }

    [TestMethod]
    public void Reopen_FromReturned_GoesToReopened()
    {
        var bug = MoveToTriaged();
        bug.Fire(Bug.Trigger.RejectAsNotBug);
        bug.Fire(Bug.Trigger.Reopen);
        Assert.AreEqual(Bug.State.Reopened, bug.CurrentState);
    }

    [TestMethod]
    public void CanFire_ReportsAllowedTrigger()
    {
        var bug = MoveToTriaged();
        Assert.IsTrue(bug.CanFire(Bug.Trigger.AcceptAsBug));
    }

    [TestMethod]
    public void CanFire_ReportsForbiddenTrigger()
    {
        var bug = new Bug("A");
        Assert.IsFalse(bug.CanFire(Bug.Trigger.ApproveFix));
    }

    [TestMethod]
    public void AvailableTriggers_InNewContainsOnlyStartTriage()
    {
        var bug = new Bug("A");
        CollectionAssert.AreEquivalent(
            new[] { Bug.Trigger.StartTriage },
            bug.AvailableTriggers.ToArray());
    }

    [TestMethod]
    public void Fire_ForbiddenTriggerInNew_ThrowsInvalidOperationException()
    {
        var bug = new Bug("A");
        Assert.ThrowsException<InvalidOperationException>(() => bug.Fire(Bug.Trigger.ApproveFix));
    }

    [TestMethod]
    public void Fire_ForbiddenTriggerInClosed_ThrowsInvalidOperationException()
    {
        var bug = MoveToClosed();
        Assert.ThrowsException<InvalidOperationException>(() => bug.Fire(Bug.Trigger.AcceptAsBug));
    }

    [TestMethod]
    public void FullHappyPath_EndsInClosed()
    {
        var bug = new Bug("A");
        bug.Fire(Bug.Trigger.StartTriage);
        bug.Fire(Bug.Trigger.AcceptAsBug);
        bug.Fire(Bug.Trigger.CompleteFix);
        bug.Fire(Bug.Trigger.ApproveFix);
        bug.Fire(Bug.Trigger.ApproveFix);

        Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
    }

    private static Bug MoveToTriaged()
    {
        var bug = new Bug("A");
        bug.Fire(Bug.Trigger.StartTriage);
        return bug;
    }

    private static Bug MoveToNeedInfo()
    {
        var bug = MoveToTriaged();
        bug.Fire(Bug.Trigger.RequestInfo);
        return bug;
    }

    private static Bug MoveToInProgress()
    {
        var bug = MoveToTriaged();
        bug.Fire(Bug.Trigger.AcceptAsBug);
        return bug;
    }

    private static Bug MoveToWaitingVerification()
    {
        var bug = MoveToInProgress();
        bug.Fire(Bug.Trigger.CompleteFix);
        return bug;
    }

    private static Bug MoveToResolved()
    {
        var bug = MoveToWaitingVerification();
        bug.Fire(Bug.Trigger.ApproveFix);
        return bug;
    }

    private static Bug MoveToClosed()
    {
        var bug = MoveToResolved();
        bug.Fire(Bug.Trigger.ApproveFix);
        return bug;
    }
}

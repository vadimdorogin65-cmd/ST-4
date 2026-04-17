using Stateless;

namespace BugPro;

public sealed class Bug
{
    public enum State
    {
        New,
        Triaged,
        NeedInfo,
        InProgress,
        WaitingVerification,
        Resolved,
        Closed,
        Reopened,
        Returned
    }

    public enum Trigger
    {
        StartTriage,
        RequestInfo,
        ProvideInfo,
        AcceptAsBug,
        RejectAsNotBug,
        MarkDuplicate,
        MarkAsCannotReproduce,
        StartFix,
        CompleteFix,
        ApproveFix,
        RejectFix,
        ReturnToQueue,
        Reopen
    }

    private readonly StateMachine<State, Trigger> _machine;

    public Bug(string title)
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Untitled bug" : title.Trim();
        CurrentState = State.New;
        _machine = new StateMachine<State, Trigger>(() => CurrentState, s => CurrentState = s);
        ConfigureWorkflow();
    }

    public string Title { get; }

    public State CurrentState { get; private set; }

    public IReadOnlyCollection<Trigger> AvailableTriggers => _machine.PermittedTriggers.ToArray();

    public bool CanFire(Trigger trigger) => _machine.CanFire(trigger);

    public void Fire(Trigger trigger) => _machine.Fire(trigger);

    private void ConfigureWorkflow()
    {
        _machine.Configure(State.New)
            .Permit(Trigger.StartTriage, State.Triaged);

        _machine.Configure(State.Triaged)
            .Permit(Trigger.RequestInfo, State.NeedInfo)
            .Permit(Trigger.AcceptAsBug, State.InProgress)
            .Permit(Trigger.RejectAsNotBug, State.Returned)
            .Permit(Trigger.MarkDuplicate, State.Returned)
            .Permit(Trigger.MarkAsCannotReproduce, State.Returned);

        _machine.Configure(State.NeedInfo)
            .Permit(Trigger.ProvideInfo, State.Triaged)
            .Permit(Trigger.ReturnToQueue, State.Returned);

        _machine.Configure(State.InProgress)
            .Permit(Trigger.CompleteFix, State.WaitingVerification)
            .Permit(Trigger.ReturnToQueue, State.Returned);

        _machine.Configure(State.WaitingVerification)
            .Permit(Trigger.ApproveFix, State.Resolved)
            .Permit(Trigger.RejectFix, State.InProgress);

        _machine.Configure(State.Resolved)
            .Permit(Trigger.ApproveFix, State.Closed)
            .Permit(Trigger.Reopen, State.Reopened);

        _machine.Configure(State.Closed)
            .Permit(Trigger.Reopen, State.Reopened);

        _machine.Configure(State.Reopened)
            .Permit(Trigger.StartFix, State.InProgress)
            .Permit(Trigger.ReturnToQueue, State.Returned);

        _machine.Configure(State.Returned)
            .Permit(Trigger.Reopen, State.Reopened);
    }
}

public static class Program
{
    public static void Main()
    {
        var bug = new Bug("Login button does nothing");

        Console.WriteLine($"Bug: {bug.Title}");
        Console.WriteLine($"Current state: {bug.CurrentState}");

        bug.Fire(Bug.Trigger.StartTriage);
        bug.Fire(Bug.Trigger.AcceptAsBug);
        bug.Fire(Bug.Trigger.CompleteFix);
        bug.Fire(Bug.Trigger.ApproveFix);
        bug.Fire(Bug.Trigger.ApproveFix);

        Console.WriteLine($"Final state: {bug.CurrentState}");
        Console.WriteLine("Available triggers after closure:");
        foreach (var trigger in bug.AvailableTriggers)
        {
            Console.WriteLine($"- {trigger}");
        }
    }
}

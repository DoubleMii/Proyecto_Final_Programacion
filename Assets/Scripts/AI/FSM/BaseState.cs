public abstract class BaseState
{
    public abstract void Enter(StateMachine machine);
    public abstract void Execute(StateMachine machine);
    public abstract void Exit(StateMachine machine);
}

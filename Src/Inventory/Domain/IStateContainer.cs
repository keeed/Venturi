namespace Domain
{
    public interface IStateContainer<TState> where TState : class
    {
         TState GetCurrentState();
    }
}
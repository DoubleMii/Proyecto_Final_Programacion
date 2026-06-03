using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private BaseState currentState;

    private void Update()
    {
        if (currentState != null)
        {
            currentState.Execute(this);
        }
    }

    public void ChangeState(BaseState newState)
    {
        if (currentState != null)
        {
            currentState.Exit(this);
        }

        currentState = newState;
        
        if (currentState != null)
        {
            currentState.Enter(this);
        }
    }
}

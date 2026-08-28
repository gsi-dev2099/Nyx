using System;

namespace CRM.ApiHub.Domain.Exceptions;

public class InvalidTransitionException : Exception
{
    public string EntityType { get; }
    public int CurrentState { get; }
    public int TargetState { get; }

    public InvalidTransitionException(string entityType, int currentState, int targetState, string message = "Transición de estado no válida.") 
        : base(message)
    {
        EntityType = entityType;
        CurrentState = currentState;
        TargetState = targetState;
    }

    public InvalidTransitionException(string message) : base(message)
    {
        EntityType = "Unknown";
    }
    
    public InvalidTransitionException(string message, Exception innerException) : base(message, innerException)
    {
        EntityType = "Unknown";
    }
}

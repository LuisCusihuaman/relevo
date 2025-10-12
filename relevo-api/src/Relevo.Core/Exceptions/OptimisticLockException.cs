namespace Relevo.Core.Exceptions;

/// <summary>
/// Exception thrown when an optimistic concurrency conflict is detected.
/// This occurs when a record has been modified by another user/process
/// since it was last read by the current operation.
/// </summary>
public class OptimisticLockException : Exception
{
    public OptimisticLockException(string message) : base(message)
    {
    }

    public OptimisticLockException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}


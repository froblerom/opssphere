namespace OpsSphere.Application.Common.Exceptions;

public sealed class ValidationException : Exception
{
    public IReadOnlyList<ValidationFailure> Failures { get; }

    public ValidationException(string field, string message)
        : base("One or more validation failures occurred.")
    {
        Failures = [new ValidationFailure(field, message)];
    }

    public ValidationException(IReadOnlyList<ValidationFailure> failures)
        : base("One or more validation failures occurred.")
    {
        Failures = failures;
    }
}

public sealed record ValidationFailure(string Field, string Message);

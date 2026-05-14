namespace OpsSphere.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entityName, object id)
        : base($"{entityName} '{id}' was not found.") { }
}

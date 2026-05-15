namespace OpsSphere.Domain.Exceptions;

public sealed class TicketDomainException : DomainException
{
    public TicketDomainException(string message) : base(message) { }
}

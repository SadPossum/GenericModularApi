namespace Auth.Domain.Exceptions;

using System;

public class TokenCantBeNullInActiveMemberSessionException : ApplicationException
{
    public TokenCantBeNullInActiveMemberSessionException()
        : base("Tokens cannot be null in an active member session.")
    {
    }

    public TokenCantBeNullInActiveMemberSessionException(string message)
        : base(message)
    {
    }

    public TokenCantBeNullInActiveMemberSessionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

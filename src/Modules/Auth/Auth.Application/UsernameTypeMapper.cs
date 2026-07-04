namespace Auth.Application;

using Auth.Contracts;
using Auth.Domain.Enums;
using Shared.Results;

internal static class UsernameTypeMapper
{
    public static Result<MemberUsernameType> Map(UsernameType usernameType) =>
        usernameType switch
        {
            UsernameType.Email => Result.Success(MemberUsernameType.Email),
            UsernameType.Phone => Result.Success(MemberUsernameType.Phone),
            _ => Result.Failure<MemberUsernameType>(AuthApplicationErrors.UsernameTypeInvalid)
        };
}

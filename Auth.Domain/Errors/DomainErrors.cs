namespace Auth.Domain.Errors;

using Shared.ErrorHandling;

public static class DomainErrors
{
    public static class Member
    {
        public static readonly Error CredentialsNotValid = new(
            $"{nameof(Member)}.{nameof(CredentialsNotValid)}",
            "Username or password is incorrect");

        public static readonly Error PasswordNotValid = new(
            $"{nameof(Member)}.{nameof(PasswordNotValid)}",
            "Password is not valid");

        public static readonly Error OldPasswordNotMatch = new(
            $"{nameof(Member)}.{nameof(OldPasswordNotMatch)}",
            "The old password provided does not match the current password");

        public static readonly Error SessionNotFound = new(
            $"{nameof(MemberSession)}.{nameof(SessionNotFound)}",
            "Session have not been found in list of member sessions");
    }

    public static class MemberSession
    {
        public static readonly Error SessionDeactivated = new(
            $"{nameof(MemberSession)}.{nameof(SessionDeactivated)}",
            "Deactivated session can not be modified");

        public static readonly Error SessionAlreadyDeactivated = new(
            $"{nameof(MemberSession)}.{nameof(SessionAlreadyDeactivated)}",
            "Session have been already deactivated");

        public static readonly Error NoAnyActiveSessionHaveBeenFound = new(
            $"{nameof(MemberSession)}.{nameof(NoAnyActiveSessionHaveBeenFound)}",
            "Member have no active sessions");

        public static readonly Error AccessTokenNotValid = new(
            $"{nameof(MemberSession)}.{nameof(NoAnyActiveSessionHaveBeenFound)}",
            "Access token is not valid");

        public static readonly Error RefreshTokenExpired = new(
            $"{nameof(MemberSession)}.{nameof(NoAnyActiveSessionHaveBeenFound)}",
            "Refresh token have been expired");

        public static readonly Error RefreshTokenNotMatch = new(
            $"{nameof(MemberSession)}.{nameof(NoAnyActiveSessionHaveBeenFound)}",
            "The refresh token provided does not match the current refresh token");
    }

    public static class MemberUsername
    {
        public static readonly Error UsernameNotFound = new(
            $"{nameof(MemberSession)}.{nameof(UsernameNotFound)}",
            "Username have not been found in list of member usernames");

        public static readonly Error UsernameIsNotValid = new(
            $"{nameof(MemberUsername)}.{nameof(UsernameIsNotValid)}",
            "Username is not validated");

        public static readonly Error UsernameDeactivated = new(
            $"{nameof(MemberUsername)}.{nameof(UsernameDeactivated)}",
            "Deactivated username can not be modified");
    }
}

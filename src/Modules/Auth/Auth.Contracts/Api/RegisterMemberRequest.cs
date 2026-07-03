namespace Auth.Contracts;

public sealed record RegisterMemberRequest(string Username, UsernameType UsernameType, string Password);

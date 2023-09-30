namespace Auth.Domain.ValueObjects;

using System;

public record RefreshToken(string Value, DateTimeOffset ExpirationDate) { }

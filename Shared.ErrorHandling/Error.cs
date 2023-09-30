namespace Shared.ErrorHandling;
using System;

public sealed class Error(string code, string message) : IEquatable<Error>
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new($"{nameof(Error)}.{nameof(NullValue)}", "The specified result value is null");

    public string Code { get; } = code;
    public string Message { get; } = message;

    public static implicit operator string(Error error) => error.Code;

    public static bool operator ==(Error? a, Error? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(Error? a, Error? b)
    {
        if (a is null && b is null)
        {
            return false;
        }

        if (a is null || b is null)
        {
            return true;
        }

        return !a.Equals(b);
    }

    public bool Equals(Error? other) => other != null && this.Code == other.Code && this.Message == other.Message;

    public override bool Equals(object? obj) => this.Equals(obj as Error);
    public override int GetHashCode() => HashCode.Combine(this.Code, this.Message);
}

namespace Auth.Domain.Entities;

using System.Text.RegularExpressions;
using Auth.Domain.Enums;
using Auth.Domain.Errors;
using Auth.Domain.ValueObjects;
using Shared.Domain.Models;
using Shared.ErrorHandling;

public class MemberUsername : Entity<MemberUsernameId>
{
    public MemberId MemberId { get; }
    public string Value { get; }
    public bool IsActive { get; private set; }
    public MemberUsernameType UsernameType { get; }

    private MemberUsername(MemberUsernameId id,
        MemberId memberId,
        string value,
        bool isActive,
        MemberUsernameType usernameType) : base(id)
    {
        this.MemberId = memberId;
        this.Value = value;
        this.IsActive = isActive;
        this.UsernameType = usernameType;
    }

    internal static Result<MemberUsername> Create(MemberUsernameId id,
        MemberId memberId,
        string value,
        MemberUsernameType usernameType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<MemberUsername>(DomainErrors.MemberUsername.UsernameIsNotValid);
        }

        if (!IsValidUsernameFormat(value, usernameType))
        {
            return Result.Failure<MemberUsername>(DomainErrors.MemberUsername.UsernameIsNotValid);
        }

        MemberUsername username = new(id,
            memberId,
            value,
            true,
            usernameType);

        return username;
    }

    internal Result Deactivate()
    {
        if (!this.IsActive)
        {
            return Result.Failure(DomainErrors.MemberUsername.UsernameDeactivated);
        }

        this.IsActive = false;

        return Result.Success();
    }

    private static bool IsValidUsernameFormat(string value, MemberUsernameType usernameType) => usernameType switch
    {
        MemberUsernameType.Email => IsValidEmailFormat(value),
        MemberUsernameType.Phone => IsValidPhoneNumberFormat(value),
        _ => throw new ArgumentOutOfRangeException(nameof(usernameType)),
    };

    private static bool IsValidEmailFormat(string email) =>
        Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

    private static bool IsValidPhoneNumberFormat(string phoneNumber) =>
        phoneNumber.Length == 10 && phoneNumber.All(char.IsDigit);
}

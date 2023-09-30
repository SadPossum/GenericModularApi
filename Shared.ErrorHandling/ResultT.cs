namespace Shared.ErrorHandling;

using System;

public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error) =>
        this._value = value;

    public T Value
    {
        get
        {
            if (this.IsSuccess)
            {
                if (this._value == null)
                {
                    throw new InvalidOperationException("The value of success result can not be null");
                }

                return this._value;
            }
            else
            {
                throw new InvalidOperationException("The value of failure result can not be accessed");
            }
        }
    }

    public static implicit operator Result<T>(T value) => Success(value);
}

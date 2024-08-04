namespace DiscountApp.Driver.Core;

/// <summary>
/// Simplified and naive implementation of the functional Either type.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct Result<T>
{
    /// <summary>
    /// Success value.
    /// </summary>
    public readonly T? Value { get; }

    /// <summary>
    /// Error value.
    /// </summary>
    public readonly string? Error { get; }

    /// <summary>
    /// Indicator for failure.
    /// </summary>
    public readonly bool IsFailure => Error is not null;

    /// <summary>
    /// Indicator for success.
    /// </summary>
    public readonly bool IsSuccess => Value is not null;

    /// <summary>
    /// Private constructor so user can only create via static methods.
    /// </summary>
    /// <param name="value">Success value</param>
    /// <param name="error">Error value</param>
    private Result(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Functional style map, but named Select for consistency with the C# lang.
    /// </summary>
    /// <typeparam name="TResult">Type of result after conversion</typeparam>
    /// <param name="selector">mapping method</param>
    /// <returns>Mapped result</returns>
    public Result<TResult> Select<TResult>(Func<T, TResult> selector) => Value switch
    {
        null => Error,
        _ => selector(Value)
    };

    /// <summary>
    /// Implicit conversion from error to result type.
    /// </summary>
    /// <param name="error">Error</param>
    public static implicit operator Result<T>(string error) => new(default, error);

    /// <summary>
    /// Implicit conversion from value to result type.
    /// </summary>
    /// <param name="value">Value</param>
    public static implicit operator Result<T>(T value) => new(value, default);
}
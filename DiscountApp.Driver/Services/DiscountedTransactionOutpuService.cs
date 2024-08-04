
using DiscountApp.Driver.Core;
using DiscountApp.Driver.Models;

namespace DiscountApp.Driver.Services;

/// <summary>
/// Controls the way discounted transactions get outputted .
/// </summary>
public interface IDiscountedTransactionOutputService
{
    /// <summary>
    /// Writes results to console.
    /// </summary>
    /// <param name="results">Discounted transaction results</param>
    void Write(IEnumerable<Result<DiscountedTransactionModel>> results);
}

/// <summary>
/// Outputs discounted transactions results to the console.
/// </summary>
public class DiscountedTransactionConsoleOutputService : IDiscountedTransactionOutputService
{
    /// <inheritdoc/>
    public void Write(IEnumerable<Result<DiscountedTransactionModel>> results)
    {
        foreach(var result in results)
        {
            Console.WriteLine(result.IsSuccess ? result.Value : $"{result.Error} Ignored");
        }
    }
}
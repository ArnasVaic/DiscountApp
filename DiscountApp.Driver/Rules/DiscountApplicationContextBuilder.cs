using DiscountApp.Driver.Core;
using DiscountApp.Driver.Enums;
using DiscountApp.Driver.Models;

namespace DiscountApp.Driver.Rules;

/// <summary>
/// Interface for discount application context builder.
/// </summary>
public interface IDiscountApplicationContextBuilder
{
    /// <summary>
    /// Build discount application context for a specific transaction.
    /// </summary>
    /// <param name="transactionIndex">Index of the current transaction</param>
    /// <param name="transaction">Current transaction</param>
    /// <param name="availableBudget">Available budget for issuing discounts</param>
    /// <param name="transactionResults">All transactions from the input file</param>
    /// <returns>Discount application context</returns>
    IDiscountApplicationContext Build( 
        int transactionIndex,
        TransactionInputModel transaction,
        decimal availableBudget,
        IEnumerable<Result<TransactionInputModel>> transactionResults);
}

/// <summary>
/// Implementation for discount rule application builder.
/// </summary>
public class DiscountRuleApplicationContextBuilder : IDiscountApplicationContextBuilder
{
    /// <inheritdoc/>
    public IDiscountApplicationContext Build(
        int transactionIndex,
        TransactionInputModel transaction,
        decimal availableBudget,
        IEnumerable<Result<TransactionInputModel>> transactionResults) => transaction.Size switch
        {
            PackageSize.Small => new SmallPackageDiscountApplicationContext
            {
                Carrier = transaction.Carrier,
                Date = transaction.Date,
                AvailableBudget = availableBudget
            },
            PackageSize.Medium => new MediumPackageDiscountApplicationContext
            {
                Carrier = transaction.Carrier,
                Date = transaction.Date,
                AvailableBudget = availableBudget,
            },
            PackageSize.Large => new LargePackageDiscountApplicationContext
            {
                Carrier = transaction.Carrier,
                Date = transaction.Date,
                AvailableBudget = availableBudget,
                MonthlyLargeLaPosteTransactionCountBeforeCurrent = transactionResults
                    // We only want to count transactions up until the current one.
                    .Take(transactionIndex)
                    .Where(result => result.IsSuccess)
                    .Select(result => result.Value!)
                    .Where(tr => tr.Date.Month == transaction.Date.Month)
                    .Where(tr => tr.Carrier is Carrier.LaPoste)
                    .Where(tr => tr.Size is PackageSize.Large)
                    .Count()
            },
            _ => throw new ArgumentOutOfRangeException(nameof(transaction.Size))
        };
}
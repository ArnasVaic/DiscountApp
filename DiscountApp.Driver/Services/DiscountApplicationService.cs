
using DiscountApp.Driver.Core;
using DiscountApp.Driver.Enums;
using DiscountApp.Driver.Models;
using DiscountApp.Driver.Rules;

namespace DiscountApp.Driver.Services;

/// <summary>
/// Apply discounts for transactions.
/// </summary>
public interface IDiscountApplicationService
{
    /// <summary>
    /// Apply discounts.
    /// </summary>
    /// <param name="transactionResults">Parsed transactions</param>
    /// <returns>Discounted transactions</returns>
    IEnumerable<Result<DiscountedTransactionModel>> Apply(IEnumerable<Result<TransactionInputModel>> transactionResults);
}

/// <inheritdoc/>
public class DiscountApplicationService(
    IDiscountApplicationRule<SmallPackageDiscountApplicationContext> smallPackageRule,
    IDiscountApplicationRule<MediumPackageDiscountApplicationContext> mediumPackageRule,
    IDiscountApplicationRule<LargePackageDiscountApplicationContext> largePackageRule,
    IDiscountApplicationContextBuilder discountRuleApplicationContextBuilder)
    : IDiscountApplicationService
{
    /// <summary>
    /// Monthly discount budget.
    /// </summary>
    private const decimal MonthlyDiscountBudget = 10m;

    /// <inheritdoc/>
    public IEnumerable<Result<DiscountedTransactionModel>> Apply(IEnumerable<Result<TransactionInputModel>> transactionResults)
    {
        var monthDiscountBudgetLookup = BuildMonthDiscountBudgetLookup(transactionResults);

        return transactionResults.Select((result, id) => 
            result.Select(transaction =>
            {
                var availableBudget = monthDiscountBudgetLookup[transaction.Date.Month];
                var context = discountRuleApplicationContextBuilder.Build(id, transaction, availableBudget, transactionResults);
                var discountedTransaction = ApplyDiscount(transaction.Size, context);

                monthDiscountBudgetLookup[transaction.Date.Month] -= discountedTransaction.Discount;

                return discountedTransaction;
            })
        );
    }

    private Dictionary<int, decimal> BuildMonthDiscountBudgetLookup(
        IEnumerable<Result<TransactionInputModel>> transactionResults) => 
        transactionResults
        .Where(result => result.IsSuccess)
        .Select(transaction => transaction.Value!.Date.Month)
        .Distinct()
        .ToDictionary(month => month, _ => MonthlyDiscountBudget);

    private DiscountedTransactionModel ApplyDiscount(PackageSize size, IDiscountApplicationContext context) => size switch
    {
        PackageSize.Small => smallPackageRule.ApplyRule((SmallPackageDiscountApplicationContext)context),
        PackageSize.Medium => mediumPackageRule.ApplyRule((MediumPackageDiscountApplicationContext)context),
        PackageSize.Large => largePackageRule.ApplyRule((LargePackageDiscountApplicationContext)context),
        _ => throw new ArgumentOutOfRangeException(nameof(size))
    };
}
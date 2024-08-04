
using DiscountApp.Driver.Core;
using DiscountApp.Driver.Enums;
using DiscountApp.Driver.Models;
using DiscountApp.Driver.Rules;

namespace DiscountApp.Driver.Services;

public interface IDiscountApplicationService
{
    IEnumerable<Result<DiscountedTransactionModel>> CalculateDiscounts(IEnumerable<Result<TransactionInputModel>> transactionResults);
}

public class DiscountApplicationService(
    IDiscountApplicationRule<SmallPackageDiscountApplicationContext> smallPackageRule,
    IDiscountApplicationRule<MediumPackageDiscountApplicationContext> mediumPackageRule,
    IDiscountApplicationRule<LargePackageDiscountApplicationContext> largePackageRule,
    IDiscountApplicationContextBuilder discountRuleApplicationContextBuilder)
    : IDiscountApplicationService
{

    public IEnumerable<Result<DiscountedTransactionModel>> CalculateDiscounts(IEnumerable<Result<TransactionInputModel>> transactionResults)
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
        .ToDictionary(month => month, _ => 10m);

    private DiscountedTransactionModel ApplyDiscount(PackageSize size, IDiscountApplicationContext context) => size switch
    {
        PackageSize.Small => smallPackageRule.ApplyRule(context as SmallPackageDiscountApplicationContext),
        PackageSize.Medium => mediumPackageRule.ApplyRule(context as MediumPackageDiscountApplicationContext),
        PackageSize.Large => largePackageRule.ApplyRule(context as LargePackageDiscountApplicationContext),
        _ => throw new ArgumentOutOfRangeException(nameof(size))
    };
}
using DiscountApp.Driver.Enums;
using DiscountApp.Driver.Models;
using DiscountApp.Driver.Repositories;

namespace DiscountApp.Driver.Rules;

/// <summary>
/// Contract for describing how rules should be applied to transactions.
/// </summary>
/// <typeparam name="TDiscountApplicationContext">Type of discount application context</typeparam>
public interface IDiscountApplicationRule<TDiscountApplicationContext> 
where TDiscountApplicationContext 
    : IDiscountApplicationContext
{
    /// <summary>
    /// Apply discount rule for given context.
    /// </summary>
    /// <param name="context">Discount application context</param>
    /// <returns>Transaction after applying discount</returns>
    DiscountedTransactionModel ApplyRule(TDiscountApplicationContext context);
}

/// <summary>
/// Discount application rule for small packages.
/// If possible, part of shipment price for small packages should be covered, so that price would 
/// stay the same independent of the shipment carrier.
/// </summary>
public class SmallPackageRule(ITransactionPriceRepository transactionPriceRepository) 
    : IDiscountApplicationRule<SmallPackageDiscountApplicationContext>
{
    /// <inheritdoc/>
    public DiscountedTransactionModel ApplyRule(SmallPackageDiscountApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var lowestPrice = transactionPriceRepository.GetBestPriceForSize(context.Size);
        var currentPrice = transactionPriceRepository.Get(context.Carrier, context.Size);
        var coveredPrice = Math.Min(context.AvailableBudget, currentPrice - lowestPrice);

        return new DiscountedTransactionModel(
            context.Date,
            context.Size,
            context.Carrier,
            currentPrice - coveredPrice,
            coveredPrice
        );
    }
}

/// <summary>
/// Discount application rule for medium packages.
/// There are no discount rules for medium sized packages.
/// </summary>
public class MediumPackageRule(ITransactionPriceRepository transactionPriceRepository) 
: IDiscountApplicationRule<MediumPackageDiscountApplicationContext>
{
    /// <inheritdoc/>
    public DiscountedTransactionModel ApplyRule(MediumPackageDiscountApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var currentPrice = transactionPriceRepository.Get(context.Carrier, context.Size);
        return new DiscountedTransactionModel(context.Date, context.Size, context.Carrier, currentPrice, 0);
    }
}

/// <summary>
/// Discount application rule for large packages.
/// If possible, every month, the third large LaPoste transaction should be fully covered.
/// </summary>
public class LargePackageRule(ITransactionPriceRepository transactionCostRepository) 
: IDiscountApplicationRule<LargePackageDiscountApplicationContext>
{
    /// <inheritdoc/>
    public DiscountedTransactionModel ApplyRule(LargePackageDiscountApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var currentPrice = transactionCostRepository.Get(context.Carrier, context.Size);
        var coveredPrice = 0m;

        if(IsThirdLargeLaPosteTransactionThisMonth(context))
        {
            // This is the third transaction, try to cover the whole cost.
            coveredPrice = Math.Min(context.AvailableBudget, currentPrice);
        }

        return new DiscountedTransactionModel(
            context.Date, 
            context.Size, 
            context.Carrier, 
            currentPrice - coveredPrice,
            coveredPrice
        );
    }

    private static bool IsThirdLargeLaPosteTransactionThisMonth(LargePackageDiscountApplicationContext context)
        => context.MonthlyLargeLaPosteTransactionCountBeforeCurrent is 2
        && context.Size is PackageSize.Large
        && context.Carrier is Carrier.LaPoste;
}
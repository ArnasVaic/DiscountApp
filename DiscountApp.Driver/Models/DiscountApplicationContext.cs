using DiscountApp.Driver.Enums;

namespace DiscountApp.Driver.Models;

/// <summary>
/// Context information required to decide if a discount can be applied for a specific transaction.
/// </summary>
public interface IDiscountApplicationContext
{
    /// <summary>
    /// Current transaction date.
    /// </summary>
    DateOnly Date { get; init; }
    
    /// <summary>
    /// Current transaction shipment carrier.
    /// </summary>
    Carrier Carrier { get; init; }

    /// <summary>
    /// Current transaction package size.
    /// </summary>
    PackageSize Size { get; }

    /// <summary>
    /// Budget remaining for giving out discounts.
    /// </summary>
    decimal AvailableBudget { get; init; }
}

/// <summary>
/// Discount application context for small packages.
/// </summary>
public class SmallPackageDiscountApplicationContext 
    : IDiscountApplicationContext
{
    /// <inheritdoc/>
    public required DateOnly Date { get; init; }

    /// <inheritdoc/>
    public required Carrier Carrier { get; init; }

    /// <inheritdoc/>
    public PackageSize Size { get; } = PackageSize.Small;

    /// <inheritdoc/>
    public required decimal AvailableBudget { get; init; }
}

/// <summary>
/// Discount application context for medium packages.
/// </summary>
public class MediumPackageDiscountApplicationContext 
    : IDiscountApplicationContext
{
    /// <inheritdoc/>
    public required DateOnly Date { get; init; }

    /// <inheritdoc/>
    public required Carrier Carrier { get; init; }

    /// <inheritdoc/>
    public PackageSize Size { get; } = PackageSize.Medium;

    /// <inheritdoc/>
    public required decimal AvailableBudget { get; init; }
}

public class LargePackageDiscountApplicationContext 
    : IDiscountApplicationContext
{
    /// <inheritdoc/>
    public required DateOnly Date { get; init; }

    /// <inheritdoc/>
    public required Carrier Carrier { get; init; }

    /// <inheritdoc/>
    public PackageSize Size { get; } = PackageSize.Large;

    /// <inheritdoc/>
    public required decimal AvailableBudget { get; init; }

    /// <summary>
    /// Number of large transactions that were shipped by LaPoste from the beginning
    /// of this month to the current transaction (non-inclusive).
    /// </summary>
    public required int MonthlyLargeLaPosteTransactionCountBeforeCurrent { get; init; }
}

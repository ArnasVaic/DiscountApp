using DiscountApp.Driver.Enums;

namespace DiscountApp.Driver.Repositories;

/// <summary>
/// Repository for accessing transaction prices.
/// </summary>
public interface ITransactionPriceRepository
{
    /// <summary>
    /// Get price based on the package size and the shipment provider.
    /// </summary>
    /// <param name="carrier">Shipment carrier</param>
    /// <param name="size">Package size</param>
    /// <returns>Transaction Price</returns>
    decimal Get(Carrier carrier, PackageSize size);

    /// <summary>
    /// Get best price amongst competitors for a package of specific size.
    /// </summary>
    /// <param name="size">Package size</param>
    /// <returns></returns>
    decimal GetBestPriceForSize(PackageSize size);
}

public class HardcodedTransactionPriceRepository : ITransactionPriceRepository
{
    /// <inheritdoc/>
    public decimal Get(Carrier provider, PackageSize size) => (provider, size) switch
    {
        (Carrier.LaPoste, PackageSize.Small) => 1.50m,
        (Carrier.LaPoste, PackageSize.Medium) => 4.90m,
        (Carrier.LaPoste, PackageSize.Large) => 6.90m,
        (Carrier.MondialRelay, PackageSize.Small) => 2.00m,
        (Carrier.MondialRelay, PackageSize.Medium) => 3.00m,
        (Carrier.MondialRelay, PackageSize.Large) => 4.00m,
        (_, _) => throw new ArgumentOutOfRangeException($"({provider}, {size})")
    };

    /// <inheritdoc/>
    public decimal GetBestPriceForSize(PackageSize size) => Enum
        .GetValues<Carrier>()
        .Select(p => Get(p, size))
        .Min();
}
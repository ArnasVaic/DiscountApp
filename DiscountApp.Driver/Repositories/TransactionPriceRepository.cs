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
    /// <param name="provider">Shipment provider</param>
    /// <param name="size">Package size</param>
    /// <returns>Transaction Price</returns>
    decimal Get(Provider provider, PackageSize size);

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
    public decimal Get(Provider provider, PackageSize size) => (provider, size) switch
    {
        (Provider.LaPoste, PackageSize.Small) => 1.50m,
        (Provider.LaPoste, PackageSize.Medium) => 4.90m,
        (Provider.LaPoste, PackageSize.Large) => 6.90m,
        (Provider.MondialRelay, PackageSize.Small) => 2.00m,
        (Provider.MondialRelay, PackageSize.Medium) => 3.00m,
        (Provider.MondialRelay, PackageSize.Large) => 4.00m,
        (_, _) => throw new ArgumentOutOfRangeException($"({provider}, {size})")
    };

    /// <inheritdoc/>
    public decimal GetBestPriceForSize(PackageSize size) => Enum
        .GetValues<Provider>()
        .Select(p => Get(p, size))
        .Min();
}
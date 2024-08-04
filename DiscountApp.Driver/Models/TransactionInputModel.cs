using DiscountApp.Driver.Enums;

namespace DiscountApp.Driver.Models;

/// <summary>
/// Structure to hold transaction data extract from the input file.
/// </summary>
/// <param name="Date">Date of the transaction</param>
/// <param name="Size">Package size</param>
/// <param name="Carrier">Shipping carrier</param>
public record TransactionInputModel(DateOnly Date, PackageSize Size, Carrier Carrier);
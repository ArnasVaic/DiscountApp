using DiscountApp.Driver.Enums;

namespace DiscountApp.Driver.Models;

/// <summary>
/// Structure to hold transaction data extract from the input file.
/// </summary>
/// <param name="Date">Date of the transaction</param>
/// <param name="Size">Package size</param>
/// <param name="Provider">Shipping provider</param>
public record TransactionInputModel(DateOnly Date, PackageSize Size, Provider Provider);
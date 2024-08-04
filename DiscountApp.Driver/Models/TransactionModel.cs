using DiscountApp.Driver.Enums;

namespace DiscountApp.Driver.Models;

/// <summary>
/// Transaction data including id which gives information about the chronological order of the transaction.
/// </summary>
/// <param name="id">Id of the transaction</param>
/// <param name="Date">Date of the transaction</param>
/// <param name="Size">Package size</param>
/// <param name="Provider">Shipping provider</param>
public record TransactionModel(int Id, DateOnly Date, PackageSize Size, Provider Provider);
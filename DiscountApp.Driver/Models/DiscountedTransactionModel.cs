using DiscountApp.Driver.Enums;

namespace DiscountApp.Driver.Models;

/// <summary>
/// Model for transaction that has an applied discount.
/// </summary>
/// <param name="Date">Date of the transaction</param>
/// <param name="Size">Package size</param>
/// <param name="Provider">Shipment provider</param>
/// <param name="ReducedCost">Cost after discount</param>
/// <param name="Discount">discount</param>
public record DiscountedTransactionModel(
    DateOnly Date, 
    PackageSize Size, 
    Provider Provider, 
    decimal ReducedCost, 
    decimal Discount);
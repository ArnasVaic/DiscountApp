using DiscountApp.Driver.Enums;
using DiscountApp.Driver.Extensions;

namespace DiscountApp.Driver.Models;

/// <summary>
/// Model for transaction that has an applied discount.
/// </summary>
/// <param name="Date">Date of the transaction</param>
/// <param name="Size">Package size</param>
/// <param name="Provider">Shipment provider</param>
/// <param name="DiscountedPrice">Price after discount</param>
/// <param name="Discount">discount</param>
public record DiscountedTransactionModel(
    DateOnly Date, 
    PackageSize Size, 
    Carrier Provider, 
    decimal DiscountedPrice, 
    decimal Discount)
{
    public override string ToString()
    {
        var size = Size.GetDisplayNameOrDefault();
        var provider = Provider.GetDisplayNameOrDefault();
        var discountText = Discount is 0 ? "-" : $"{Discount:0.00}";
        return $"{Date:yyyy-MM-dd} {size} {provider} {DiscountedPrice:0.00} {discountText}";
    }
}
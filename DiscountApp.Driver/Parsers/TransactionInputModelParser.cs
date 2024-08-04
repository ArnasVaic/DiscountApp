using DiscountApp.Driver.Core;
using DiscountApp.Driver.Enums;
using DiscountApp.Driver.Models;

namespace DiscountApp.Driver.Parsers;

/// <summary>
/// Parser for transaction input model.
/// </summary>
public static class TransactionInputModelParser
{
    /// <summary>
    /// Parse transaction input model.
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>Result representing success or failure</returns>
    public static Result<TransactionInputModel> Parse(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var tokens = input.Split(' ');

        if(tokens.Length is not 3)
        {
            return input;
        }

        if(!DateOnly.TryParse(tokens[0], out var date))
        {
            return input;
        }

        if(!EnumParsingUtilities.TryParseByDisplayName<PackageSize>(tokens[1], out var size))
        {
            return input;
        }

        if(!EnumParsingUtilities.TryParseByDisplayName<Provider>(tokens[2], out var provider))
        {
            return input;
        }

        return new TransactionInputModel(date, size, provider);
    }
}

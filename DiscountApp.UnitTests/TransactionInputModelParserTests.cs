using DiscountApp.Driver.Enums;
using DiscountApp.Driver.Parsers;

namespace DiscountApp.UnitTests;

public class TransactionInputModelParserTests
{
    [Fact]
    public void Parse_EmptyString_Fails()
    {
        // ARRANGE
        var input = string.Empty;
        // ACT
        var result = TransactionInputModelParser.Parse(input);
        // ASSERT
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Parse_InputOk_Success()
    {
        // ARRANGE
        var input = "2024-01-12 L LP";
        // ACT
        var result = TransactionInputModelParser.Parse(input);
        // ASSERT
        Assert.True(result.IsSuccess);
        var transaction = result.Value!;
        Assert.Equal(PackageSize.Large, transaction.Size);
        Assert.Equal(Carrier.LaPoste, transaction.Carrier);
        Assert.Equal(2024, transaction.Date.Year);
        Assert.Equal(1, transaction.Date.Month);
        Assert.Equal(12, transaction.Date.Day);
    }
}
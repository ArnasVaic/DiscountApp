
using Vinted;

if (args.Length is not 1 || args[0] is "--help")
{
    Console.Error.WriteLine("usage: vinted-hw <file>");
    return;
}

var filename = args[0];

// var filename = "input.txt";

try {
    var lines = await File.ReadAllLinesAsync(filename);
    var parseResults = lines.Select(TransactionInputParser.Parse);
    var discountCalculationService = new DiscountCalculationService();
    var discountResult = discountCalculationService.CalculateDiscounts(parseResults);
}
catch(Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
}
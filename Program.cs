
using Vinted;

// if (args.Length is not 1 || args[0] is "--help")
// {
//     Console.Error.WriteLine("usage: vinted-hw <file>");
//     return;
// }

// var filename = args[0];

var filename = "input.txt";

try {
    var lines = await File.ReadAllLinesAsync(filename);
    var parseResults = lines.Select(TransactionInputParser.Parse);

    // Poor man's dependency injection
    var transactionPriceRepository = new TransactionPriceRepository();
    var discountCalculationService = new DiscountCalculationService(
        new SmallPackageSizeDiscountRule(transactionPriceRepository),
        new MediumPackageSizeDiscountRule(transactionPriceRepository),
        new LargePackageSizeDiscountRule(transactionPriceRepository),
        new DiscountRuleApplicationContextBuilder()
    );
    var discountedTransactionResults = discountCalculationService.CalculateDiscounts(parseResults);

    foreach(var dtr in discountedTransactionResults)
    {
        if(dtr.IsSuccess)
        {
            var t = dtr.Value;
            var discount = t.Discount is 0 ? "-" : $"{t.Discount:0.00}";
            Console.WriteLine($"{t.Date.ToString("yyyy-MM-dd")} {t.Size.GetDisplayNameOrDefault()} {t.Provider.GetDisplayNameOrDefault()} {t.ReducedCost:0.00} {discount}");
        }
        else
        {
            Console.WriteLine($"{dtr.Error} Ignored");
        }
    }

}
catch(Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
}
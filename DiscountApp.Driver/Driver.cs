using DiscountApp.Driver.Core;
using DiscountApp.Driver.Parsers;
using DiscountApp.Driver.Repositories;
using DiscountApp.Driver.Rules;
using DiscountApp.Driver.Services;

namespace DiscountApp.Driver;

/// <summary>
/// Main application driver.
/// </summary>
public class Driver(
    IFileReadingService fileReadingService,
    IDiscountApplicationService discountApplicationService, 
    IDiscountedTransactionOutputService discountedTransactionOutputService,
    string filename)
{
    /// <summary>
    /// Usage text.
    /// </summary>
    public const string UsageText = "usage:\n\tvinted-hw <file>\nor if running via dotnet cli:\n\tdotnet run -- <file>";

    /// <summary>
    /// Try to create application driver from given args, can fail if args are invalid.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Result<Driver> WithArgs(string[] args)
    {
        if (args.Length is not 1 || args[0] is "--help")
        {
            return UsageText;
        }

        // Poor man's DI
        var fileReadingService = new FileReadingService();
        var transactionPriceRepository = new HardcodedTransactionPriceRepository();
        var discountApplicationService = new DiscountApplicationService(
            new SmallPackageRule(transactionPriceRepository),
            new MediumPackageRule(transactionPriceRepository),
            new LargePackageRule(transactionPriceRepository),
            new DiscountRuleApplicationContextBuilder()
        );
        var discountedTransactionOutputService = new DiscountedTransactionConsoleOutputService();

        return new Driver(
            fileReadingService,
            discountApplicationService,
            discountedTransactionOutputService,
            args[0]);
    }

    /// <summary>
    /// Run the application.
    /// </summary>
    /// <returns>Result</returns>
    public Result Run()
    {
        var linesResult = fileReadingService.ReadAllLines(filename);

        // Bind could be used here, but not everything has to be functional.
        if (linesResult.IsFailure)
            return linesResult.Error!;

        var transactionParsingResults = linesResult.Value!.Select(TransactionInputModelParser.Parse);
        var discountedTransactions = discountApplicationService.Apply(transactionParsingResults);
        discountedTransactionOutputService.Write(discountedTransactions);

        return Result.Ok();
    }
}
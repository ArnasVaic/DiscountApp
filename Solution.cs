using System.ComponentModel;
using System.Reflection;

namespace Vinted;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class AcceptedNamesAttribute(params string[] names) : Attribute
{
    public IEnumerable<string> Names { get; } = names;
}

public enum Provider
{       
    [AcceptedNames("LP")]
    LaPoste,
    
    [AcceptedNames("MR")]
    MondialRelay
}

public enum PackageSize
{
    [AcceptedNames("S")]
    Small,
    [AcceptedNames("M")]
    Medium,
    [AcceptedNames("L")]
    Large
}

public record TransactionInputModel(DateOnly Date, PackageSize Size, Provider Provider);

public static class EnumParsingUtilities
{
    public static bool TryParseByAcceptedNames<TEnum>(string input, out TEnum result) where TEnum : struct
    {
        result = default;
        var fields = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static);
        var fieldAcceptedNamesLookup = fields.ToDictionary(
            field => Enum.Parse<TEnum>(field.Name),
            field => field
                .GetCustomAttributes<AcceptedNamesAttribute>()
                .SelectMany(attr => attr.Names)
        );

        Dictionary<string, TEnum> enumValueLookup = new();

        foreach(var (enumValue, acceptedNames) in fieldAcceptedNamesLookup)
        {
            if(acceptedNames.Any(enumValueLookup.ContainsKey))
            {
                throw new Exception("Different enum values cannot have the same accepted name.");
            }

            foreach (var name in acceptedNames)
                enumValueLookup.Add(name, enumValue);
        }

        if (!enumValueLookup.Any())
        {
            return false;
        }

        return enumValueLookup.TryGetValue(input, out result);
    }
}

public readonly struct Result<T>
{
    public readonly T? Value { get; }
    public readonly string? Error { get; }

    public readonly bool IsFailure => Error is not null;
    public readonly bool IsSuccess => Value is not null;

    private Result(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    public Result<TResult> Select<TResult>(Func<T, TResult> selector) => Value switch
    {
        null => Error,
        _ => selector(Value)
    };

    public static implicit operator Result<T>(string error) => new(default, error);
    public static implicit operator Result<T>(T value) => new(value, default);
}

public static class TransactionInputParser
{
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

        if(!EnumParsingUtilities.TryParseByAcceptedNames<PackageSize>(tokens[1], out var size))
        {
            return input;
        }

        if(!EnumParsingUtilities.TryParseByAcceptedNames<Provider>(tokens[2], out var provider))
        {
            return input;
        }

        return new TransactionInputModel(date, size, provider);
    }
}

public class TransactionPriceRepository
{
    public decimal Get(Provider provider, PackageSize size) => (provider, size) switch
    {
        (Provider.LaPoste, PackageSize.Small) => 1.50m,
        (Provider.LaPoste, PackageSize.Medium) => 4.90m,
        (Provider.LaPoste, PackageSize.Large) => 6.90m,
        (Provider.MondialRelay, PackageSize.Small) => 2m,
        (Provider.MondialRelay, PackageSize.Medium) => 3m,
        (Provider.MondialRelay, PackageSize.Large) => 4m,
        (_, _) => throw new ArgumentOutOfRangeException($"({provider}, {size})")
    };

    public decimal GetBestPriceForSize(PackageSize size) => Enum
        .GetValues<Provider>()
        .Select(p => Get(p, size))
        .Min();
}

public record DiscountedTransactionModel(
    DateOnly Date, 
    PackageSize Size, 
    Provider Provider, 
    decimal ReducedCost, 
    decimal Discount);

public interface IDiscountRuleApplicationContext
{
    DateOnly Date { get; init; }
    PackageSize PackageSize { get; init; }
    Provider Provider { get; init; }
    decimal AvailableBudget { get; init; }
}

public class SmallPackageRuleApplicationContext : IDiscountRuleApplicationContext
{
    public decimal AvailableBudget { get; init; }
    public DateOnly Date { get; init; }
    public PackageSize PackageSize { get; init; }
    public Provider Provider { get; init; }
}

public class LargePackageRuleApplicationContext : IDiscountRuleApplicationContext
{
    public DateOnly Date { get; init; }
    public PackageSize PackageSize { get; init; }
    public Provider Provider { get; init; }
    public decimal AvailableBudget { get; init; }
    public bool IsThirdTransactionWithLaPosteProviderThisMonth { get; init; }
}

public interface IDiscountRule<TDiscountRuleApplicationContext> where TDiscountRuleApplicationContext : IDiscountRuleApplicationContext
{
    DiscountedTransactionModel ApplyRule(TDiscountRuleApplicationContext context);
}

public class SmallPackageSizeDiscountRule(TransactionPriceRepository transactionCostRepository) : IDiscountRule<SmallPackageRuleApplicationContext>
{
    public DiscountedTransactionModel ApplyRule(TransactionInputModel transaction, SmallPackageRuleApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(context);

        if(transaction.Size is not PackageSize.Small)
        {
            // Sanity check
            throw new InvalidOperationException("Small package discount rule can only accept small package transactions");
        }

        var lowestPrice = transactionCostRepository.GetBestPriceForSize(transaction.Size);
        var currentPrice = transactionCostRepository.Get(transaction.Provider, transaction.Size);
        var coveredPrice = Math.Min(context.AvailableBudget, currentPrice - lowestPrice);

        return new DiscountedTransactionModel(
            transaction.Date,
            transaction.Size,
            transaction.Provider,
            currentPrice - coveredPrice,
            coveredPrice
        );
    }
}

public interface IDiscountRuleApplicationContextBuilder
{
    IDiscountRuleApplicationContext Build( 
        int transactionIndex,
        decimal availableBudget,
        IEnumerable<Result<TransactionInputModel>> transactionResults);
}

public class DiscountRuleApplicationContextBuilder : IDiscountRuleApplicationContextBuilder
{
    public IDiscountRuleApplicationContext Build(
        int transactionIndex,
        decimal availableBudget,
        IEnumerable<Result<TransactionInputModel>> transactionResults) => transaction.Size switch
    {
        PackageSize.Small => new SmallPackageRuleApplicationContext { AvailableBudget = availableBudget },
        PackageSize.Large => new LargePackageRuleApplicationContext
        {
            AvailableBudget = availableBudget,
            IsThirdTransactionWithLaPosteProviderThisMonth = transactionResults
                .Where(result => result.IsSuccess)
                .Select(result => result.Value)
                .Where(transaction => transaction.Date.Month == transaction.Date.Month)
                .
                
        }
    }

    private LargePackageRuleApplicationContext BuildLargePackageRuleApplicationContext(
        int transactionIndex,
        decimal availableBudget,
        IEnumerable<Result<TransactionInputModel>> transactionResults
    )
    {
        var isThirdAndLP = 

        return new LargePackageRuleApplicationContext
        {
            AvailableBudget = availableBudget,
            IsThirdTransactionWithLaPosteProviderThisMonth = transactionResults
                .Where(result => result.IsSuccess)
                .Select(result => result.Value)
                .Where(transaction => transaction.Date.Month == transaction.Date.Month)
                .
        };
    }
}

public class DiscountCalculationService
{
    private readonly IDiscountRule<SmallPackageRuleApplicationContext> _smallPackageDiscountRule;
    private readonly IDiscountRule<LargePackageRuleApplicationContext> _largePackageDiscountRule;
    private readonly IDiscountRuleApplicationContextBuilder _discountRuleApplicationContextBuilder;

    public DiscountCalculationService()
    {

    }

    public IEnumerable<Result<DiscountedTransactionModel>> CalculateDiscounts(IEnumerable<Result<TransactionInputModel>> transactionResults)
    {
        var monthDiscountBudgetLookup = transactionResults
            .Where(result => result.IsSuccess)
            .Select(transaction => transaction.Value!.Date.Month)
            .ToDictionary(month => month, _ => 10m);

        return transactionResults.Select((result, id) => 
            result.Select(transaction =>
            {
                var availableBudget = monthDiscountBudgetLookup[transaction.Date.Month];
                var context = _discountRuleApplicationContextBuilder.Build(id, availableBudget, transactionResults);
                var discountedTransaction = transaction.Size switch
                {
                    PackageSize.Small => _smallPackageDiscountRule.ApplyRule(context as SmallPackageRuleApplicationContext),
                    PackageSize.Large => _largePackageDiscountRule.ApplyRule(context as LargePackageRuleApplicationContext),
                    _ => throw new ArgumentOutOfRangeException(nameof(transaction.Size))
                };

                monthDiscountBudgetLookup[transaction.Date.Month] -= discountedTransaction.Discount;

                return discountedTransaction;
            })
        );
    }
}
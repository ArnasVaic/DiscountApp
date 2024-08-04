using System.Reflection;

namespace Vinted;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class DisplayNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

public enum Provider
{       
    [DisplayName("LP")]
    LaPoste,
    
    [DisplayName("MR")]
    MondialRelay
}

public enum PackageSize
{
    [DisplayName("S")]
    Small,
    [DisplayName("M")]
    Medium,
    [DisplayName("L")]
    Large
}

public static class EnumExtensions
{
    public static string? GetDisplayNameOrDefault<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        var valueName = Enum.GetName(value);

        if(valueName is null)
        {
            return default;
        }

        var field = typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .First(field => field.Name == valueName);

        var attribute = field.GetCustomAttribute<DisplayNameAttribute>();

        if(attribute is null)
        {
            return default;
        }

        return attribute.Name;
    }
}

public record TransactionInputModel(DateOnly Date, PackageSize Size, Provider Provider);

public static class EnumParsingUtilities
{
    /// <summary>
    /// Tries to parse enum from string by display name or by value names if display name is not set.
    /// </summary>
    /// <typeparam name="TEnum">Type of enum</typeparam>
    /// <param name="input">Input string</param>
    /// <param name="result">Result enum value</param>
    /// <returns>Boolean flag indicating success or failure</returns>
    /// <exception cref="Exception"></exception>
    public static bool TryParseByDisplayName<TEnum>(string input, out TEnum result) where TEnum : struct
    {
        result = default;
        var fields = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static);
        var fieldDisplayNameLookup = fields.ToDictionary(
            field => Enum.Parse<TEnum>(field.Name),
            field => field.GetCustomAttribute<DisplayNameAttribute>()?.Name ?? field.Name
        );

        Dictionary<string, TEnum> enumValueLookup = [];

        foreach(var (enumValue, displayName) in fieldDisplayNameLookup)
        {
            if(enumValueLookup.ContainsKey(displayName))
            {
                throw new Exception("Different enum values cannot have the same display name.");
            }

            enumValueLookup.Add(displayName, enumValue);
        }

        if (enumValueLookup.Count is 0)
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
    PackageSize Size { get; init; }
    Provider Provider { get; init; }
    decimal AvailableBudget { get; init; }
}

public class SmallPackageRuleApplicationContext 
    : IDiscountRuleApplicationContext
{
    public required decimal AvailableBudget { get; init; }
    public required DateOnly Date { get; init; }
    public required PackageSize Size { get; init; }
    public required Provider Provider { get; init; }
}

public class MediumPackageRuleApplicationContext 
    : IDiscountRuleApplicationContext
{
    public required DateOnly Date { get; init; }
    public required PackageSize Size { get; init; }
    public required Provider Provider { get; init; }
    public required decimal AvailableBudget { get; init; }
}

public class LargePackageRuleApplicationContext 
    : IDiscountRuleApplicationContext
{
    public required DateOnly Date { get; init; }
    public required PackageSize Size { get; init; }
    public required Provider Provider { get; init; }
    public required decimal AvailableBudget { get; init; }
    public required int LaPosteTransactionsThisMonth { get; init; }
}

public interface IDiscountRule<TDiscountRuleApplicationContext> 
where TDiscountRuleApplicationContext 
    : IDiscountRuleApplicationContext
{
    DiscountedTransactionModel ApplyRule(TDiscountRuleApplicationContext context);
}

public class SmallPackageSizeDiscountRule(TransactionPriceRepository transactionCostRepository) 
: IDiscountRule<SmallPackageRuleApplicationContext>
{
    public DiscountedTransactionModel ApplyRule(SmallPackageRuleApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if(context.Size is not PackageSize.Small)
        {
            // Sanity check
            throw new InvalidOperationException("Small package discount rule can only accept small package transactions");
        }

        var lowestPrice = transactionCostRepository.GetBestPriceForSize(context.Size);
        var currentPrice = transactionCostRepository.Get(context.Provider, context.Size);
        var coveredPrice = Math.Min(context.AvailableBudget, currentPrice - lowestPrice);

        return new DiscountedTransactionModel(
            context.Date,
            context.Size,
            context.Provider,
            currentPrice - coveredPrice,
            coveredPrice
        );
    }
}

public class MediumPackageSizeDiscountRule(TransactionPriceRepository transactionCostRepository) 
: IDiscountRule<MediumPackageRuleApplicationContext>
{
    public DiscountedTransactionModel ApplyRule(MediumPackageRuleApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if(context.Size is not PackageSize.Medium)
        {
            // Sanity check
            throw new InvalidOperationException("Medium package discount rule can only accept medium package transactions");
        }

        var currentPrice = transactionCostRepository.Get(context.Provider, context.Size);

        return new DiscountedTransactionModel(
            context.Date,
            context.Size,
            context.Provider,
            currentPrice,
            0
        );
    }
}

public class LargePackageSizeDiscountRule(TransactionPriceRepository transactionCostRepository) 
: IDiscountRule<LargePackageRuleApplicationContext>
{
    public DiscountedTransactionModel ApplyRule(LargePackageRuleApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if(context.Size is not PackageSize.Large)
        {
            // Sanity check
            throw new InvalidOperationException("Large package discount rule can only accept large package transactions");
        }

        var currentPrice = transactionCostRepository.Get(context.Provider, context.Size);
        var coveredPrice = 0m;

        if(context.LaPosteTransactionsThisMonth is 2)
        {
            // This is the third transaction, try to cover the whole cost
            coveredPrice = Math.Min(context.AvailableBudget, currentPrice);
        }

        return new DiscountedTransactionModel(
            context.Date,
            context.Size,
            context.Provider,
            currentPrice - coveredPrice,
            coveredPrice
        );
    }
}


public interface IDiscountRuleApplicationContextBuilder
{
    IDiscountRuleApplicationContext Build( 
        int transactionIndex,
        TransactionInputModel transaction,
        decimal availableBudget,
        IEnumerable<Result<TransactionInputModel>> transactionResults);
}

public class DiscountRuleApplicationContextBuilder : IDiscountRuleApplicationContextBuilder
{
    public IDiscountRuleApplicationContext Build(
        int transactionIndex,
        TransactionInputModel transaction,
        decimal availableBudget,
        IEnumerable<Result<TransactionInputModel>> transactionResults) => transaction.Size switch
        {
            PackageSize.Small => new SmallPackageRuleApplicationContext
            {
                Size = transaction.Size,
                Provider = transaction.Provider,
                Date = transaction.Date,
                AvailableBudget = availableBudget
            },
            PackageSize.Large => new LargePackageRuleApplicationContext
            {
                Size = transaction.Size,
                Provider = transaction.Provider,
                Date = transaction.Date,
                AvailableBudget = availableBudget,
                LaPosteTransactionsThisMonth = transactionResults
                    .Where(result => result.IsSuccess)
                    .Select(result => result.Value!)
                    .Where(tr => tr.Date.Month == transaction.Date.Month)
                    .Where(tr => tr.Provider is Provider.LaPoste)
                    .Count()
            },
            PackageSize.Medium => new MediumPackageRuleApplicationContext
            {
                Size = transaction.Size,
                Provider = transaction.Provider,
                Date = transaction.Date,
                AvailableBudget = availableBudget,
            }
        };
}

public class DiscountCalculationService(
    IDiscountRule<SmallPackageRuleApplicationContext> smallPackageDiscountRule,
    IDiscountRule<MediumPackageRuleApplicationContext> mediumPackageDiscountRule,
    IDiscountRule<LargePackageRuleApplicationContext> largePackageDiscountRule,
    IDiscountRuleApplicationContextBuilder discountRuleApplicationContextBuilder)
{

    public IEnumerable<Result<DiscountedTransactionModel>> CalculateDiscounts(IEnumerable<Result<TransactionInputModel>> transactionResults)
    {
        var monthDiscountBudgetLookup = transactionResults
            .Where(result => result.IsSuccess)
            .Select(transaction => transaction.Value!.Date.Month)
            .Distinct()
            .ToDictionary(month => month, _ => 10m);

        return transactionResults.Select((result, id) => 
            result.Select(transaction =>
            {
                var availableBudget = monthDiscountBudgetLookup[transaction.Date.Month];
                var context = discountRuleApplicationContextBuilder.Build(id, transaction, availableBudget, transactionResults);
                var discountedTransaction = transaction.Size switch
                {
                    PackageSize.Small => smallPackageDiscountRule.ApplyRule(context as SmallPackageRuleApplicationContext),
                    PackageSize.Medium => mediumPackageDiscountRule.ApplyRule(context as MediumPackageRuleApplicationContext),
                    PackageSize.Large => largePackageDiscountRule.ApplyRule(context as LargePackageRuleApplicationContext),
                    _ => throw new ArgumentOutOfRangeException(nameof(transaction.Size))
                };

                monthDiscountBudgetLookup[transaction.Date.Month] -= discountedTransaction.Discount;

                return discountedTransaction;
            })
        );
    }
}
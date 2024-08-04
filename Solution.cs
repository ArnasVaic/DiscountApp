using System.Reflection;
using System.Runtime.CompilerServices;

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

public record TransactionInputModel(DateOnly date, PackageSize Size, Provider provider);

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

public readonly struct Result<TResult>
{
    TResult? Value { get; }
    string Error { get; }

    
}

public static class TransactionInputParser
{
    public static TransactionInputModel? ParseOrDefault(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var tokens = input.Split(' ');

        if(tokens.Length is not 3)
        {
            return default;
        }

        if(!DateOnly.TryParse(tokens[0], out var date))
        {
            return default;
        }


        if(!EnumParsingUtilities.TryParseByAcceptedNames<Provider>(tokens[1], out var provider))
        {
            return default;
        }

        if(!EnumParsingUtilities.TryParseByAcceptedNames<PackageSize>(tokens[1], out var size))
        {
            return default;
        }

        return new(date, size, provider);
    }
}

public static class TransactionInputFileParser
{
    public static IEnumerable<TransactionInputModel?> ParseInputFile()
}
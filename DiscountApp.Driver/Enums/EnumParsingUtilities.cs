using System.Reflection;
using DiscountApp.Driver.Attributes;

namespace DiscountApp.Driver.Enums;

/// <summary>
/// Generic enum parsing utilities.
/// </summary>
public static class EnumParsingUtilities
{
    /// <summary>
    /// Tries to parse enum from string by display name or by value names if display name is not set.
    /// </summary>
    /// <typeparam name="TEnum">Type of enum</typeparam>
    /// <param name="input">Input string</param>
    /// <param name="result">Result enum value</param>
    /// <returns>Boolean flag indicating success or failure</returns>
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
            if(!enumValueLookup.ContainsKey(displayName))
            {
                enumValueLookup.Add(displayName, enumValue);
            }
        }

        if (enumValueLookup.Count is 0)
        {
            return false;
        }

        return enumValueLookup.TryGetValue(input, out result);
    }
}
using System.Reflection;
using DiscountApp.Driver.Attributes;

namespace DiscountApp.Driver.Extensions;

/// <summary>
/// Extensions for enums.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Get display name or null of a specific enum value.
    /// </summary>
    /// <typeparam name="TEnum">Type of enum</typeparam>
    /// <param name="value">Enum value</param>
    /// <returns>Enum display name or null</returns>
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
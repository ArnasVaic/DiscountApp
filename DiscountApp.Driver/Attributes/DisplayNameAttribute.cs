namespace DiscountApp.Driver.Attributes;

/// <summary>
/// Provide enum values with display names that can be used for parsing.
/// </summary>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class DisplayNameAttribute(string name) : Attribute
{
    /// <summary>
    /// Display name to be for this enum value.
    /// </summary>
    public string Name { get; } = name;
}
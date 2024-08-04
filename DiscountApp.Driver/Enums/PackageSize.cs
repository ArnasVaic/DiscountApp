using DiscountApp.Driver.Attributes;

namespace DiscountApp.Driver.Enums;

/// <summary>
/// Type of package size. 
/// </summary>
public enum PackageSize
{
    [DisplayName("S")]
    Small,

    [DisplayName("M")]
    Medium,

    [DisplayName("L")]
    Large
}
using DiscountApp.Driver.Attributes;

namespace DiscountApp.Driver.Enums;

/// <summary>
/// Type of provider.
/// </summary>
public enum Provider
{       
    [DisplayName("LP")]
    LaPoste,
    
    [DisplayName("MR")]
    MondialRelay
}
using DiscountApp.Driver.Attributes;

namespace DiscountApp.Driver.Enums;

/// <summary>
/// Type of carrier.
/// </summary>
public enum Carrier
{       
    [DisplayName("LP")]
    LaPoste,
    
    [DisplayName("MR")]
    MondialRelay
}
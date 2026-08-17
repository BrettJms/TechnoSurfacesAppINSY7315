namespace TechnoSurfacesApp.Models;

/// <summary>Two roles only, per the client brief. The MD self-approves; estimators do not.</summary>
public enum UserRole
{
    ManagingDirector,
    Estimator
}

/// <summary>
/// Quote lifecycle. Note there is deliberately NO Rejected state - the client
/// confirmed the MD corrects an estimator's quote and approves it rather than
/// sending it back.
/// </summary>
public enum QuoteStatus
{
    Draft,
    PendingApproval,
    Approved,
    Sent,
    Accepted,
    Invoiced
}

/// <summary>
/// Catalogue entries are never hard-deleted - historic quotes must keep resolving.
/// Max on Top flag phased-out items on their price list.
/// </summary>
public enum CatalogueStatus
{
    Active,
    PhasingOut,
    Discontinued
}

/// <summary>
/// Three of five suppliers price by colour band; two price each item individually.
/// Neither is the general case, so the catalogue supports both.
/// </summary>
public enum PricingStructure
{
    Band,
    Item
}

/// <summary>
/// Sections of the costing sheet. BelowTheLine items are cost recovery and are
/// NOT marked up - confirmed by the client.
/// </summary>
public enum RateGroup
{
    Fabrication,
    Consumables,
    Installation,
    WoodSubstrate,
    SinksHardware,
    BelowTheLine
}

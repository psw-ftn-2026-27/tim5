namespace Exploration.Contracts;

/// <summary>
/// Fixed IDs of the demo tours that the Exploration module seeds in development, so that
/// other modules' development seeds can refer to them by ID (e.g. Payment's products).
/// Every other tour gets a random ID from its constructor.
/// </summary>
public static class WellKnownTours
{
    public static readonly Guid PetrovaradinFortressWalk = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid FruskaGoraBikeRide = Guid.Parse("30000000-0000-0000-0000-000000000002");
    public static readonly Guid CityCenterWalk = Guid.Parse("30000000-0000-0000-0000-000000000003");
}

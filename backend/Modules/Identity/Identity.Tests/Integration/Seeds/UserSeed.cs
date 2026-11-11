using Identity.Core;
using Microsoft.AspNetCore.Identity;
using Shared.Tests;

namespace Identity.Tests.Integration.Seeds;

/// <summary>
/// Unlike feature-module seeds, these rows cannot come from a domain constructor:
/// Reseed inserts them straight into the database, bypassing UserManager, so the
/// normalized columns and the password hash must be filled in here.
/// </summary>
internal static class UserSeed
{
    public const string Password = "SuperSecret1!";

    private static readonly PasswordHasher<ApplicationUser> Hasher = new();

    public static readonly ApplicationUser Explorer = new()
    {
        Id = WellKnownUsers.Explorer,
        UserName = "explorer",
        NormalizedUserName = "EXPLORER",
        Email = "explorer@test.com",
        NormalizedEmail = "EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "5f4a2f3e-8b1c-4d6e-9a0b-7c2d1e3f4a5b"
    };

    public static readonly IdentityUserRole<Guid> ExplorerRole = new() { UserId = WellKnownUsers.Explorer, RoleId = RoleSeed.Explorer.Id };

    public static readonly ApplicationUser SecondExplorer = new()
    {
        Id = WellKnownUsers.SecondExplorer,
        UserName = "secondexplorer",
        NormalizedUserName = "SECONDEXPLORER",
        Email = "secondexplorer@test.com",
        NormalizedEmail = "SECONDEXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "8c1e7d2a-3f4b-4a5c-b6d7-e8f9a0b1c2d3"
    };

    public static readonly IdentityUserRole<Guid> SecondExplorerRole = new() { UserId = WellKnownUsers.SecondExplorer, RoleId = RoleSeed.Explorer.Id };

    public static readonly ApplicationUser ExplorerWithDeposit = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0002-000000000002"),
        UserName = "depositedexplorer",
        NormalizedUserName = "DEPOSITEDEXPLORER",
        Email = "depositedexplorer@test.com",
        NormalizedEmail = "DEPOSITEDEXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "2b9d4e6f-1a3c-4e5b-8d7f-0c2e4a6b8d1f"
    };

    public static readonly IdentityUserRole<Guid> ExplorerWithDepositRole = new() { UserId = ExplorerWithDeposit.Id, RoleId = RoleSeed.Explorer.Id };

    public static object[] All => [Explorer, ExplorerRole, SecondExplorer, SecondExplorerRole, ExplorerWithDeposit, ExplorerWithDepositRole];
}

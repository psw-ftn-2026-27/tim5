using Shared.Tests;
using Social.Domain.Connections;

namespace Social.Tests.Integration.Seeds;

internal static class ConnectionSeed
{
    public static readonly Connection ExplorerRequestedAdministrator = new(WellKnownUsers.Explorer, WellKnownUsers.Administrator);
    public static readonly Connection SecondExplorerRequestedAdministrator = new(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);
    public static readonly Connection NewcomerRequestedExplorer = new(Guid.NewGuid(), WellKnownUsers.Explorer);
    public static readonly Connection SecondExplorerFollowsExplorer;

    static ConnectionSeed()
    {
        SecondExplorerFollowsExplorer = new(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer);
        SecondExplorerFollowsExplorer.Accept(WellKnownUsers.Explorer);
    }

    public static Connection[] All => [ExplorerRequestedAdministrator, SecondExplorerRequestedAdministrator, SecondExplorerFollowsExplorer, NewcomerRequestedExplorer];
}

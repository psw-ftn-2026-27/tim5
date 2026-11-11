using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge DraftSunsetGathering = new("Okupljanje na zalasku", "Bravo, uhvatili ste zalazak zajedno!", 5, 200, 50, new TimeWindow(new TimeOnly(18, 0), new TimeOnly(20, 0)));
    public static readonly SocialChallenge DraftNightWalk = new("Noćna šetnja", "Grad je vaš i posle ponoći!", 3, 500, 80, new TimeWindow(new TimeOnly(22, 0), new TimeOnly(2, 0)));

    public static SocialChallenge[] All => [DraftSunsetGathering, DraftNightWalk];
}

using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge DraftSunsetGathering = new("Okupljanje na zalasku", "Bravo, uhvatili ste zalazak zajedno!", 5, 200, 50, new TimeWindow(new TimeOnly(18, 0), new TimeOnly(20, 0)));
    public static readonly SocialChallenge DraftNightWalk = new("Noćna šetnja", "Grad je vaš i posle ponoći!", 3, 500, 80, new TimeWindow(new TimeOnly(22, 0), new TimeOnly(2, 0)));
    public static readonly SocialChallenge PublishedMorningRun;
    public static readonly SocialChallenge RetiredMarketMeetup;
    public static readonly SocialChallenge ArchivedRiverPicnic;

    static SocialChallengeSeed()
    {
        PublishedMorningRun = new("Jutarnje trčanje na keju", "Svaka čast, ranoranioci!", 4, 300, 40, new TimeWindow(new TimeOnly(6, 0), new TimeOnly(8, 0)));
        PublishedMorningRun.Publish();

        RetiredMarketMeetup = new("Susret na pijaci", "Pijaca je bolja u društvu!", 6, 150, 30, new TimeWindow(new TimeOnly(7, 0), new TimeOnly(11, 0)));
        RetiredMarketMeetup.Publish();
        RetiredMarketMeetup.Retire();

        ArchivedRiverPicnic = new("Piknik uz reku", "Hvala što ste se okupili uz reku!", 8, 400, 60, new TimeWindow(new TimeOnly(12, 0), new TimeOnly(16, 0)));
        ArchivedRiverPicnic.Publish();
        ArchivedRiverPicnic.Archive();
    }

    public static SocialChallenge[] All => [DraftSunsetGathering, DraftNightWalk, PublishedMorningRun, RetiredMarketMeetup, ArchivedRiverPicnic];
}

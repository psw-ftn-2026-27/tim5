using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"], new GroupSize(2, 8));
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"], new GroupSize(1, 15));
    public static readonly Tour SecondExplorersTrail = new(WellKnownUsers.SecondExplorer, "Tuđa staza", "Kratak opis staze drugog autora.", TourDifficulty.Easy, ["grad"], new GroupSize(1, 4));
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublishedVineyards;
    public static readonly Tour PublishedMonasteries;
    public static readonly Tour ArchivedSalasi;
    public static readonly Tour PublishedOldTown;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"], new GroupSize(1, 6));
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"], new GroupSize(4, 12));
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.Publish(new Price(true, 0));

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"], new GroupSize(2, 5));
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.Publish(new Price(true, 0));

        ArchivedSalasi = new(WellKnownUsers.Explorer, "Bački salaši", "Tura obilazi salaše u okolini Novog Sada, uz upoznavanje sa starim zanatima, domaćom kuhinjom i životom na ravnici kroz prethodna stoleća.", TourDifficulty.Easy, ["tradicija"], new GroupSize(2, 10));
        ArchivedSalasi.AddTransportTime(TransportMode.Car, 30);
        ArchivedSalasi.Publish(new Price(true, 0));
        ArchivedSalasi.Archive();

        PublishedOldTown = new(WellKnownUsers.Explorer, "Stari grad Novog Sada", "Tura polazi sa Trga slobode, prolazi Zmaj Jovinom i Dunavskom ulicom do Dunavskog parka i završava se u Katoličkoj porti, uz priču o istoriji grada.", TourDifficulty.Easy, ["grad", "istorija"], new GroupSize(1, 20));
        PublishedOldTown.AddTransportTime(TransportMode.Walking, 120);
        PublishedOldTown.AddTransportTime(TransportMode.Bicycle, 40);
        PublishedOldTown.Publish(new Price(true, 0));
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishedVineyards, PublishedMonasteries, ArchivedSalasi, PublishedOldTown];
}

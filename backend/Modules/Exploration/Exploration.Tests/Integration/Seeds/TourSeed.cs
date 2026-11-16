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
    public static readonly Tour MeasuredDraftQuay;
    public static readonly Tour PublishedMarshWithPublicSecret;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"], new GroupSize(1, 6));
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);
        PublishableRiverside.AddKeyPoint("Ribarsko ostrvo", "Polazna tačka staze, gde se ribari i danas okupljaju uz obalu.", UploadedImageSeed.ExplorersLinkedFishermansIslandPhoto.Id, null, true, new Location(45.2700, 19.8930));
        PublishableRiverside.AddKeyPoint("Most slobode", "Kraj staze, ispod mosta koji spaja Novi Sad sa Sremom.", UploadedImageSeed.ExplorersLinkedLibertyBridgePhoto.Id, "Ispod mosta je skriveno mesto za odmor.", false, new Location(45.2448, 19.8562));

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"], new GroupSize(4, 12));
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.AddKeyPoint("Porodični podrum", "Podrum u kome se bermet pravi po recepturi staroj više od dva veka.", UploadedImageSeed.ExplorersLinkedCellarPhoto.Id, null, true, new Location(45.2010, 19.9310));
        PublishedVineyards.AddKeyPoint("Trg u Sremskim Karlovcima", "Centralni trg sa Česmom četiri lava i Sabornom crkvom.", UploadedImageSeed.ExplorersLinkedKarlovciSquarePhoto.Id, null, false, new Location(45.2030, 19.9340));
        PublishedVineyards.Publish(new Price(true, 0));

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"], new GroupSize(2, 5));
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.AddKeyPoint("Manastir Krušedol", "Zadužbina poslednjih Brankovića, sa freskama iz šesnaestog veka.", UploadedImageSeed.ExplorersLinkedKrusedolPhoto.Id, null, true, new Location(45.1197, 19.9425));
        PublishedMonasteries.AddKeyPoint("Manastir Grgeteg", "Manastir obnovljen posle Drugog svetskog rata, okružen šumom.", UploadedImageSeed.ExplorersLinkedGrgetegPhoto.Id, null, false, new Location(45.1450, 19.8790));
        PublishedMonasteries.Publish(new Price(true, 0));

        ArchivedSalasi = new(WellKnownUsers.Explorer, "Bački salaši", "Tura obilazi salaše u okolini Novog Sada, uz upoznavanje sa starim zanatima, domaćom kuhinjom i životom na ravnici kroz prethodna stoleća.", TourDifficulty.Easy, ["tradicija"], new GroupSize(2, 10));
        ArchivedSalasi.AddTransportTime(TransportMode.Car, 30);
        ArchivedSalasi.AddKeyPoint("Salaš sa ambarom", "Salaš na kome se čuva stari ambar za kukuruz.", UploadedImageSeed.ExplorersLinkedFarmhousePhoto.Id, null, true, new Location(45.3050, 19.8000));
        ArchivedSalasi.AddKeyPoint("Kovačka radionica", "Radionica u kojoj se i dalje kuju potkovice.", UploadedImageSeed.ExplorersLinkedWorkshopPhoto.Id, null, false, new Location(45.3120, 19.7900));
        ArchivedSalasi.Publish(new Price(true, 0));
        ArchivedSalasi.Archive();

        PublishedOldTown = new(WellKnownUsers.Explorer, "Stari grad Novog Sada", "Tura polazi sa Trga slobode, prolazi Zmaj Jovinom i Dunavskom ulicom do Dunavskog parka i završava se u Katoličkoj porti, uz priču o istoriji grada.", TourDifficulty.Easy, ["grad", "istorija"], new GroupSize(1, 20));
        PublishedOldTown.AddTransportTime(TransportMode.Walking, 120);
        PublishedOldTown.AddTransportTime(TransportMode.Bicycle, 40);
        PublishedOldTown.AddKeyPoint("Trg slobode", "Centralni trg grada, sa Gradskom kućom i spomenikom Svetozaru Miletiću.", UploadedImageSeed.ExplorersLinkedLibertySquarePhoto.Id, null, true, new Location(45.2551, 19.8451));
        PublishedOldTown.AddKeyPoint("Dunavski park", "Park sa jezercetom i ostrvom, nastao na mestu nekadašnje bare.", UploadedImageSeed.ExplorersLinkedDanubeParkPhoto.Id, null, false, new Location(45.2550, 19.8508));
        PublishedOldTown.Publish(new Price(true, 0));

        MeasuredDraftQuay = new(WellKnownUsers.Explorer, "Šetnja kejom", "Kratak opis šetnje kejom.", TourDifficulty.Easy, ["grad"], new GroupSize(1, 8));
        MeasuredDraftQuay.AddKeyPoint("Početak keja", "Kej počinje kod Štranda.", UploadedImageSeed.ExplorersLinkedQuayStartPhoto.Id, "Prva tajna keja.", true, new Location(45.2400, 19.8450));
        MeasuredDraftQuay.AddKeyPoint("Pristanište", "Pristanište za rečne brodove.", UploadedImageSeed.ExplorersLinkedQuayPierPhoto.Id, null, false, new Location(45.2480, 19.8550));
        MeasuredDraftQuay.AddKeyPoint("Kraj keja", "Kej se završava kod Varadinskog mosta.", UploadedImageSeed.ExplorersLinkedQuayEndPhoto.Id, "Treća tajna keja.", false, new Location(45.2530, 19.8620));
        MeasuredDraftQuay.RecordLength(2.4m);

        PublishedMarshWithPublicSecret = new(WellKnownUsers.Explorer, "Koviljski rit", "Tura vodi nasipom kroz Koviljski rit, pored rukavaca Dunava i bara, do kolonije čaplji koja se posmatra sa drvene osmatračnice.", TourDifficulty.Moderate, ["priroda"], new GroupSize(2, 6));
        PublishedMarshWithPublicSecret.AddTransportTime(TransportMode.Walking, 150);
        PublishedMarshWithPublicSecret.AddKeyPoint("Nasip uz rukavac", "Nasip sa koga se vide rukavci Dunava i stare vrbe.", UploadedImageSeed.ExplorersLinkedMarshPhoto.Id, "Ispod nasipa je skrivena stara skela.", true, new Location(45.1900, 20.1200));
        PublishedMarshWithPublicSecret.AddKeyPoint("Kolonija čaplji", "Osmatračnica iznad kolonije sivih čaplji.", UploadedImageSeed.ExplorersLinkedHeronColonyPhoto.Id, null, false, new Location(45.1850, 20.1350));
        PublishedMarshWithPublicSecret.RecordLength(6.5m);
        PublishedMarshWithPublicSecret.Publish(new Price(true, 0));
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishedVineyards, PublishedMonasteries, ArchivedSalasi, PublishedOldTown, MeasuredDraftQuay, PublishedMarshWithPublicSecret];
}

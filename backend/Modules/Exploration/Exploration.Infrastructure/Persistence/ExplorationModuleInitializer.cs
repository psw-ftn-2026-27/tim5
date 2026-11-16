using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Domain.Images;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationModuleInitializer : IHostedService
{
    private static readonly Guid FirstExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");

    // A complete, valid 1x1 PNG, standing in for the photos of the demo key points.
    private static readonly byte[] OnePixelPng =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
        0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
        0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
        0x42, 0x60, 0x82
    ];

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public ExplorationModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ExplorationDbContext>();
        await dbContext.Database.MigrateAsync();

        if (_environment.IsDevelopment() && !await dbContext.Tours.AnyAsync())
        {
            var images = CreateDemoImages();
            dbContext.UploadedImages.AddRange(images);
            dbContext.Tours.AddRange(CreateDemoTours(images));
            await dbContext.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<UploadedImage> CreateDemoImages()
    {
        var images = new List<UploadedImage>();
        for (var i = 0; i < 4; i++)
        {
            var image = UploadQuota.CreateImage(FirstExplorer, 0, new ImageFile(OnePixelPng));
            image.LinkTo(FirstExplorer);
            images.Add(image);
        }
        return images;
    }

    private static List<Tour> CreateDemoTours(List<UploadedImage> images)
    {
        var fortress = new Tour(FirstExplorer, "Šetnja Petrovaradinskom tvrđavom",
            "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav i stari deo Novog Sada.",
            TourDifficulty.Easy, ["istorija", "priroda"], new GroupSize(1, 10));
        fortress.AddTransportTime(TransportMode.Walking, 120);
        fortress.AddKeyPoint("Sahat kula", "Sat na kuli ima zamenjene kazaljke: mala pokazuje minute, a velika sate, da bi se vreme videlo sa reke.",
            images[0].Id, "Ispod kule počinje ulaz u podzemne vojne galerije.", true, new Location(45.2527, 19.8619));
        fortress.AddKeyPoint("Vidikovac nad Dunavom", "Sa ivice Gornjeg platoa pruža se pogled na Dunav, Varadinski most i stari deo Novog Sada.",
            images[1].Id, null, false, new Location(45.2533, 19.8608));
        fortress.Publish(new Price(true, 0));

        var fruskaGora = new Tour(FirstExplorer, "Biciklom preko Fruške gore",
            "Ruta vodi od Sremske Kamenice preko Popovice do Iriškog venca, pored manastira Beočin i vidikovaca sa kojih se vidi ceo Srem, uz nekoliko dužih uspona.",
            TourDifficulty.Hard, ["biciklizam", "planina"], new GroupSize(2, 6));
        fruskaGora.AddTransportTime(TransportMode.Bicycle, 240);
        fruskaGora.AddKeyPoint("Sremska Kamenica", "Polazna tačka rute, u podnožju Fruške gore, odakle počinje prvi duži uspon ka Popovici.",
            images[2].Id, null, true, new Location(45.2208, 19.8410));
        fruskaGora.AddKeyPoint("Iriški venac", "Najviša tačka rute, sa spomenikom slobodi i vidikovcem sa kog se vidi ceo Srem.",
            images[3].Id, "Sa vidikovca se po vedrom danu vidi i Avala.", false, new Location(45.1466, 19.8547));
        fruskaGora.Publish(new Price(true, 0));

        var cityCenter = new Tour(FirstExplorer, "Obilazak centra grada",
            "Kratak obilazak Trga slobode, Dunavske ulice i Dunavskog parka.",
            TourDifficulty.Moderate, ["grad"], new GroupSize(1, 20));

        return [fortress, fruskaGora, cityCenter];
    }
}

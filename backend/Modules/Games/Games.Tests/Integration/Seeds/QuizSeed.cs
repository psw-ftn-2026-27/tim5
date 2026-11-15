using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Quiz DraftFortressQuiz = new("Tajne Petrovaradinske tvrđave", "Prošetajte tvrđavom i odgovorite na pitanja.", 60, new Location(45.2517, 19.8628),
        [
            new Question("Koliko kilometara podzemnih hodnika ima tvrđava?", [new Answer("Oko 16", true), new Answer("Oko 2", false), new Answer("Oko 50", false)]),
            new Question("Šta je neobično kod sata na tvrđavi?", [new Answer("Kazaljke su zamenjene", true), new Answer("Nema brojeve", false)])
        ]);

    public static readonly Quiz DraftOldTownQuiz = new("Stari grad", "Pronađite odgovore na trgu.", 30, new Location(45.2551, 19.8451),
        [
            new Question("Kako se zove glavni trg?", [new Answer("Trg slobode", true), new Answer("Trg republike", false)])
        ]);

    public static readonly Quiz PublishedCathedralQuiz;
    public static readonly Quiz RetiredDanubeParkQuiz;
    public static readonly Quiz ArchivedFishermansIslandQuiz;

    static QuizSeed()
    {
        PublishedCathedralQuiz = new("Katedrala", "Obiđite katedralu i odgovorite na pitanja.", 40, new Location(45.2554, 19.8449),
            [
                new Question("Kojim stilom je građena katedrala?", [new Answer("Neogotičkim", true), new Answer("Baroknim", false)])
            ]);
        PublishedCathedralQuiz.Publish();

        RetiredDanubeParkQuiz = new("Dunavski park", "Prošetajte parkom i pronađite odgovore.", 20, new Location(45.2540, 19.8520),
            [
                new Question("Šta se nalazi na ostrvu u jezeru parka?", [new Answer("Vrba", true), new Answer("Fontana", false)])
            ]);
        RetiredDanubeParkQuiz.Publish();
        RetiredDanubeParkQuiz.Retire();

        ArchivedFishermansIslandQuiz = new("Ribarsko ostrvo", "Istražite ostrvo uz Dunav.", 50, new Location(45.2700, 19.8900),
            [
                new Question("Uz koju reku je Ribarsko ostrvo?", [new Answer("Dunav", true), new Answer("Sava", false)])
            ]);
        ArchivedFishermansIslandQuiz.Publish();
        ArchivedFishermansIslandQuiz.Archive();
    }

    public static Quiz[] All => [DraftFortressQuiz, DraftOldTownQuiz, PublishedCathedralQuiz, RetiredDanubeParkQuiz, ArchivedFishermansIslandQuiz];
}

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

    public static Quiz[] All => [DraftFortressQuiz, DraftOldTownQuiz];
}

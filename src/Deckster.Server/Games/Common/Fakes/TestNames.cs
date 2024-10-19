using Deckster.Server.Games.CrazyEights.Core;

namespace Deckster.Server.Games.Common.Fakes;

public static class TestNames
{
    private static readonly string[] Names =
    [
        "Kamuf Larsen",
        "Ellef van Znabel",
        "Sølvi Normalbakken",
        "Gard Ihnstang",
        "Per Pleks",
        "Piirka Lihti Salaten",
        "Bangkok Kjemperap",
        "Alf Wiedersehen",
        "Won Trang Truse",
        "Tarjei Sammen Jr"
    ];

    public static string Random() => Names.Random();
}
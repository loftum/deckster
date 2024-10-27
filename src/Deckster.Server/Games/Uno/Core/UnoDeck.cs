using Deckster.Client.Games.Uno;

namespace Deckster.Server.Games.Uno.Core;

public static class UnoDeck
{
    public static List<UnoCard> Standard
    {
        get
        {
            var ret = new List<UnoCard>();
            foreach (var color in Enum.GetValues<UnoColor>())
            {
                if (color == UnoColor.Wild)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        ret.Add(new UnoCard(color, UnoValue.Wild));
                        ret.Add(new UnoCard(color, UnoValue.WildDrawFour));
                    }
                }
                else
                {
                    for (var i = 0; i < 2; i++)
                    {
                        foreach (var value in Enum.GetValues<UnoValue>())
                        {
                            if (value == UnoValue.Wild || value == UnoValue.WildDrawFour)
                            {
                                continue;
                            }
                            if(value == UnoValue.Zero && i == 1)
                            {
                                continue;
                            }
                            ret.Add(new UnoCard(color, value));
                        }
                    }
                }
            }
            
            return ret;
        }
    }
}

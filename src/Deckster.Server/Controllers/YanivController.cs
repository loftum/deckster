using Deckster.Games.Yaniv;
using Deckster.Server.Data;
using Deckster.Server.Games;
using Deckster.Server.Games.Yaniv;

namespace Deckster.Server.Controllers;

public class YanivController(GameHostRegistry hostRegistry, IRepo repo)
    : GameController<YanivGameHost, YanivGame>(hostRegistry, repo);
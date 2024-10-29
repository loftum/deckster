using Deckster.Client.Games.Uno;
using Deckster.Server.Data;
using Deckster.Server.Games;
using Deckster.Server.Games.Uno;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

[Route("uno")]
public class UnoController(GameHostRegistry hostRegistry, IRepo repo)
    : GameController<UnoClient, UnoGameHost, UnoGame>(hostRegistry, repo);
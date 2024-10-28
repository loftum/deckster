using Deckster.Client.Games.Idiot;
using Deckster.Server.Data;
using Deckster.Server.Games;
using Deckster.Server.Games.Idiot;
using Deckster.Server.Games.Idiot.Core;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

[Route("idiot")]
public class IdiotController(GameHostRegistry hostRegistry, IRepo repo)
    : GameController<IdiotClient, IdiotGameHost, IdiotGame>(hostRegistry, repo);
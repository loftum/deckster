using System.Reflection;
using Deckster.Client.Protocol;
using Deckster.Server.Games.CrazyEights.Core;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;

namespace Deckster.Server.Games;

public abstract class GameProjection<TGame> : SingleStreamProjection<TGame>
{
    private static readonly Dictionary<Type, MethodInfo> _applies;
    
    static GameProjection()
    {
        var methods = from method in typeof(GameProjection<TGame>)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            where method.Name == "Apply" && method.ReturnType == typeof(Task)
            let parameters = method.GetParameters()
            where parameters.Length == 2 &&
                  parameters[0].ParameterType.IsSubclassOf(typeof(DecksterRequest)) &&
                  parameters[1].ParameterType == typeof(TGame)
            let parameter = parameters[0]
            select (parameter, method);
        _applies = methods.ToDictionary(m => m.parameter.ParameterType, m => m.method);
    }
    
    [MartenIgnore]
    public async Task<bool> HandleAsync(DecksterRequest request, TGame game)
    {
        if (!_applies.TryGetValue(request.GetType(), out var del))
        {
            return false;
        }

        await (Task)del.Invoke(this, [request, game]);
        return true;
    }

    public abstract (TGame game, object startEvent) Create(IGameHost host);
}
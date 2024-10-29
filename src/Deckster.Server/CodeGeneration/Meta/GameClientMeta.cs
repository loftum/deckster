using System.Reflection;
using Deckster.Server.Games;

namespace Deckster.Server.CodeGeneration.Meta;

public class GameClientMeta
{
    public string Name { get; init; }
    public List<MethodMeta> Methods { get; init; }
    
    public static bool TryGetFor(Type type, out GameClientMeta meta)
    {
        meta = default;
        if (!type.InheritsFrom<GameObject>())
        {
            return false;
        }

        var methods = new List<MethodMeta>();
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => !m.IsSpecialName))
        {
            if (method.TryGetGameMethod(out var gameMethod))
            {
                methods.Add(gameMethod);
            }
        }
        
        meta = new GameClientMeta
        {
            Name = $"{type.Name.Replace("Game", "")}Client",
            Methods = methods
        };

        return true;
    }
}
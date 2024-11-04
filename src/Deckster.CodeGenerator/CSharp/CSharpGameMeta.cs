using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Deckster.Games;
using Deckster.Games.CodeGeneration.Meta;

namespace Deckster.CodeGenerator.CSharp;

public class CSharpGameMeta
{
    public string Name { get; }
    public NotificationInfo[] Notifications { get; }
    public GameMethodInfo[] Methods { get; }
    public GameExtensionMethodInfo[] ExtensionMethods { get; }
    public string[] Usings { get; }
    
    public CSharpGameMeta(Type gameType)
    {
        Name = gameType.Name.Replace("Game", "");
        Notifications = gameType.GetNotifications().ToArray();
        Methods = gameType.GetGameMethods().ToArray();

        var usings = new HashSet<string>();
        foreach (var ns in Notifications.Select(n => n.MessageType.Namespace))
        {
            usings.AddIfNotNull(ns);
        }

        var extensionMethods = new List<GameExtensionMethodInfo>();
        foreach (var method in Methods)
        {
            if (method.Request.ParameterType.Namespace != null)
            {
                usings.Add(method.Request.ParameterType.Namespace);
            }

            if (method.ResponseType.Namespace != null)
            {
                usings.Add(method.ResponseType.Namespace);
            }

            if (method.TryGetExtensionMethod(out var extensionMethod))
            {
                extensionMethods.Add(extensionMethod);
                usings.AddIfNotNull(extensionMethod.ReturnType.Namespace);
                usings.AddRangeIfNotNull(extensionMethod.Parameters.Select(p => p.ParameterType.Namespace));
            }
        }

        ExtensionMethods = extensionMethods.ToArray();

        Usings = usings.ToArray();
    }

    public static bool TryGetFor(Type type, [MaybeNullWhen(false)] out CSharpGameMeta meta)
    {
        if (type.InheritsFrom<GameObject>() && !type.IsAbstract)
        {
            meta = new CSharpGameMeta(type);
            return true;
        }

        meta = default;
        return false;
    }
}

public record NotificationInfo(string Name, Type MessageType);

public record GameMethodInfo(string Name, ParameterInfo Request, Type ResponseType);

public record GameExtensionMethodInfo(string Name, GameParameterInfo[] Parameters, GameMethodInfo Method, Type ReturnType);

public record GameParameterInfo(string Name, Type ParameterType);

public static class HashSetExtensions
{
    public static void AddIfNotNull<T>(this HashSet<T> set, T? item) where T : class
    {
        if (item != null)
        {
            set.Add(item);
        }
    }

    public static void AddRangeIfNotNull<T>(this HashSet<T> set, IEnumerable<T?> items) where T : class
    {
        foreach (var item in items)
        {
            set.AddIfNotNull(item);
        }
    }
}
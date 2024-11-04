using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Deckster.Core.Protocol;
using Deckster.Games;
using Deckster.Games.CodeGeneration.Meta;

namespace Deckster.CodeGenerator.CSharp;

public class CSharpGameMeta
{
    public string Name { get; }
    public NotificationInfo[] Notifications { get; }
    public GameMethodInfo[] Methods { get; }
    public string[] Usings { get; }
    
    public CSharpGameMeta(Type gameType)
    {
        Name = gameType.Name;
        Notifications = gameType.GetNotifications().ToArray();
        Methods = gameType.GetGameMethods().ToArray();

        var usings = new HashSet<string>();
        foreach (var ns in Notifications.Select(n => n.Message.Namespace))
        {
            if (ns != null)
            {
                usings.Add(ns);    
            }
        }

        foreach (var method in Methods)
        {
            if (method.Parameter.ParameterType.Namespace != null)
            {
                usings.Add(method.Parameter.ParameterType.Namespace);
            }

            if (method.ReturnType.Namespace != null)
            {
                usings.Add(method.ReturnType.Namespace);
            }
        }

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

public class NotificationInfo
{
    public string Name { get; }
    public EventInfo Event { get; }
    public Type Message { get; }
    
    public NotificationInfo(EventInfo e, Type message)
    {
        Name = e.Name;
        Event = e;
        Message = message;
    }
}

public class GameMethodInfo
{
    public string Name { get; }
    public ParameterInfo Parameter { get; }
    public Type ReturnType { get; }

    public GameMethodInfo(string name, ParameterInfo parameter, Type returnType)
    {
        Name = name;
        Parameter = parameter;
        ReturnType = returnType;
    }
}


public static class GameReflectionExtensions
{
    public static IEnumerable<NotificationInfo> GetNotifications(this Type type)
    {
        foreach (var e in type.GetEvents())
        {
            if (e.IsNotifyAll(out var argument) || e.IsNotifyPlayer(out argument))
            {
                yield return new NotificationInfo(e, argument);
            }
        }
    }

    public static IEnumerable<GameMethodInfo> GetGameMethods(this Type type)
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (method.IsGameMethod(out var info))
            {
                yield return info;
            }
        }
    }

    private static bool IsGameMethod(this MethodInfo method, [MaybeNullWhen(false)] out GameMethodInfo gameMethod)
    {
        if (method.TryGetRequestParameter(out var parameter) && method.ReturnType.IsTaskOfDecksterResponse(out var responseType))
        {
            gameMethod = new GameMethodInfo(method.Name, parameter, responseType);
            return true;
        }

        gameMethod = default;
        return false;
    }

    private static bool TryGetRequestParameter(this MethodInfo info, [MaybeNullWhen(false)] out ParameterInfo parameter)
    {
        var parameters = info.GetParameters();
        if (parameters.Length == 1 && parameters[0].ParameterType.InheritsFrom<DecksterResponse>())
        {
            parameter = parameters[0];
            return true;
        }

        parameter = default;
        return false;
    }

    private static bool IsTaskOfDecksterResponse(this Type type, [MaybeNullWhen(false)] out Type responseType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>) && type.GenericTypeArguments[0].InheritsFrom<DecksterResponse>())
        {
            responseType = type.GenericTypeArguments[0];
            return true;
        }

        responseType = default;
        return false;
    }
}
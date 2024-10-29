using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Deckster.Client.Protocol;

namespace Deckster.Server.CodeGeneration.Meta;

internal static class GameTypeExtensions
{
    public static bool InheritsFrom<T>(this Type type)
    {
        return typeof(T).IsAssignableFrom(type);
    }
    
    public static bool TryGetGameMethod(this MethodInfo method, [MaybeNullWhen(false)] out MethodMeta meta)
    {
        if (method.ReturnType.TryGetTaskOfDecksterResponse(out var returnType) &&
            method.TryGetDecksterRequestParameter(out var requestParameter))
        {
            meta = new MethodMeta
            {
                Name = method.Name,
                ReturnType = returnType,
                Parameters = [requestParameter]
            };
            return true;
        }

        meta = default;
        return false;
    }

    private static bool TryGetDecksterRequestParameter(this MethodInfo method, [MaybeNullWhen(false)] out ParameterMeta meta)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 1 &&
            parameters[0].ParameterType.InheritsFrom<DecksterRequest>())
        {
            var parameter = parameters[0];
            meta = new ParameterMeta
            {
                Name = parameter.Name,
                Type = new TypeMeta
                {
                    Name = parameter.ParameterType.Name
                }
            };
            return true;
        }

        meta = default;
        return false;
    }

    private static bool TryGetTaskOfDecksterResponse(this Type type, [MaybeNullWhen(false)] out TypeMeta meta)
    {
        meta = default;
        if (!type.IsGenericType)
        {
            return false;
        }
        
        var genericTypeDefinition = type.GetGenericTypeDefinition();
        if (type.IsGenericType &&
            type.BaseType == typeof(Task) &&
            genericTypeDefinition == typeof(Task<>) &&
            type.GenericTypeArguments[0].InheritsFrom<DecksterResponse>())
        {
            meta = new TypeMeta
            {
                Name = type.GenericTypeArguments[0].Name 
            };
            return true;
        }

        meta = default;
        return false;

    }
}
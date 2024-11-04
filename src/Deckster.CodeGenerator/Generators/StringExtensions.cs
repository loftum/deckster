namespace Deckster.CodeGenerator.Generators;

public static class StringExtensions
{
    public static string StringJoined<T>(this IEnumerable<T> items, string separator)
    {
        return string.Join(separator, items);
    }
    
    public static string ToCamelCase(this string input)
    {
        if (char.IsLower(input[0]))
        {
            return input;
        }

        var chars = input.ToCharArray();
        chars[0] = char.ToLowerInvariant(chars[0]);
        return new string(chars);
    }
}
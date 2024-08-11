using System.Diagnostics.CodeAnalysis;
using Deckster.Server.Users;

namespace Deckster.Server.Authentication;

public static class AuthenticationExtensions
{
    public static void SetUser(this HttpContext context, User user)
    {
        context.Items["User"] = user;
    }
    
    public static User? GetUser(this HttpContext context)
    {
        return context.Items.TryGetValue("User", out var o) && o is User u ? u : null;
    }

    public static bool TryGetUser(this HttpContext context, [MaybeNullWhen(false)] out User user)
    {
        if (context.Items.TryGetValue("User", out var o) && o is User u)
        {
            user = u;
            return true;
        }

        user = null;
        return false;
    }
    
    public static User GetRequiredUser(this HttpContext context)
    {
        if (context.Items.TryGetValue("User", out var o) && o is User u)
        {
            return u;
        }

        throw new ApplicationException("User is required");
    }

    public static IApplicationBuilder LoadUser(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserLoaderMiddleware>();
    }
}

public static class AuthenticationSchemes
{
    public const string Cookie = "cookie";
}
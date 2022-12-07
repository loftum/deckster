using System.Collections.Concurrent;
using Deckster.Core.Games.CrazyEights;

namespace Deckster.Core.Domain;

public class UserRepo
{
    private readonly ConcurrentBag<User> _users = new();

    public Task<User?> GetAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }


    public Task<User?> GetByTokenAsync(string token)
    {
        var user = _users.FirstOrDefault(u => u.AccessToken == token);
        return Task.FromResult(user);
    }
}
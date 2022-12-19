using System.Collections.Concurrent;

namespace Deckster.Server.Users;

public class UserRepo
{
    private readonly ConcurrentBag<User> _users = new();

    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }


    public Task<User?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.AccessToken == token);
        return Task.FromResult(user);
    }
}
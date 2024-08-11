using System.Collections.Concurrent;

namespace Deckster.Server.Users;

public interface IUserRepo
{
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task SaveAsync(User user);
}

public class UserRepo : IUserRepo
{
    private readonly ConcurrentDictionary<Guid, User> _users = new()
    {
        [Guid.Parse("eed69907-916a-47fb-bc3c-a96dd096e64d")] = new User
        {
            Id = Guid.Parse("eed69907-916a-47fb-bc3c-a96dd096e64d"),
            AccessToken = "abc123",
            Name = "Kamuf Larsen"
        }
    };

    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = _users.GetValueOrDefault(id);
        return Task.FromResult(user);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => string.Equals(u.Name, username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<User?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => u.AccessToken == token);
        return Task.FromResult(user);
    }

    public Task SaveAsync(User user)
    {
        if (user.Id == default)
        {
            user.Id = Guid.NewGuid();
        }
        _users.AddOrUpdate(user.Id, user, (_, _) => user);
        return Task.CompletedTask;
    }
}
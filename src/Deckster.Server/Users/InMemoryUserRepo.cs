using System.Collections.Concurrent;

namespace Deckster.Server.Users;

public interface IUserRepo
{
    Task<DecksterUser?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DecksterUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<DecksterUser?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task SaveAsync(DecksterUser user);
}

public class InMemoryUserRepo : IUserRepo
{
    private readonly ConcurrentDictionary<Guid, DecksterUser> _users = new()
    {
        [Guid.Parse("eed69907-916a-47fb-bc3c-a96dd096e64d")] = new DecksterUser
        {
            Id = Guid.Parse("eed69907-916a-47fb-bc3c-a96dd096e64d"),
            Password = "hest",
            AccessToken = "abc123",
            Name = "Kamuf Larsen"
        }
    };

    public Task<DecksterUser?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = _users.GetValueOrDefault(id);
        return Task.FromResult(user);
    }

    public Task<DecksterUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => string.Equals(u.Name, username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<DecksterUser?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => u.AccessToken == token);
        return Task.FromResult(user);
    }

    public Task SaveAsync(DecksterUser user)
    {
        if (user.Id == default)
        {
            user.Id = Guid.NewGuid();
        }
        _users.AddOrUpdate(user.Id, user, (_, _) => user);
        return Task.CompletedTask;
    }
}
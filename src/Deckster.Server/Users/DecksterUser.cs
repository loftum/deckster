namespace Deckster.Server.Users;

public class DecksterUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AccessToken { get; set; } = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
    public string Name { get; set; } = "New player";
    public string Password { get; set; }
}
namespace Loyaltybenefit.Domain.Entities;

public sealed class DatabaseCredential
{
    public Guid Id { get; private set; }

    public Guid DatabaseServerId { get; private set; }

    public string Username { get; private set; } = string.Empty;

    public string PasswordSecret { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private DatabaseCredential()
    {
    }

    public DatabaseCredential(
        Guid id,
        Guid databaseServerId,
        string username,
        string passwordSecret)
    {
        Id = id;
        DatabaseServerId = databaseServerId;
        Username = username;
        PasswordSecret = passwordSecret;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}

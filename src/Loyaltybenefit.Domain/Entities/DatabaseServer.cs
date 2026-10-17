namespace Loyaltybenefit.Domain.Entities;

public sealed class DatabaseServer
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string ProviderCode { get; private set; } = string.Empty;

    public string EngineCode { get; private set; } = string.Empty;

    public string Host { get; private set; } = string.Empty;

    public int Port { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private DatabaseServer()
    {
    }

    public DatabaseServer(
        Guid id,
        string code,
        string name,
        string providerCode,
        string engineCode,
        string host,
        int port)
    {
        Id = id;
        Code = code;
        Name = name;
        ProviderCode = providerCode;
        EngineCode = engineCode;
        Host = host;
        Port = port;
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

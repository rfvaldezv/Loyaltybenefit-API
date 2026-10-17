namespace Loyaltybenefit.Domain.Entities;

public sealed class Tenant
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public Guid IndustryId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Tenant()
    {
    }

    public Tenant(
        Guid id,
        string code,
        string name,
        Guid industryId)
    {
        Id = id;
        Code = code;
        Name = name;
        IndustryId = industryId;
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

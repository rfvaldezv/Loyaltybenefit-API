namespace Loyaltybenefit.Domain.Entities;

public sealed class Organization
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string IndustryCode { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    private Organization()
    {
    }

    public Organization(
        Guid id,
        string code,
        string name,
        string industryCode)
    {
        Id = id;
        Code = code;
        Name = name;
        IndustryCode = industryCode;
        CreatedAtUtc = DateTime.UtcNow;
        IsActive = true;
    }
}

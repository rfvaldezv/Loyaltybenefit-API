namespace Loyaltybenefit.Domain.Entities;

public sealed class Property
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; } // El corporativo al que pertenece
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? LogoUrl { get; private set; }
    public string? PrimaryColorHex { get; private set; }
    public string? SecondaryColorHex { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Property() { }

    public Property(Guid id, Guid tenantId, string name, string code)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        Code = code;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        PrimaryColorHex = "#4F46E5";
        SecondaryColorHex = "#8B5CF6";
    }

    public void UpdateBranding(string? logoUrl, string? primaryColorHex, string? secondaryColorHex)
    {
        LogoUrl = logoUrl;
        PrimaryColorHex = primaryColorHex;
        SecondaryColorHex = secondaryColorHex;
    }
}

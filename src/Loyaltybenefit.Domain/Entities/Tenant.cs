namespace Loyaltybenefit.Domain.Entities;

public sealed class Tenant
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public Guid IndustryId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    // NUEVAS PROPIEDADES DE BRANDING
    public string? LogoUrl { get; private set; }
    
    public string? PrimaryColorHex { get; private set; }
    
    public string? SecondaryColorHex { get; private set; }

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
        PrimaryColorHex = "#4F46E5"; // Color por defecto
        SecondaryColorHex = "#8B5CF6"; // Color por defecto
    }

    // NUEVO MÉTODO PARA ACTUALIZAR BRANDING
    public void UpdateBranding(string? logoUrl, string? primaryColorHex, string? secondaryColorHex)
    {
        LogoUrl = logoUrl;
        PrimaryColorHex = primaryColorHex;
        SecondaryColorHex = secondaryColorHex;
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

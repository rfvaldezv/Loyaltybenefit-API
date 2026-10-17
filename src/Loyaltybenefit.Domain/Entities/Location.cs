namespace Loyaltybenefit.Domain.Entities;

public sealed class Location
{
    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string State { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Location()
    {
    }

    public Location(
        Guid id,
        Guid organizationId,
        string code,
        string name,
        string city,
        string state,
        string country)
    {
        Id = id;
        OrganizationId = organizationId;
        Code = code;
        Name = name;
        City = city;
        State = state;
        Country = country;
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

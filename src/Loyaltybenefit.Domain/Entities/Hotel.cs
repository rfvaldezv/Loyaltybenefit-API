namespace Loyaltybenefit.Domain.Entities;

public sealed class Hotel
{
    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public string? City { get; private set; }

    public string? State { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Hotel()
    {
        Code = null!;
        Name = null!;
    }

    public Hotel(
        Guid id,
        Guid organizationId,
        string code,
        string name)
    {
        Id = id;
        OrganizationId = organizationId;
        Code = code;
        Name = name;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }
}

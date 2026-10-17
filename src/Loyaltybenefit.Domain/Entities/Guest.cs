namespace Loyaltybenefit.Domain.Entities;

public sealed class Guest
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public DateTime? BirthDate { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Guest()
    {
    }

    public Guest(
        Guid id,
        string firstName,
        string lastName,
        string? email = null,
        string? phone = null)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        CreatedAtUtc = DateTime.UtcNow;
        IsActive = true;
    }

    public void UpdateContactInformation(
        string? email,
        string? phone)
    {
        Email = email;
        Phone = phone;
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

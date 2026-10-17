namespace Loyaltybenefit.Domain.Entities;

public sealed class Industry
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    private Industry()
    {
    }

    public Industry(
        Guid id,
        string code,
        string name)
    {
        Id = id;
        Code = code;
        Name = name;
        IsActive = true;
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

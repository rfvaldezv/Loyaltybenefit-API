namespace Loyaltybenefit.Domain.Entities;

public sealed class PointTransaction
{
    public Guid Id { get; private set; }
    public Guid LoyaltyAccountId { get; private set; }
    public Guid? StayId { get; private set; }
    
    // NUEVO: Rastreo de la propiedad (Sucursal)
    public Guid? PropertyId { get; private set; } 
    
    public int Points { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Reference { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    
    // NUEVO: Fecha de expiración de los puntos
    public DateTime? ExpiresAtUtc { get; private set; } 

    private PointTransaction() { }

    public PointTransaction(
        Guid id, Guid loyaltyAccountId, int points, string type, 
        string? description = null, string? reference = null, 
        Guid? stayId = null, Guid? propertyId = null, DateTime? expiresAtUtc = null)
    {
        if (points == 0) throw new ArgumentException("Points cannot be zero.", nameof(points));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Transaction type is required.", nameof(type));

        Id = id;
        LoyaltyAccountId = loyaltyAccountId;
        StayId = stayId;
        PropertyId = propertyId;
        Points = points;
        Type = type;
        Description = description;
        Reference = reference;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }
}

namespace Loyaltybenefit.Domain.Entities;

public sealed class LoyaltyRule
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    // Cada cuánto dinero se genera un bloque de puntos.
    // Ejemplo: $100 = 10 puntos.
    public decimal AmountThreshold { get; private set; }

    // Puntos generados por cada AmountThreshold.
    // Ejemplo: $100 = 10 puntos.
    public int Points { get; private set; }

    // Cantidad de puntos necesarios para obtener
    // RedemptionAmount.
    // Ejemplo: 100 puntos = $10.
    public int RedemptionPoints { get; private set; }

    // Valor monetario obtenido al redimir RedemptionPoints.
    // Ejemplo: 100 puntos = $10.
    public decimal RedemptionAmount { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private LoyaltyRule()
    {
    }

    public LoyaltyRule(
        Guid id,
        string code,
        string name,
        decimal amountThreshold,
        int points,
        int redemptionPoints,
        decimal redemptionAmount)
    {
        if (amountThreshold <= 0)
        {
            throw new ArgumentException(
                "Amount threshold must be greater than zero.",
                nameof(amountThreshold));
        }

        if (points <= 0)
        {
            throw new ArgumentException(
                "Points must be greater than zero.",
                nameof(points));
        }

        if (redemptionPoints <= 0)
        {
            throw new ArgumentException(
                "Redemption points must be greater than zero.",
                nameof(redemptionPoints));
        }

        if (redemptionAmount <= 0)
        {
            throw new ArgumentException(
                "Redemption amount must be greater than zero.",
                nameof(redemptionAmount));
        }

        Id = id;
        Code = code;
        Name = name;
        AmountThreshold = amountThreshold;
        Points = points;
        RedemptionPoints = redemptionPoints;
        RedemptionAmount = redemptionAmount;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

using Loyaltybenefit.Domain.Enums;

namespace Loyaltybenefit.Domain.Entities;

public sealed class TenantDatabase
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public Guid DatabaseServerId { get; private set; }

    public string LogicalName { get; private set; } = string.Empty;

    public string PhysicalDatabaseName { get; private set; } = string.Empty;

    public TenantDatabaseStatus Status { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private TenantDatabase()
    {
    }

    public TenantDatabase(
        Guid id,
        Guid tenantId,
        Guid databaseServerId,
        string logicalName,
        string physicalDatabaseName)
    {
        Id = id;
        TenantId = tenantId;
        DatabaseServerId = databaseServerId;
        LogicalName = logicalName;
        PhysicalDatabaseName = physicalDatabaseName;
        Status = TenantDatabaseStatus.Pending;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkProvisioning()
    {
        Status = TenantDatabaseStatus.Provisioning;
    }

    public void MarkReady()
    {
        Status = TenantDatabaseStatus.Ready;
    }

    public void MarkFailed()
    {
        Status = TenantDatabaseStatus.Failed;
    }

    public void Suspend()
    {
        Status = TenantDatabaseStatus.Suspended;
        IsActive = false;
    }

    public void Activate()
    {
        Status = TenantDatabaseStatus.Ready;
        IsActive = true;
    }
}

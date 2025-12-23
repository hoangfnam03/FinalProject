using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Enums;

namespace Domain.Entities
{
    public class Report : AuditableEntity, ITenantEntity
    {
        public long ReporterMemberId { get; set; }
        public ReportTargetType TargetType { get; set; }
        public long TargetId { get; set; }
        public string Reason { get; set; } = default!;
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public DateTime? ResolvedAt { get; set; }
        public long? ResolvedByMemberId { get; set; }

        public Guid? TenantId { get; set; }
    }
}

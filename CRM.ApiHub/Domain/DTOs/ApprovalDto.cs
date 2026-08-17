namespace CRM.ApiHub.Domain.DTOs
{
    public class ApprovalDto
    {
        public long IdApproval { get; set; }
        public long IdOrder { get; set; }
        public bool IsApproved { get; set; } // true: Aprobado, false: Rechazado
        public string? Comments { get; set; }
        public long AuthorizedBy { get; set; } // ID del supervisor
    }
}
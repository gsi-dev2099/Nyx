namespace CRM.ApiHub.Domain.DTOs
{
    public class AlternateProfileDto 
    {
    public long IdOrder { get; set; }
        public string AlternateType { get; set; } = string.Empty;
        public string AlternateData { get; set; } = string.Empty; 
        public string OriginalData { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public long CreatedBy { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("order_document", Schema = "sales_service")]
public class OrderDocument
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_document")]
    public long IdDocument { get; set; }

    [Column("id_order")]
    public long IdOrder { get; set; }

    [Column("document_type")]
    public string DocumentType { get; set; } = string.Empty;

    [Column("file_name")]
    public string FileName { get; set; } = string.Empty;

    [Column("file_path")]
    public string FilePath { get; set; } = string.Empty;

    [Column("file_size_kb")]
    public int? FileSizeKb { get; set; }

    [Column("mime_type")]
    public string? MimeType { get; set; }

    [Column("verified_by")]
    public long? VerifiedBy { get; set; }

    [Column("verified_at")]
    public DateTime? VerifiedAt { get; set; }

    [Column("verification_status")]
    public string VerificationStatus { get; set; } = "PENDING";

    [Column("verification_notes")]
    public string? VerificationNotes { get; set; }

    [Column("uploaded_by")]
    public long UploadedBy { get; set; }

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [NotMapped]
    public string? CustomerName { get; set; }

    [NotMapped]
    public string? Priority { get; set; }
}
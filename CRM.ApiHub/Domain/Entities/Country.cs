using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("country")]
public class Country
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty; // Ej. PE, US, CL

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("phone_code")]
    public string? PhoneCode { get; set; } // Ej. +51

    [Column("is_active")]
    public bool IsActive { get; set; }
}
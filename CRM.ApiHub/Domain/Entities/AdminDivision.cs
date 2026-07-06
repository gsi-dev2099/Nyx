using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("admin_division", Schema = "ext_ecosystem")]
public class AdminDivision
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("country_id")]
    public int CountryId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("type")]
    public string Type { get; set; } = string.Empty; // Ej. Departamento, Provincia, Distrito

    [Column("is_active")]
    public bool IsActive { get; set; }
}
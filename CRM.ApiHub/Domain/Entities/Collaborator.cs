using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("collaborators", Schema = "ext_ecosystem")]
public class Collaborator
{
    [Column("id_user")]
    public long? IdUser { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("paternal_surname")]
    public string? PaternalSurname { get; set; }

    [Column("maternal_surname")]
    public string? MaternalSurname { get; set; }

    [Column("document_number")]
    public string? DocumentNumber { get; set; }

    [Column("state")]
    public short? State { get; set; }
}
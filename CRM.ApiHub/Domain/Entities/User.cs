using System;

namespace CRM.ApiHub.Domain.Entities;

public class User
{
    public long IdUser { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime? DateCreated { get; set; }
    public bool? IsLoggedIn { get; set; }
    public DateTime? LastActivity { get; set; }
    public bool? Fingerprint { get; set; }
    public short? State { get; set; }
}

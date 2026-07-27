using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Services;

public class PreSaleViewModel
{
    public long IdPresale { get; set; }
    public long IdCmpg { get; set; }
    public string? Phone { get; set; }
    public string? Operator { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? CoverageStatus { get; set; }
    public long IdStatus { get; set; }
    public long OwnerUserId { get; set; }
    public long CurrentUserId { get; set; }
    public string? Notes { get; set; }
    public DateTime Register { get; set; }
}

public interface IPreSaleService
{
    Task<IEnumerable<PreSaleViewModel>> GetByUserAsync(long? userId = null);
    Task<bool> AddCallLogAsync(long id, string callLog);
    Task<bool> ConvertAsync(long id);
}

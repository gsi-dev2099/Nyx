namespace CRM.WebFrontend.Client.Models;

// ===== ESTADOS DE VENTA =====
public class OrderStatusMaintenanceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsTerminal { get; set; }
    public bool RequiresSubstatus { get; set; }
    public bool RequiresComment { get; set; }
    public bool AllowsEditByAsesor { get; set; }
    public bool AllowsEditBySupervisor { get; set; }
    public short OrderIndex { get; set; }
    public bool IsActive { get; set; }
}

// ===== PRODUCTOS / TARIFAS =====
public class ProductMaintenanceDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

// ===== CATÁLOGO DE INCIDENCIAS =====
public class IncidentCatalogMaintenanceDto
{
    public long IdIncident { get; set; }
    public long? IdCmpg { get; set; }
    public long? IdStatus { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SolutionTemplate { get; set; }
    public string? ResolutionType { get; set; }
    public bool RequiresResponse { get; set; }
    public bool IsRecurrent { get; set; }
    public short Priority { get; set; }
    public short SlaHours { get; set; }
    public long? CreatedBy { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CampaignName { get; set; }
}

public class CreateIncidentCatalogDto
{
    public long IdCmpg { get; set; }
    public long IdStatus { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SolutionTemplate { get; set; }
    public string? ResolutionType { get; set; }
    public bool RequiresResponse { get; set; }
    public bool IsRecurrent { get; set; }
    public short Priority { get; set; } = 3;
    public short SlaHours { get; set; } = 24;
}

public class UpdateIncidentCatalogDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SolutionTemplate { get; set; }
    public string? ResolutionType { get; set; }
    public bool? RequiresResponse { get; set; }
    public bool? IsRecurrent { get; set; }
    public short? Priority { get; set; }
    public short? SlaHours { get; set; }
    public bool? IsActive { get; set; }
}

// ===== TIPOS DE CAMBIO =====
public class ExchangeRateMaintenanceDto
{
    public long IdRate { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string Source { get; set; } = string.Empty;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExchangeRateDto
{
    public string FromCurrency { get; set; } = "EUR";
    public string ToCurrency { get; set; } = "PEN";
    public decimal Rate { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.Today;
    public DateTime? ValidTo { get; set; }
    public string Source { get; set; } = "MANUAL";
}

// ===== CAMPAÑAS (auxiliar) =====
public class CampaignSimpleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

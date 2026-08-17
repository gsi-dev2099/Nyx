using Dapper;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class FormRepository : IFormRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FormRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<FormTemplate>> GetTemplatesByCampaignStageAsync(long idCmpg, long idStage)
    {
        using var connection = _connectionFactory.CreateConnection();
        await SeedDefaultFormsAsync();

        const string sqlExact = @"
            SELECT * FROM sales_service.sales_form_template 
            WHERE id_cmpg = @IdCmpg AND id_stage = @IdStage AND is_active = true 
            ORDER BY form_order;";
            
        var templates = (await connection.QueryAsync<FormTemplate>(sqlExact, new { IdCmpg = idCmpg, IdStage = idStage })).ToList();
        if (templates.Any())
        {
            return templates;
        }

        // Fallback 1: Buscar por campaña sola
        const string sqlCmpg = @"
            SELECT * FROM sales_service.sales_form_template 
            WHERE id_cmpg = @IdCmpg AND is_active = true 
            ORDER BY form_order;";

        templates = (await connection.QueryAsync<FormTemplate>(sqlCmpg, new { IdCmpg = idCmpg })).ToList();
        if (templates.Any())
        {
            return templates;
        }

        // Fallback 2: Buscar cualquier plantilla activa configurada
        const string sqlAny = @"
            SELECT * FROM sales_service.sales_form_template 
            WHERE is_active = true 
            ORDER BY form_order;";

        return await connection.QueryAsync<FormTemplate>(sqlAny);
    }

    public async Task<IEnumerable<FormField>> GetFieldsByTemplateAsync(long idForm)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM sales_service.sales_form_field 
            WHERE id_form = @IdForm AND is_active = true 
            ORDER BY order_index;";
            
        var fields = (await connection.QueryAsync<FormField>(sql, new { IdForm = idForm })).ToList();
        if (fields.Any())
        {
            return fields;
        }

        // Fallback: Si el formulario no tiene campos registrados en DB, sembrar campos por defecto
        await SeedFieldsForFormAsync(connection, idForm);
        return await connection.QueryAsync<FormField>(sql, new { IdForm = idForm });
    }

    private async Task SeedFieldsForFormAsync(System.Data.IDbConnection connection, long formId)
    {
        try
        {
            const string insertFieldSql = @"
                INSERT INTO sales_service.sales_form_field 
                (id_form, label, field_key, field_type, is_required, validation_type, placeholder, help_text, options, order_index, is_active)
                VALUES 
                (@IdForm, @Label, @FieldKey, @FieldType, @IsRequired, @ValidationType, @Placeholder, @HelpText, @Options, @OrderIndex, true);";

            var defaultFields = new[]
            {
                new { IdForm = formId, Label = "DNI / NIE del Titular", FieldKey = "dni_titular", FieldType = "text", IsRequired = true, ValidationType = (string?)"DNI_ES", Placeholder = (string?)"12345678Z", HelpText = (string?)"DNI español (8 números + 1 letra, Módulo 23)", Options = (string?)null, OrderIndex = 1 },
                new { IdForm = formId, Label = "Cuenta Bancaria (IBAN)", FieldKey = "iban_cuenta", FieldType = "text", IsRequired = true, ValidationType = (string?)"IBAN", Placeholder = (string?)"ES9121000418450200051332", HelpText = (string?)"IBAN español de 24 caracteres (Módulo 97)", Options = (string?)null, OrderIndex = 2 },
                new { IdForm = formId, Label = "Nombre Completo del Titular", FieldKey = "nombre_titular", FieldType = "text", IsRequired = true, ValidationType = (string?)null, Placeholder = (string?)"Juan Pérez García", HelpText = (string?)"Nombre y apellidos completos del cliente", Options = (string?)null, OrderIndex = 3 },
                new { IdForm = formId, Label = "Teléfono de Contacto", FieldKey = "telefono_contacto", FieldType = "text", IsRequired = true, ValidationType = (string?)"PHONE_ES", Placeholder = (string?)"612345678", HelpText = (string?)"Teléfono móvil o fijo de 9 dígitos", Options = (string?)null, OrderIndex = 4 },
                new { IdForm = formId, Label = "Código CUPS (Opcional)", FieldKey = "cups_suministro", FieldType = "text", IsRequired = false, ValidationType = (string?)"CUPS_ENERGY", Placeholder = (string?)"ES0031405012345678NN1F", HelpText = (string?)"Código CUPS de 22 caracteres", Options = (string?)null, OrderIndex = 5 },
                new { IdForm = formId, Label = "Tipo de Contrato / Tarifa", FieldKey = "tipo_contrato", FieldType = "select", IsRequired = true, ValidationType = (string?)null, Placeholder = (string?)"-- Selecciona --", HelpText = (string?)"Tarifa o producto a contratar", Options = (string?)"[\"Fibra 600Mb + Móvil 50GB\", \"Fibra 1Gb + 2 Líneas Móviles\", \"Solo Móvil 100GB\", \"Luz + Gas Residencial\"]", OrderIndex = 6 },
                new { IdForm = formId, Label = "Fecha Preferida de Instalación", FieldKey = "fecha_instalacion", FieldType = "date", IsRequired = false, ValidationType = (string?)null, Placeholder = (string?)null, HelpText = (string?)"Fecha agendada para el alta técnico", Options = (string?)null, OrderIndex = 7 }
            };

            foreach (var f in defaultFields)
            {
                await connection.ExecuteAsync(insertFieldSql, f);
            }
        }
        catch { }
    }

    public async Task SaveOrderDataAsync(long idOrder, long idForm, IEnumerable<OrderData> fields)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.sales_order_data 
            (id_order, id_fld, value_text, value_json, field_status, version, source_form_id, register) 
            VALUES 
            (@IdOrder, @IdFld, @ValueText, @ValueJson::jsonb, @FieldStatus, 1, @SourceFormId, NOW());";

        foreach (var field in fields)
        {
            field.IdOrder = idOrder;
            field.SourceFormId = idForm;
            if (string.IsNullOrEmpty(field.FieldStatus)) field.FieldStatus = "PENDING"; 
        }
        
        await connection.ExecuteAsync(sql, fields);
    }

    public async Task SeedDefaultFormsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        try
        {
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sales_service.sales_form_template;");
            if (count > 0) return;

            var campaigns = (await connection.QueryAsync<long>("SELECT id_cmpg FROM campaign_service.campaign WHERE is_active = true;")).ToList();
            if (!campaigns.Any()) campaigns.Add(1);

            var stages = (await connection.QueryAsync<long>("SELECT id_status FROM sales_service.order_status WHERE is_active = true;")).ToList();
            if (!stages.Any()) stages.Add(1);

            foreach (var cmpgId in campaigns)
            {
                foreach (var stageId in stages)
                {
                    const string insertTemplateSql = @"
                        INSERT INTO sales_service.sales_form_template 
                        (id_cmpg, id_stage, name, description, is_active, form_order, is_primary, allows_partial, min_completion_pct, register)
                        VALUES 
                        (@IdCmpg, @IdStage, @Name, @Description, true, 1, true, true, 80, NOW())
                        RETURNING id_form;";

                    long formId = await connection.ExecuteScalarAsync<long>(insertTemplateSql, new {
                        IdCmpg = cmpgId,
                        IdStage = stageId,
                        Name = "Formulario de Ventas y Alta de Servicio",
                        Description = "Formulario dinámico de contratación con validación DNI y IBAN"
                    });

                    const string insertFieldSql = @"
                        INSERT INTO sales_service.sales_form_field 
                        (id_form, label, field_key, field_type, is_required, validation_type, placeholder, help_text, options, order_index, is_active)
                        VALUES 
                        (@IdForm, @Label, @FieldKey, @FieldType, @IsRequired, @ValidationType, @Placeholder, @HelpText, @Options, @OrderIndex, true);";

                    var fields = new[]
                    {
                        new { IdForm = formId, Label = "DNI / NIE del Titular", FieldKey = "dni_titular", FieldType = "text", IsRequired = true, ValidationType = (string?)"DNI_ES", Placeholder = (string?)"12345678Z", HelpText = (string?)"DNI español (8 números + 1 letra, Módulo 23)", Options = (string?)null, OrderIndex = 1 },
                        new { IdForm = formId, Label = "Cuenta Bancaria (IBAN)", FieldKey = "iban_cuenta", FieldType = "text", IsRequired = true, ValidationType = (string?)"IBAN", Placeholder = (string?)"ES9121000418450200051332", HelpText = (string?)"IBAN español de 24 caracteres (Módulo 97)", Options = (string?)null, OrderIndex = 2 },
                        new { IdForm = formId, Label = "Nombre Completo del Titular", FieldKey = "nombre_titular", FieldType = "text", IsRequired = true, ValidationType = (string?)null, Placeholder = (string?)"Juan Pérez García", HelpText = (string?)"Nombre y apellidos completos del cliente", Options = (string?)null, OrderIndex = 3 },
                        new { IdForm = formId, Label = "Teléfono de Contacto", FieldKey = "telefono_contacto", FieldType = "text", IsRequired = true, ValidationType = (string?)"PHONE_ES", Placeholder = (string?)"612345678", HelpText = (string?)"Teléfono móvil o fijo de 9 dígitos", Options = (string?)null, OrderIndex = 4 },
                        new { IdForm = formId, Label = "Código CUPS (Opcional)", FieldKey = "cups_suministro", FieldType = "text", IsRequired = false, ValidationType = (string?)"CUPS_ENERGY", Placeholder = (string?)"ES0031405012345678NN1F", HelpText = (string?)"Código CUPS de 22 caracteres", Options = (string?)null, OrderIndex = 5 },
                        new { IdForm = formId, Label = "Tipo de Contrato / Tarifa", FieldKey = "tipo_contrato", FieldType = "select", IsRequired = true, ValidationType = (string?)null, Placeholder = (string?)"-- Selecciona --", HelpText = (string?)"Tarifa o producto a contratar", Options = (string?)"[\"Fibra 600Mb + Móvil 50GB\", \"Fibra 1Gb + 2 Líneas Móviles\", \"Solo Móvil 100GB\", \"Luz + Gas Residencial\"]", OrderIndex = 6 },
                        new { IdForm = formId, Label = "Fecha Preferida de Instalación", FieldKey = "fecha_instalacion", FieldType = "date", IsRequired = false, ValidationType = (string?)null, Placeholder = (string?)null, HelpText = (string?)"Fecha agendada para el alta técnico", Options = (string?)null, OrderIndex = 7 }
                    };

                    foreach (var f in fields)
                    {
                        await connection.ExecuteAsync(insertFieldSql, f);
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // Ignorar errores si la tabla ya contenía o faltaban restricciones en esquemas de test
        }
    }
}
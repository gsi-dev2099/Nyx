using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Dapper;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CurrencyRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Currency>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                code AS Code,
                name AS Name,
                symbol AS Symbol,
                decimal_places AS DecimalPlaces,
                is_active AS IsActive,
                is_base AS IsBase,
                is_local AS IsLocal
            FROM sales_service.currency
            WHERE is_active = true;";

        return await connection.QueryAsync<Currency>(
            new CommandDefinition(sql, cancellationToken: ct)
        );
    }

    public async Task<decimal> GetExchangeRateAsync(string from, string to, DateTime date, CancellationToken ct = default)
    {
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
        {
            return 1.0m;
        }

        using var connection = _connectionFactory.CreateConnection();
        
        // 1. Try direct exchange rate
        const string directSql = @"
            SELECT rate 
            FROM sales_service.exchange_rate 
            WHERE from_currency = @From AND to_currency = @To 
              AND valid_from <= @Date 
              AND (valid_to IS NULL OR valid_to >= @Date)
            ORDER BY valid_from DESC 
            LIMIT 1;";

        var directRate = await connection.QueryFirstOrDefaultAsync<decimal?>(
            new CommandDefinition(directSql, new { From = from.ToUpper(), To = to.ToUpper(), Date = date.Date }, cancellationToken: ct)
        );

        if (directRate.HasValue)
        {
            return directRate.Value;
        }

        // 2. Try inverse exchange rate
        const string inverseSql = @"
            SELECT rate 
            FROM sales_service.exchange_rate 
            WHERE from_currency = @To AND to_currency = @From 
              AND valid_from <= @Date 
              AND (valid_to IS NULL OR valid_to >= @Date)
            ORDER BY valid_from DESC 
            LIMIT 1;";

        var inverseRate = await connection.QueryFirstOrDefaultAsync<decimal?>(
            new CommandDefinition(inverseSql, new { From = from.ToUpper(), To = to.ToUpper(), Date = date.Date }, cancellationToken: ct)
        );

        if (inverseRate.HasValue && inverseRate.Value > 0)
        {
            return 1.0m / inverseRate.Value;
        }

        // Fallbacks standard
        if (from.ToUpper() == "EUR" && to.ToUpper() == "PEN") return 4.0m;
        if (from.ToUpper() == "PEN" && to.ToUpper() == "EUR") return 0.25m;

        throw new InvalidOperationException($"No se encontró tasa de cambio de {from} a {to} para la fecha {date:yyyy-MM-dd}.");
    }

    public async Task<decimal> ConvertAmountAsync(decimal amount, string from, string to, DateTime date, CancellationToken ct = default)
    {
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
        {
            return amount;
        }

        var rate = await GetExchangeRateAsync(from, to, date, ct);
        return amount * rate;
    }
}

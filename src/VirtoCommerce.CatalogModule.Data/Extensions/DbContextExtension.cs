using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace VirtoCommerce.CatalogModule.Data.Extensions;

public static class DbContextExtension
{
    public static async Task<T[]> ExecuteEntityArrayAsync<T>(this DbContext context, string rawSql, params object[] parameters) where T : new()
    {
        var conn = context.Database.GetDbConnection();
        await using var command = conn.CreateCommand();

        command.CommandText = rawSql;
        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                command.Parameters.Add(p);
            }
        }

        // Bind command to the current DbContext transaction if one exists
        if (context.Database.CurrentTransaction != null)
        {
            command.Transaction = context.Database.CurrentTransaction.GetDbTransaction();
        }

        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        var result = new List<T>();

        await using var reader = await command.ExecuteReaderAsync();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite).ToList();

        while (await reader.ReadAsync())
        {
            var entity = new T();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var property = properties.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));
                if (property != null && !await reader.IsDBNullAsync(i))
                {
                    var value = await reader.GetFieldValueAsync<object>(i);
                    property.SetValue(entity, Convert.ChangeType(value, property.PropertyType));
                }
            }
            result.Add(entity);
        }

        return result.ToArray();
    }
}

using System.Data;
using Dapper;

namespace Abank.Usuarios.Infrastructure.Persistence;

// Dapper todavía no sabe convertir DateOnly por su cuenta
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value;
    }

    public override DateOnly Parse(object value) => value switch
    {
        DateOnly fecha => fecha,
        DateTime fecha => DateOnly.FromDateTime(fecha),
        _ => DateOnly.Parse(value.ToString()!)
    };
}

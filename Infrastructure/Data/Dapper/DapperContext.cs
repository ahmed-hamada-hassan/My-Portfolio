using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

using Application.Interfaces.DI;

namespace Infrastructure.Data.Dapper;

public sealed class DapperContext : IScopedMark
{
    private readonly IConfiguration _configuration;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection"));
    }
}

using NotoriousTest.Core.Infrastructures;

using System.Data.Common;

namespace NotoriousTest.Database
{
    public interface IDatabaseInfrastructure : IInfrastructure
    {
        string DbPrefix { get; init; }
        string[] TableToIgnore { get; init; }
        string[] TableToInclude { get; init; }

        DbConnection GetDatabaseConnection();
        DbConnection GetServerConnection();
        string GetDatabaseConnectionString();
        string GetServerConnectionString();
    }
}
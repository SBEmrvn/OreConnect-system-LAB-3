using System.Text.Json;
using Npgsql;

namespace OreConnect.Desktop.Models.Data;

/// <summary>Opens PostgreSQL connections and creates the tables on first run.</summary>
public class Database
{
    private readonly string _connectionString;

    public Database(string connectionString) => _connectionString = connectionString;

    /// <summary>
    /// Reads the connection string from the OREC_DB environment variable, or from
    /// appsettings.json next to the .exe: { "ConnectionString": "Host=...;Database=...;Username=...;Password=..." }
    /// </summary>
    public static Database FromConfiguration()
    {
        var fromEnv = Environment.GetEnvironmentVariable("OREC_DB");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return new Database(fromEnv);

        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            throw new InvalidOperationException(
                "No database settings found. Copy appsettings.example.json to appsettings.json and fill in your PostgreSQL password.");

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var cs = doc.RootElement.GetProperty("ConnectionString").GetString();
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionString in appsettings.json is empty.");
        return new Database(cs);
    }

    public NpgsqlConnection Open()
    {
        var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public void EnsureSchema()
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS samples (
                id              SERIAL PRIMARY KEY,
                sample_code     VARCHAR(32) UNIQUE,
                client_name     VARCHAR(150) NOT NULL,
                source_location VARCHAR(200) NOT NULL,
                date_received   DATE NOT NULL,
                weight_kg       NUMERIC(12,3) NOT NULL CHECK (weight_kg > 0),
                sample_type     VARCHAR(80) NOT NULL,
                status          VARCHAR(20) NOT NULL DEFAULT 'Received'
            );
            CREATE TABLE IF NOT EXISTS listings (
                id               SERIAL PRIMARY KEY,
                sample_id        INTEGER REFERENCES samples(id) ON DELETE RESTRICT,
                mineral_type     VARCHAR(80) NOT NULL,
                quantity_tonnes  NUMERIC(14,3) NOT NULL CHECK (quantity_tonnes > 0),
                grade_percent    NUMERIC(6,3) NOT NULL CHECK (grade_percent BETWEEN 0 AND 100),
                location         VARCHAR(200) NOT NULL,
                price_per_tonne  NUMERIC(14,2) NOT NULL CHECK (price_per_tonne > 0),
                status           VARCHAR(20) NOT NULL DEFAULT 'Draft'
            );
            """;
        using var conn = Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }
}

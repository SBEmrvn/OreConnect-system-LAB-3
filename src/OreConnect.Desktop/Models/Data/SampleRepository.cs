using Npgsql;

namespace OreConnect.Desktop.Models.Data;

public interface ISampleRepository
{
    List<Sample> GetAll(string? search = null);
    Sample? GetById(int id);
    Sample Add(Sample sample);
    bool Update(Sample sample);
    bool Delete(int id);
    bool HasListings(int id);
}

/// <summary>CRUD for the samples table. All SQL uses parameters (no string concatenation of user input).</summary>
public class SampleRepository : ISampleRepository
{
    private readonly Database _db;
    private const string Columns = "id, sample_code, client_name, source_location, date_received, weight_kg, sample_type, status";

    public SampleRepository(Database db) => _db = db;

    public List<Sample> GetAll(string? search = null)
    {
        using var conn = _db.Open();
        var sql = $"SELECT {Columns} FROM samples";
        using var cmd = new NpgsqlCommand { Connection = conn };
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " WHERE sample_code ILIKE @q OR client_name ILIKE @q OR source_location ILIKE @q";
            cmd.Parameters.AddWithValue("q", $"%{search.Trim()}%");
        }
        cmd.CommandText = sql + " ORDER BY id DESC";
        var list = new List<Sample>();
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Read(r));
        return list;
    }

    public Sample? GetById(int id)
    {
        using var conn = _db.Open();
        using var cmd = new NpgsqlCommand($"SELECT {Columns} FROM samples WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Read(r) : null;
    }

    public Sample Add(Sample s)
    {
        using var conn = _db.Open();
        using var tx = conn.BeginTransaction();
        using var insert = new NpgsqlCommand("""
            INSERT INTO samples (client_name, source_location, date_received, weight_kg, sample_type, status)
            VALUES (@client, @location, @date, @weight, @type, @status) RETURNING id
            """, conn, tx);
        Bind(insert, s);
        s.Id = (int)insert.ExecuteScalar()!;

        // Unique ID derived from the primary key, so two samples can never share it (FR-2.2).
        s.SampleCode = $"OC-{s.DateReceived.Year}-{s.Id:D6}";
        using var code = new NpgsqlCommand("UPDATE samples SET sample_code = @code WHERE id = @id", conn, tx);
        code.Parameters.AddWithValue("code", s.SampleCode);
        code.Parameters.AddWithValue("id", s.Id);
        code.ExecuteNonQuery();
        tx.Commit();
        return s;
    }

    public bool Update(Sample s)
    {
        using var conn = _db.Open();
        using var cmd = new NpgsqlCommand("""
            UPDATE samples SET client_name = @client, source_location = @location, date_received = @date,
                   weight_kg = @weight, sample_type = @type, status = @status
            WHERE id = @id
            """, conn);
        Bind(cmd, s);
        cmd.Parameters.AddWithValue("id", s.Id);
        return cmd.ExecuteNonQuery() == 1;
    }

    public bool Delete(int id)
    {
        using var conn = _db.Open();
        using var cmd = new NpgsqlCommand("DELETE FROM samples WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return cmd.ExecuteNonQuery() == 1;
    }

    public bool HasListings(int id)
    {
        using var conn = _db.Open();
        using var cmd = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM listings WHERE sample_id = @id)", conn);
        cmd.Parameters.AddWithValue("id", id);
        return (bool)cmd.ExecuteScalar()!;
    }

    private static void Bind(NpgsqlCommand cmd, Sample s)
    {
        cmd.Parameters.AddWithValue("client", s.ClientName.Trim());
        cmd.Parameters.AddWithValue("location", s.SourceLocation.Trim());
        cmd.Parameters.AddWithValue("date", DateOnly.FromDateTime(s.DateReceived));
        cmd.Parameters.AddWithValue("weight", s.WeightKg);
        cmd.Parameters.AddWithValue("type", s.SampleType);
        cmd.Parameters.AddWithValue("status", s.Status.ToString());
    }

    private static Sample Read(NpgsqlDataReader r) => new()
    {
        Id = r.GetInt32(0),
        SampleCode = r.IsDBNull(1) ? "" : r.GetString(1),
        ClientName = r.GetString(2),
        SourceLocation = r.GetString(3),
        DateReceived = r.GetFieldValue<DateOnly>(4).ToDateTime(TimeOnly.MinValue),
        WeightKg = r.GetDecimal(5),
        SampleType = r.GetString(6),
        Status = Enum.Parse<SampleStatus>(r.GetString(7)),
    };
}

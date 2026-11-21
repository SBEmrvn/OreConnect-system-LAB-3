using Npgsql;

namespace OreConnect.Desktop.Models.Data;

public interface IListingRepository
{
    List<Listing> GetAll(string? search = null);
    Listing Add(Listing listing);
    bool Update(Listing listing);
    bool Delete(int id);
}

public class ListingRepository : IListingRepository
{
    private readonly Database _db;

    public ListingRepository(Database db) => _db = db;

    public List<Listing> GetAll(string? search = null)
    {
        using var conn = _db.Open();
        var sql = """
            SELECT l.id, l.sample_id, s.sample_code, l.mineral_type, l.quantity_tonnes, l.grade_percent,
                   l.location, l.price_per_tonne, l.status
            FROM listings l LEFT JOIN samples s ON s.id = l.sample_id
            """;
        using var cmd = new NpgsqlCommand { Connection = conn };
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " WHERE l.mineral_type ILIKE @q OR l.location ILIKE @q OR s.sample_code ILIKE @q";
            cmd.Parameters.AddWithValue("q", $"%{search.Trim()}%");
        }
        cmd.CommandText = sql + " ORDER BY l.id DESC";
        var list = new List<Listing>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new Listing
            {
                Id = r.GetInt32(0),
                SampleId = r.IsDBNull(1) ? null : r.GetInt32(1),
                SampleCode = r.IsDBNull(2) ? null : r.GetString(2),
                MineralType = r.GetString(3),
                QuantityTonnes = r.GetDecimal(4),
                GradePercent = r.GetDecimal(5),
                Location = r.GetString(6),
                PricePerTonne = r.GetDecimal(7),
                Status = Enum.Parse<ListingStatus>(r.GetString(8)),
            });
        }
        return list;
    }

    public Listing Add(Listing l)
    {
        using var conn = _db.Open();
        using var cmd = new NpgsqlCommand("""
            INSERT INTO listings (sample_id, mineral_type, quantity_tonnes, grade_percent, location, price_per_tonne, status)
            VALUES (@sample, @mineral, @qty, @grade, @location, @price, @status) RETURNING id
            """, conn);
        Bind(cmd, l);
        l.Id = (int)cmd.ExecuteScalar()!;
        return l;
    }

    public bool Update(Listing l)
    {
        using var conn = _db.Open();
        using var cmd = new NpgsqlCommand("""
            UPDATE listings SET sample_id = @sample, mineral_type = @mineral, quantity_tonnes = @qty,
                   grade_percent = @grade, location = @location, price_per_tonne = @price, status = @status
            WHERE id = @id
            """, conn);
        Bind(cmd, l);
        cmd.Parameters.AddWithValue("id", l.Id);
        return cmd.ExecuteNonQuery() == 1;
    }

    public bool Delete(int id)
    {
        using var conn = _db.Open();
        using var cmd = new NpgsqlCommand("DELETE FROM listings WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return cmd.ExecuteNonQuery() == 1;
    }

    private static void Bind(NpgsqlCommand cmd, Listing l)
    {
        cmd.Parameters.AddWithValue("sample", (object?)l.SampleId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("mineral", l.MineralType.Trim());
        cmd.Parameters.AddWithValue("qty", l.QuantityTonnes);
        cmd.Parameters.AddWithValue("grade", l.GradePercent);
        cmd.Parameters.AddWithValue("location", l.Location.Trim());
        cmd.Parameters.AddWithValue("price", l.PricePerTonne);
        cmd.Parameters.AddWithValue("status", l.Status.ToString());
    }
}

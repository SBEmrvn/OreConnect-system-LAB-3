using OreConnect.Desktop.Models;
using OreConnect.Desktop.Models.Data;

namespace OreConnect.Desktop.Tests;

/// <summary>
/// Integration tests against a real PostgreSQL database.
/// Set the OREC_TEST_DB environment variable to a connection string for an EMPTY test database
/// (tables in it are wiped). Without it these tests pass without doing anything.
/// </summary>
public class RepositoryTests
{
    private static Database? TestDb()
    {
        var cs = Environment.GetEnvironmentVariable("OREC_TEST_DB");
        if (string.IsNullOrWhiteSpace(cs)) return null;
        var db = new Database(cs);
        db.EnsureSchema();
        using var conn = db.Open();
        using var cmd = new Npgsql.NpgsqlCommand("TRUNCATE listings, samples RESTART IDENTITY", conn);
        cmd.ExecuteNonQuery();
        return db;
    }

    private static Sample NewSample(string client = "Kivu Mining") => new()
    {
        ClientName = client, SourceLocation = "Rutsiro", DateReceived = new DateTime(2026, 10, 1),
        WeightKg = 2.5m, SampleType = "Raw ore",
    };

    [Fact]
    public void Sample_crud_round_trip()
    {
        var db = TestDb();
        if (db is null) return;
        var repo = new SampleRepository(db);

        var a = repo.Add(NewSample());
        var b = repo.Add(NewSample("Second"));
        Assert.Equal("OC-2026-000001", a.SampleCode);
        Assert.NotEqual(a.SampleCode, b.SampleCode);

        a.Status = SampleStatus.Approved;
        a.WeightKg = 3.125m;
        Assert.True(repo.Update(a));
        var loaded = repo.GetById(a.Id)!;
        Assert.Equal(SampleStatus.Approved, loaded.Status);
        Assert.Equal(3.125m, loaded.WeightKg);

        Assert.Single(repo.GetAll("second"));
        Assert.True(repo.Delete(b.Id));
        Assert.Null(repo.GetById(b.Id));
    }

    [Fact]
    public void Search_treats_input_as_text_not_sql()
    {
        var db = TestDb();
        if (db is null) return;
        var repo = new SampleRepository(db);
        repo.Add(NewSample());

        Assert.Empty(repo.GetAll("' OR 1=1 --"));
        Assert.Single(repo.GetAll());
    }

    [Fact]
    public void Listing_links_to_sample()
    {
        var db = TestDb();
        if (db is null) return;
        var samples = new SampleRepository(db);
        var listings = new ListingRepository(db);
        var s = samples.Add(NewSample());

        listings.Add(new Listing { SampleId = s.Id, MineralType = "Cassiterite", QuantityTonnes = 20, GradePercent = 71.5m, Location = "Rutsiro", PricePerTonne = 18500 });

        var all = listings.GetAll();
        Assert.Equal(s.SampleCode, all.Single().SampleCode);
        Assert.True(samples.HasListings(s.Id));
    }
}

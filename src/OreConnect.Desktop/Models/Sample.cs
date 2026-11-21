namespace OreConnect.Desktop.Models;

public enum SampleStatus
{
    Received,
    Analysed,
    UnderReview,
    Approved,
    Rejected
}

/// <summary>A mineral sample registered by the laboratory (OreConnect FR-2).</summary>
public class Sample
{
    public static readonly string[] SampleTypes =
        { "Ore concentrate", "Raw ore", "Tailings", "Rock chip", "Alluvial", "Other" };

    public int Id { get; set; }

    /// <summary>Unique ID such as OC-2026-000001, assigned by the database layer (FR-2.2).</summary>
    public string SampleCode { get; set; } = "";

    public string ClientName { get; set; } = "";
    public string SourceLocation { get; set; } = "";
    public DateTime DateReceived { get; set; } = DateTime.Today;
    public decimal WeightKg { get; set; }
    public string SampleType { get; set; } = SampleTypes[0];
    public SampleStatus Status { get; set; } = SampleStatus.Received;

    /// <summary>Business rules for a sample. Returns an empty list when the sample is valid.</summary>
    public List<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(ClientName) || ClientName.Trim().Length < 2)
            errors.Add("Client name must have at least 2 characters.");
        if (ClientName.Length > 150)
            errors.Add("Client name must be 150 characters or fewer.");
        if (string.IsNullOrWhiteSpace(SourceLocation) || SourceLocation.Trim().Length < 2)
            errors.Add("Source location must have at least 2 characters.");
        if (DateReceived.Date > DateTime.Today)
            errors.Add("Date received cannot be in the future.");
        if (WeightKg <= 0 || WeightKg > 100_000)
            errors.Add("Weight must be greater than 0 and at most 100,000 kg.");
        if (string.IsNullOrWhiteSpace(SampleType))
            errors.Add("Sample type is required.");
        return errors;
    }

    public static string StatusText(SampleStatus s) => s == SampleStatus.UnderReview ? "Under Review" : s.ToString();
}

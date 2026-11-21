namespace OreConnect.Desktop.Models;

public enum ListingStatus
{
    Draft,
    Published
}

/// <summary>A marketplace offer made by a mining company (OreConnect FR-7).</summary>
public class Listing
{
    public int Id { get; set; }

    /// <summary>The analysed sample that backs this listing. Optional for a draft.</summary>
    public int? SampleId { get; set; }

    /// <summary>Read-only display value joined from the samples table.</summary>
    public string? SampleCode { get; set; }

    public string MineralType { get; set; } = "";
    public decimal QuantityTonnes { get; set; }
    public decimal GradePercent { get; set; }
    public string Location { get; set; } = "";
    public decimal PricePerTonne { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Draft;

    public List<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(MineralType) || MineralType.Trim().Length < 2)
            errors.Add("Mineral type must have at least 2 characters.");
        if (QuantityTonnes <= 0)
            errors.Add("Quantity must be greater than 0 tonnes.");
        if (GradePercent < 0 || GradePercent > 100)
            errors.Add("Grade must be between 0 and 100%.");
        if (string.IsNullOrWhiteSpace(Location) || Location.Trim().Length < 2)
            errors.Add("Location must have at least 2 characters.");
        if (PricePerTonne <= 0)
            errors.Add("Price per tonne must be greater than 0.");
        return errors;
    }
}

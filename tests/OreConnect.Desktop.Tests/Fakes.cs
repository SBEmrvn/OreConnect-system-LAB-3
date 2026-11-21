using OreConnect.Desktop.Models;
using OreConnect.Desktop.Models.Data;
using OreConnect.Desktop.Views;

namespace OreConnect.Desktop.Tests;

/// <summary>In-memory repository so controllers can be tested without a database.</summary>
internal class FakeSampleRepository : ISampleRepository
{
    public readonly List<Sample> Items = new();
    public readonly HashSet<int> WithListings = new();
    private int _next = 1;

    public List<Sample> GetAll(string? search = null) =>
        Items.Where(s => string.IsNullOrWhiteSpace(search) || s.ClientName.Contains(search, StringComparison.OrdinalIgnoreCase)
                         || s.SampleCode.Contains(search, StringComparison.OrdinalIgnoreCase))
             .OrderByDescending(s => s.Id).ToList();
    public Sample? GetById(int id) => Items.FirstOrDefault(s => s.Id == id);
    public Sample Add(Sample s) { s.Id = _next++; s.SampleCode = $"OC-{s.DateReceived.Year}-{s.Id:D6}"; Items.Add(s); return s; }
    public bool Update(Sample s)
    {
        var i = Items.FindIndex(x => x.Id == s.Id);
        if (i < 0) return false;
        s.SampleCode = Items[i].SampleCode;
        Items[i] = s;
        return true;
    }
    public bool Delete(int id) => Items.RemoveAll(s => s.Id == id) == 1;
    public bool HasListings(int id) => WithListings.Contains(id);
}

internal class FakeListingRepository : IListingRepository
{
    public readonly List<Listing> Items = new();
    private int _next = 1;
    public List<Listing> GetAll(string? search = null) => Items.ToList();
    public Listing Add(Listing l) { l.Id = _next++; Items.Add(l); return l; }
    public bool Update(Listing l) { var i = Items.FindIndex(x => x.Id == l.Id); if (i < 0) return false; Items[i] = l; return true; }
    public bool Delete(int id) => Items.RemoveAll(l => l.Id == id) == 1;
}

/// <summary>A view that records what the controller asked it to show.</summary>
internal class FakeSampleView : ISampleView
{
    public int? SelectedId { get; set; }
    public string ClientName { get; set; } = "Kivu Mining Ltd";
    public string SourceLocation { get; set; } = "Rutsiro";
    public DateTime DateReceived { get; set; } = DateTime.Today;
    public string WeightText { get; set; } = "2.5";
    public string SampleType { get; set; } = "Raw ore";
    public SampleStatus Status { get; set; } = SampleStatus.Received;
    public string SearchText { get; set; } = "";
    public bool ConfirmAnswer { get; set; } = true;

    public List<string> Errors { get; } = new();
    public string? Info { get; private set; }
    public IReadOnlyList<Sample> Shown { get; private set; } = Array.Empty<Sample>();
    public Sample? Filled { get; private set; }
    public bool Cleared { get; private set; }

    public event EventHandler? SaveClicked;
    public event EventHandler? UpdateClicked;
    public event EventHandler? DeleteClicked;
    public event EventHandler? DisplayClicked;
    public event EventHandler? ClearClicked;
    public event EventHandler<int>? RowSelected;

    public void ClickSave() => SaveClicked?.Invoke(this, EventArgs.Empty);
    public void ClickUpdate() => UpdateClicked?.Invoke(this, EventArgs.Empty);
    public void ClickDelete() => DeleteClicked?.Invoke(this, EventArgs.Empty);
    public void ClickDisplay() => DisplayClicked?.Invoke(this, EventArgs.Empty);
    public void ClickClear() => ClearClicked?.Invoke(this, EventArgs.Empty);
    public void SelectRow(int id) { SelectedId = id; RowSelected?.Invoke(this, id); }

    public void ShowSamples(IReadOnlyList<Sample> samples) => Shown = samples;
    public void FillFields(Sample sample) { Filled = sample; SelectedId = sample.Id; }
    public void ClearFields() { Cleared = true; SelectedId = null; }
    public void ShowInfo(string message) => Info = message;
    public void ShowErrors(IEnumerable<string> errors) => Errors.AddRange(errors);
    public bool Confirm(string message) => ConfirmAnswer;
}

#pragma warning disable CS0067 // events required by the interface but not raised in these tests
internal class FakeListingView : IListingView
{
    public int? SelectedId { get; set; }
    public int? LinkedSampleId { get; set; }
    public string MineralType { get; set; } = "Cassiterite";
    public string QuantityText { get; set; } = "20";
    public string GradeText { get; set; } = "71.5";
    public string ListingLocation { get; set; } = "Rutsiro";
    public string PriceText { get; set; } = "18500";
    public string SearchText { get; set; } = "";

    public List<string> Errors { get; } = new();
    public string? Info { get; private set; }
    public IReadOnlyList<Listing> Shown { get; private set; } = Array.Empty<Listing>();

    public event EventHandler? SaveClicked;
    public event EventHandler? UpdateClicked;
    public event EventHandler? DeleteClicked;
    public event EventHandler? DisplayClicked;
    public event EventHandler? ClearClicked;
    public event EventHandler? PublishClicked;
    public event EventHandler<int>? RowSelected;

    public void ClickSave() => SaveClicked?.Invoke(this, EventArgs.Empty);
    public void ClickPublish() => PublishClicked?.Invoke(this, EventArgs.Empty);
    public void ClickDisplay() => DisplayClicked?.Invoke(this, EventArgs.Empty);

    public void ShowListings(IReadOnlyList<Listing> listings) => Shown = listings;
    public void ShowSampleChoices(IReadOnlyList<Sample> samples) { }
    public void FillFields(Listing listing) => SelectedId = listing.Id;
    public void ClearFields() => SelectedId = null;
    public void ShowInfo(string message) => Info = message;
    public void ShowErrors(IEnumerable<string> errors) => Errors.AddRange(errors);
    public bool Confirm(string message) => true;
}

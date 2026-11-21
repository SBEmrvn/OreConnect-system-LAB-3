using Npgsql;
using OreConnect.Desktop.Models;
using OreConnect.Desktop.Models.Data;
using OreConnect.Desktop.Views;

namespace OreConnect.Desktop.Controllers;

/// <summary>Marketplace listings. A listing can be published only when its sample is Approved (FR-7.2).</summary>
public class ListingController
{
    private readonly IListingView _view;
    private readonly IListingRepository _listings;
    private readonly ISampleRepository _samples;
    private List<Listing> _current = new();

    public ListingController(IListingView view, IListingRepository listings, ISampleRepository samples)
    {
        _view = view;
        _listings = listings;
        _samples = samples;
        _view.SaveClicked += (_, _) => Save();
        _view.UpdateClicked += (_, _) => Update();
        _view.DeleteClicked += (_, _) => Delete();
        _view.DisplayClicked += (_, _) => Display();
        _view.PublishClicked += (_, _) => Publish();
        _view.ClearClicked += (_, _) => _view.ClearFields();
        _view.RowSelected += (_, id) => Select(id);
    }

    public void RefreshSampleChoices() => Run(() => _view.ShowSampleChoices(_samples.GetAll()));

    private Listing? ReadForm()
    {
        var errors = new List<string>();
        if (!InputParser.TryDecimal(_view.QuantityText, out var qty)) errors.Add("Quantity must be a number.");
        if (!InputParser.TryDecimal(_view.GradeText, out var grade)) errors.Add("Grade must be a number.");
        if (!InputParser.TryDecimal(_view.PriceText, out var price)) errors.Add("Price must be a number.");
        var listing = new Listing
        {
            Id = _view.SelectedId ?? 0,
            SampleId = _view.LinkedSampleId,
            MineralType = _view.MineralType,
            QuantityTonnes = qty,
            GradePercent = grade,
            Location = _view.ListingLocation,
            PricePerTonne = price,
        };
        if (errors.Count == 0) errors.AddRange(listing.Validate());
        if (errors.Count > 0)
        {
            _view.ShowErrors(errors);
            return null;
        }
        return listing;
    }

    public void Save()
    {
        var listing = ReadForm();
        if (listing is null) return;
        listing.Status = ListingStatus.Draft;
        Run(() =>
        {
            _listings.Add(listing);
            _view.ShowInfo("Listing saved as a draft. Publish it once its sample is Approved.");
            _view.ClearFields();
            Display();
        });
    }

    public void Update()
    {
        if (_view.SelectedId is not int id)
        {
            _view.ShowErrors(new[] { "Select a listing in the table first." });
            return;
        }
        var listing = ReadForm();
        if (listing is null) return;
        Run(() =>
        {
            var existing = _current.FirstOrDefault(l => l.Id == id);
            listing.Status = existing?.Status ?? ListingStatus.Draft;
            // A published listing must stay backed by an approved sample.
            if (listing.Status == ListingStatus.Published && !SampleIsApproved(listing.SampleId))
                listing.Status = ListingStatus.Draft;
            _listings.Update(listing);
            _view.ShowInfo(listing.Status == ListingStatus.Draft && existing?.Status == ListingStatus.Published
                ? "Listing updated and moved back to Draft because its sample is not Approved."
                : "Listing updated.");
            Display();
        });
    }

    public void Publish()
    {
        if (_view.SelectedId is not int id)
        {
            _view.ShowErrors(new[] { "Select a listing in the table first." });
            return;
        }
        Run(() =>
        {
            var listing = _current.FirstOrDefault(l => l.Id == id);
            if (listing is null) return;
            if (!SampleIsApproved(listing.SampleId))
            {
                _view.ShowErrors(new[] { "A listing can only be published when it is linked to an Approved sample." });
                return;
            }
            listing.Status = ListingStatus.Published;
            _listings.Update(listing);
            _view.ShowInfo("Listing published.");
            Display();
        });
    }

    public void Delete()
    {
        if (_view.SelectedId is not int id)
        {
            _view.ShowErrors(new[] { "Select a listing in the table first." });
            return;
        }
        if (!_view.Confirm("Delete the selected listing?")) return;
        Run(() =>
        {
            _listings.Delete(id);
            _view.ShowInfo("Listing deleted.");
            _view.ClearFields();
            Display();
        });
    }

    public void Display() => Run(() =>
    {
        _current = _listings.GetAll(_view.SearchText);
        _view.ShowListings(_current);
    });

    private void Select(int id)
    {
        var l = _current.FirstOrDefault(x => x.Id == id);
        if (l is not null) _view.FillFields(l);
    }

    private bool SampleIsApproved(int? sampleId) =>
        sampleId is int sid && _samples.GetById(sid)?.Status == SampleStatus.Approved;

    private void Run(Action action)
    {
        try { action(); }
        catch (NpgsqlException ex) { _view.ShowErrors(new[] { "Database error: " + ex.Message }); }
        catch (InvalidOperationException ex) { _view.ShowErrors(new[] { ex.Message }); }
    }
}

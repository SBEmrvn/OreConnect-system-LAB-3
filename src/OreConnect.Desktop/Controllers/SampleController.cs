using Npgsql;
using OreConnect.Desktop.Models;
using OreConnect.Desktop.Models.Data;
using OreConnect.Desktop.Views;

namespace OreConnect.Desktop.Controllers;

/// <summary>Handles the Save / Update / Delete / Display buttons of the Samples screen.</summary>
public class SampleController
{
    private readonly ISampleView _view;
    private readonly ISampleRepository _repo;

    /// <summary>Raised after any change, so other screens (Listings) can refresh their sample list.</summary>
    public event EventHandler? SamplesChanged;

    public SampleController(ISampleView view, ISampleRepository repo)
    {
        _view = view;
        _repo = repo;
        _view.SaveClicked += (_, _) => Save();
        _view.UpdateClicked += (_, _) => Update();
        _view.DeleteClicked += (_, _) => Delete();
        _view.DisplayClicked += (_, _) => Display();
        _view.ClearClicked += (_, _) => _view.ClearFields();
        _view.RowSelected += (_, id) => Select(id);
    }

    /// <summary>Builds a Sample from the form. Returns null and shows errors if the input is invalid.</summary>
    private Sample? ReadForm()
    {
        var errors = new List<string>();
        if (!InputParser.TryDecimal(_view.WeightText, out var weight))
            errors.Add("Weight must be a number, for example 2.5.");

        var sample = new Sample
        {
            Id = _view.SelectedId ?? 0,
            ClientName = _view.ClientName,
            SourceLocation = _view.SourceLocation,
            DateReceived = _view.DateReceived.Date,
            WeightKg = weight,
            SampleType = _view.SampleType,
            Status = _view.Status,
        };
        if (errors.Count == 0) errors.AddRange(sample.Validate());
        if (errors.Count > 0)
        {
            _view.ShowErrors(errors);
            return null;
        }
        return sample;
    }

    public void Save()
    {
        var sample = ReadForm();
        if (sample is null) return;
        Run(() =>
        {
            var saved = _repo.Add(sample);
            _view.ShowInfo($"Sample saved with ID {saved.SampleCode}.");
            _view.ClearFields();
            Display();
            SamplesChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    public void Update()
    {
        if (_view.SelectedId is null)
        {
            _view.ShowErrors(new[] { "Select a sample in the table first, then change it and click Update." });
            return;
        }
        var sample = ReadForm();
        if (sample is null) return;
        Run(() =>
        {
            if (!_repo.Update(sample))
            {
                _view.ShowErrors(new[] { "That sample no longer exists. Click Display to refresh." });
                return;
            }
            _view.ShowInfo("Sample updated.");
            Display();
            SamplesChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    public void Delete()
    {
        if (_view.SelectedId is not int id)
        {
            _view.ShowErrors(new[] { "Select a sample in the table first." });
            return;
        }
        Run(() =>
        {
            if (_repo.HasListings(id))
            {
                _view.ShowErrors(new[] { "This sample is linked to a marketplace listing. Delete or relink the listing first." });
                return;
            }
            if (!_view.Confirm("Delete the selected sample? This cannot be undone.")) return;
            _repo.Delete(id);
            _view.ShowInfo("Sample deleted.");
            _view.ClearFields();
            Display();
            SamplesChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    public void Display() => Run(() => _view.ShowSamples(_repo.GetAll(_view.SearchText)));

    private void Select(int id) => Run(() =>
    {
        var s = _repo.GetById(id);
        if (s is not null) _view.FillFields(s);
    });

    /// <summary>Turns database failures into a readable message instead of a crash.</summary>
    private void Run(Action action)
    {
        try { action(); }
        catch (NpgsqlException ex) { _view.ShowErrors(new[] { "Database error: " + ex.Message }); }
        catch (InvalidOperationException ex) { _view.ShowErrors(new[] { ex.Message }); }
    }
}

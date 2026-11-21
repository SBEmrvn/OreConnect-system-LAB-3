using OreConnect.Desktop.Models;

namespace OreConnect.Desktop.Views;

/// <summary>
/// What the Samples screen exposes to its controller. The view only shows data and raises events;
/// it never talks to the database. This keeps the controller testable with a fake view.
/// </summary>
public interface ISampleView
{
    // Field values typed by the user (raw text, so the controller can validate them).
    int? SelectedId { get; }
    string ClientName { get; }
    string SourceLocation { get; }
    DateTime DateReceived { get; }
    string WeightText { get; }
    string SampleType { get; }
    SampleStatus Status { get; }
    string SearchText { get; }

    event EventHandler SaveClicked;
    event EventHandler UpdateClicked;
    event EventHandler DeleteClicked;
    event EventHandler DisplayClicked;
    event EventHandler ClearClicked;
    event EventHandler<int> RowSelected;

    void ShowSamples(IReadOnlyList<Sample> samples);
    void FillFields(Sample sample);
    void ClearFields();
    void ShowInfo(string message);
    void ShowErrors(IEnumerable<string> errors);
    bool Confirm(string message);
}

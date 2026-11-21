using OreConnect.Desktop.Models;

namespace OreConnect.Desktop.Views;

public interface IListingView
{
    int? SelectedId { get; }
    int? LinkedSampleId { get; }
    string MineralType { get; }
    string QuantityText { get; }
    string GradeText { get; }
    string ListingLocation { get; }
    string PriceText { get; }
    string SearchText { get; }

    event EventHandler SaveClicked;
    event EventHandler UpdateClicked;
    event EventHandler DeleteClicked;
    event EventHandler DisplayClicked;
    event EventHandler ClearClicked;
    event EventHandler PublishClicked;
    event EventHandler<int> RowSelected;

    void ShowListings(IReadOnlyList<Listing> listings);
    void ShowSampleChoices(IReadOnlyList<Sample> samples);
    void FillFields(Listing listing);
    void ClearFields();
    void ShowInfo(string message);
    void ShowErrors(IEnumerable<string> errors);
    bool Confirm(string message);
}

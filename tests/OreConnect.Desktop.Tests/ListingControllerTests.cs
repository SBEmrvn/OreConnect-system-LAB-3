using OreConnect.Desktop.Controllers;
using OreConnect.Desktop.Models;

namespace OreConnect.Desktop.Tests;

public class ListingControllerTests
{
    private readonly FakeListingView _view = new();
    private readonly FakeListingRepository _listings = new();
    private readonly FakeSampleRepository _samples = new();

    public ListingControllerTests() => _ = new ListingController(_view, _listings, _samples);

    private int AddSample(SampleStatus status) =>
        _samples.Add(new Sample { ClientName = "Kivu", SourceLocation = "Rutsiro", WeightKg = 1, Status = status }).Id;

    [Fact]
    public void Save_creates_draft_listing()
    {
        _view.ClickSave();
        Assert.Equal(ListingStatus.Draft, _listings.Items.Single().Status);
    }

    [Theory]
    [InlineData("abc", "71.5", "18500", "Quantity must be a number")]
    [InlineData("20", "150", "18500", "Grade must be between 0 and 100")]
    [InlineData("20", "71.5", "0", "Price per tonne must be greater than 0")]
    public void Save_invalid_listing_is_rejected(string qty, string grade, string price, string expected)
    {
        _view.QuantityText = qty;
        _view.GradeText = grade;
        _view.PriceText = price;
        _view.ClickSave();
        Assert.Empty(_listings.Items);
        Assert.Contains(_view.Errors, e => e.Contains(expected));
    }

    [Fact]
    public void Publish_requires_approved_sample()
    {
        _view.LinkedSampleId = AddSample(SampleStatus.Analysed);
        _view.ClickSave();
        _view.ClickDisplay();
        _view.SelectedId = _listings.Items[0].Id;

        _view.ClickPublish();

        Assert.Equal(ListingStatus.Draft, _listings.Items[0].Status);
        Assert.Contains(_view.Errors, e => e.Contains("Approved sample"));
    }

    [Fact]
    public void Publish_without_linked_sample_is_rejected()
    {
        _view.ClickSave();
        _view.ClickDisplay();
        _view.SelectedId = _listings.Items[0].Id;

        _view.ClickPublish();

        Assert.Equal(ListingStatus.Draft, _listings.Items[0].Status);
    }

    [Fact]
    public void Publish_succeeds_with_approved_sample()
    {
        _view.LinkedSampleId = AddSample(SampleStatus.Approved);
        _view.ClickSave();
        _view.ClickDisplay();
        _view.SelectedId = _listings.Items[0].Id;

        _view.ClickPublish();

        Assert.Equal(ListingStatus.Published, _listings.Items[0].Status);
        Assert.Equal("Listing published.", _view.Info);
    }
}

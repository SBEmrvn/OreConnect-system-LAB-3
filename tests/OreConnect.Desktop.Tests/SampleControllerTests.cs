using OreConnect.Desktop.Controllers;
using OreConnect.Desktop.Models;

namespace OreConnect.Desktop.Tests;

public class SampleControllerTests
{
    private readonly FakeSampleView _view = new();
    private readonly FakeSampleRepository _repo = new();

    public SampleControllerTests() => _ = new SampleController(_view, _repo);

    [Fact]
    public void Save_valid_sample_stores_it_and_assigns_unique_id()
    {
        _view.ClickSave();
        _view.ClickSave();

        Assert.Empty(_view.Errors);
        Assert.Equal(2, _repo.Items.Count);
        Assert.NotEqual(_repo.Items[0].SampleCode, _repo.Items[1].SampleCode);
        Assert.StartsWith("OC-", _repo.Items[0].SampleCode);
        Assert.Contains("saved", _view.Info);
        Assert.True(_view.Cleared);
        Assert.Equal(2, _view.Shown.Count); // table refreshed after save
    }

    [Theory]
    [InlineData("", "Rutsiro", "2.5", "Client name")]
    [InlineData("Kivu", "", "2.5", "Source location")]
    [InlineData("Kivu", "Rutsiro", "heavy", "Weight must be a number")]
    [InlineData("Kivu", "Rutsiro", "0", "Weight must be greater than 0")]
    [InlineData("Kivu", "Rutsiro", "-4", "Weight must be greater than 0")]
    public void Save_invalid_input_shows_error_and_saves_nothing(string client, string location, string weight, string expected)
    {
        _view.ClientName = client;
        _view.SourceLocation = location;
        _view.WeightText = weight;

        _view.ClickSave();

        Assert.Empty(_repo.Items);
        Assert.Contains(_view.Errors, e => e.Contains(expected));
    }

    [Fact]
    public void Save_rejects_future_date()
    {
        _view.DateReceived = DateTime.Today.AddDays(1);
        _view.ClickSave();
        Assert.Empty(_repo.Items);
        Assert.Contains(_view.Errors, e => e.Contains("future"));
    }

    [Fact]
    public void Weight_accepts_comma_decimal_separator()
    {
        _view.WeightText = "2,75";
        _view.ClickSave();
        Assert.Equal(2.75m, _repo.Items.Single().WeightKg);
    }

    [Fact]
    public void Selecting_a_row_fills_the_form()
    {
        _view.ClickSave();
        var id = _repo.Items[0].Id;

        _view.SelectRow(id);

        Assert.Equal(id, _view.Filled?.Id);
    }

    [Fact]
    public void Update_changes_selected_sample()
    {
        _view.ClickSave();
        _view.SelectRow(_repo.Items[0].Id);
        _view.ClientName = "Rubavu Mining";
        _view.Status = SampleStatus.Approved;

        _view.ClickUpdate();

        Assert.Equal("Rubavu Mining", _repo.Items[0].ClientName);
        Assert.Equal(SampleStatus.Approved, _repo.Items[0].Status);
        Assert.Equal("Sample updated.", _view.Info);
    }

    [Fact]
    public void Update_without_selection_shows_error()
    {
        _view.ClickUpdate();
        Assert.Contains(_view.Errors, e => e.Contains("Select a sample"));
    }

    [Fact]
    public void Delete_removes_sample_after_confirmation()
    {
        _view.ClickSave();
        _view.SelectRow(_repo.Items[0].Id);

        _view.ClickDelete();

        Assert.Empty(_repo.Items);
    }

    [Fact]
    public void Delete_does_nothing_when_user_says_no()
    {
        _view.ClickSave();
        _view.SelectRow(_repo.Items[0].Id);
        _view.ConfirmAnswer = false;

        _view.ClickDelete();

        Assert.Single(_repo.Items);
    }

    [Fact]
    public void Delete_blocked_when_sample_has_listing()
    {
        _view.ClickSave();
        var id = _repo.Items[0].Id;
        _repo.WithListings.Add(id);
        _view.SelectRow(id);

        _view.ClickDelete();

        Assert.Single(_repo.Items);
        Assert.Contains(_view.Errors, e => e.Contains("linked to a marketplace listing"));
    }

    [Fact]
    public void Display_filters_by_search_text()
    {
        _view.ClickSave();
        _view.ClientName = "Other Client";
        _view.ClickSave();

        _view.SearchText = "kivu";
        _view.ClickDisplay();

        Assert.Single(_view.Shown);
        Assert.Equal("Kivu Mining Ltd", _view.Shown[0].ClientName);
    }
}

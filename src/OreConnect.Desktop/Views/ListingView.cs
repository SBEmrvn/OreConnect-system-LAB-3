using OreConnect.Desktop.Models;

namespace OreConnect.Desktop.Views;

/// <summary>Marketplace listings screen, same layout as the Samples screen.</summary>
public class ListingView : UserControl, IListingView
{
    private readonly TextBox _id = UiKit.Input(readOnly: true);
    private readonly ComboBox _sample = UiKit.Dropdown(Array.Empty<object>());
    private readonly TextBox _mineral = UiKit.Input();
    private readonly TextBox _quantity = UiKit.Input();
    private readonly TextBox _grade = UiKit.Input();
    private readonly TextBox _location = UiKit.Input();
    private readonly TextBox _price = UiKit.Input();
    private readonly TextBox _search = UiKit.Input();
    private readonly DataGridView _grid = UiKit.Grid();
    private readonly Label _message = new() { AutoSize = true, Font = UiKit.Body, ForeColor = UiKit.Success, Margin = new Padding(0, 10, 0, 0) };
    private int? _selectedId;

    public event EventHandler? SaveClicked;
    public event EventHandler? UpdateClicked;
    public event EventHandler? DeleteClicked;
    public event EventHandler? DisplayClicked;
    public event EventHandler? ClearClicked;
    public event EventHandler? PublishClicked;
    public event EventHandler<int>? RowSelected;

    public ListingView()
    {
        Dock = DockStyle.Fill;
        BackColor = UiKit.Background;
        Padding = new Padding(16);
        _quantity.PlaceholderText = "tonnes";
        _grade.PlaceholderText = "0 – 100";
        _price.PlaceholderText = "USD per tonne";
        _search.PlaceholderText = "Search by mineral, location or sample ID";

        var tips = new ToolTip();
        tips.SetToolTip(_sample, "The analysed sample behind this offer. It must be Approved before you can publish.");

        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true, Padding = new Padding(0, 0, 16, 0) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(fields, "Listing ID", _id);
        AddRow(fields, "Linked sample", _sample);
        AddRow(fields, "Mineral type", _mineral);
        AddRow(fields, "Quantity (t)", _quantity);
        AddRow(fields, "Grade (%)", _grade);
        AddRow(fields, "Location", _location);
        AddRow(fields, "Price / tonne", _price);
        SampleView.AddFiller(fields);

        _grid.Columns.AddRange(
            UiKit.Column("ID", nameof(ListingRow.Id), 40),
            UiKit.Column("Mineral", nameof(ListingRow.MineralType), 120),
            UiKit.Column("Qty (t)", nameof(ListingRow.QuantityTonnes), 70, "0.###"),
            UiKit.Column("Grade %", nameof(ListingRow.GradePercent), 70, "0.##"),
            UiKit.Column("Location", nameof(ListingRow.Location), 120),
            UiKit.Column("Price / t", nameof(ListingRow.PricePerTonne), 90, "$#,0.00"),
            UiKit.Column("Sample", nameof(ListingRow.SampleCode), 110),
            UiKit.Column("Status", nameof(ListingRow.Status), 80));
        _grid.CellClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is ListingRow row)
                RowSelected?.Invoke(this, row.Id);
        };

        var searchBtn = UiKit.Action("Search");
        searchBtn.Margin = new Padding(8, 3, 0, 0);
        searchBtn.Click += (_, _) => DisplayClicked?.Invoke(this, EventArgs.Empty);
        _search.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; DisplayClicked?.Invoke(this, EventArgs.Empty); } };
        var searchRow = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, Height = 44 };
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        searchRow.Controls.Add(_search, 0, 0);
        searchRow.Controls.Add(searchBtn, 1, 0);

        var right = new Panel { Dock = DockStyle.Fill };
        right.Controls.Add(_grid);
        right.Controls.Add(searchRow);

        var save = UiKit.Action("Save", primary: true);
        var delete = UiKit.Action("Delete", danger: true);
        var update = UiKit.Action("Update");
        var display = UiKit.Action("Display");
        var publish = UiKit.Action("Publish");
        var clear = UiKit.Action("Clear");
        save.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);
        delete.Click += (_, _) => DeleteClicked?.Invoke(this, EventArgs.Empty);
        update.Click += (_, _) => UpdateClicked?.Invoke(this, EventArgs.Empty);
        display.Click += (_, _) => DisplayClicked?.Invoke(this, EventArgs.Empty);
        publish.Click += (_, _) => PublishClicked?.Invoke(this, EventArgs.Empty);
        clear.Click += (_, _) => ClearClicked?.Invoke(this, EventArgs.Empty);
        tips.SetToolTip(publish, "Make the selected listing visible to buyers. Needs an Approved sample.");

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 12, 0, 0) };
        buttons.Controls.AddRange(new Control[] { save, delete, update, display, publish, clear });

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(fields, 0, 0);
        root.Controls.Add(right, 1, 0);
        root.Controls.Add(buttons, 0, 1);
        root.SetColumnSpan(buttons, 2);
        root.Controls.Add(_message, 0, 2);
        root.SetColumnSpan(_message, 2);
        Controls.Add(root);
    }

    private static void AddRow(TableLayoutPanel t, string label, Control input)
    {
        var row = t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.Controls.Add(UiKit.FieldLabel(label), 0, row);
        t.Controls.Add(input, 1, row);
    }

    // ---- IListingView ----
    public int? SelectedId => _selectedId;
    public int? LinkedSampleId => (_sample.SelectedItem as SampleChoice)?.Id;
    public string MineralType => _mineral.Text;
    public string QuantityText => _quantity.Text;
    public string GradeText => _grade.Text;
    public string ListingLocation => _location.Text;
    public string PriceText => _price.Text;
    public string SearchText => _search.Text;

    public void ShowListings(IReadOnlyList<Listing> listings)
    {
        _grid.DataSource = listings.Select(l => new ListingRow(l)).ToList();
        _grid.ClearSelection();
        _message.ForeColor = UiKit.Muted;
        _message.Text = $"{listings.Count} listing(s) shown.";
    }

    public void ShowSampleChoices(IReadOnlyList<Sample> samples)
    {
        var keep = LinkedSampleId;
        _sample.Items.Clear();
        _sample.Items.Add(new SampleChoice(null, "(none)"));
        foreach (var s in samples)
            _sample.Items.Add(new SampleChoice(s.Id, $"{s.SampleCode} · {Sample.StatusText(s.Status)} · {s.ClientName}"));
        SelectSample(keep);
    }

    public void FillFields(Listing l)
    {
        _selectedId = l.Id;
        _id.Text = $"{l.Id} ({l.Status})";
        SelectSample(l.SampleId);
        _mineral.Text = l.MineralType;
        _quantity.Text = l.QuantityTonnes.ToString("0.###");
        _grade.Text = l.GradePercent.ToString("0.##");
        _location.Text = l.Location;
        _price.Text = l.PricePerTonne.ToString("0.00");
    }

    public void ClearFields()
    {
        _selectedId = null;
        _id.Clear(); _mineral.Clear(); _quantity.Clear(); _grade.Clear(); _location.Clear(); _price.Clear();
        SelectSample(null);
        _grid.ClearSelection();
        _mineral.Focus();
    }

    public void ShowInfo(string message)
    {
        _message.ForeColor = UiKit.Success;
        _message.Text = message;
    }

    public void ShowErrors(IEnumerable<string> errors) => UiKit.ShowErrors(this, errors);
    public bool Confirm(string message) => UiKit.Confirm(this, message);

    private void SelectSample(int? id)
    {
        var match = _sample.Items.OfType<SampleChoice>().FirstOrDefault(c => c.Id == id);
        _sample.SelectedItem = match ?? _sample.Items.OfType<SampleChoice>().FirstOrDefault();
    }

    private sealed record SampleChoice(int? Id, string Text)
    {
        public override string ToString() => Text;
    }

    private sealed class ListingRow(Listing l)
    {
        public int Id { get; } = l.Id;
        public string MineralType { get; } = l.MineralType;
        public decimal QuantityTonnes { get; } = l.QuantityTonnes;
        public decimal GradePercent { get; } = l.GradePercent;
        public string Location { get; } = l.Location;
        public decimal PricePerTonne { get; } = l.PricePerTonne;
        public string SampleCode { get; } = l.SampleCode ?? "-";
        public string Status { get; } = l.Status.ToString();
    }
}

using OreConnect.Desktop.Models;

namespace OreConnect.Desktop.Views;

/// <summary>
/// Samples screen. Layout follows the lab sketch: input fields on the left, a table on the right,
/// and Save / Delete / Update / Display buttons underneath.
/// </summary>
public class SampleView : UserControl, ISampleView
{
    private readonly TextBox _id = UiKit.Input(readOnly: true);
    private readonly TextBox _client = UiKit.Input();
    private readonly TextBox _location = UiKit.Input();
    private readonly DateTimePicker _date = new() { Font = UiKit.Body, Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Margin = new Padding(0, 4, 0, 4) };
    private readonly TextBox _weight = UiKit.Input();
    private readonly ComboBox _type = UiKit.Dropdown(Sample.SampleTypes);
    private readonly ComboBox _status = UiKit.Dropdown(Enum.GetValues<SampleStatus>().Select(s => (object)new StatusItem(s)));
    private readonly TextBox _search = UiKit.Input();
    private readonly DataGridView _grid = UiKit.Grid();
    private readonly Label _message = new() { AutoSize = true, Font = UiKit.Body, ForeColor = UiKit.Success, Margin = new Padding(0, 10, 0, 0) };
    private int? _selectedId;

    public event EventHandler? SaveClicked;
    public event EventHandler? UpdateClicked;
    public event EventHandler? DeleteClicked;
    public event EventHandler? DisplayClicked;
    public event EventHandler? ClearClicked;
    public event EventHandler<int>? RowSelected;

    public SampleView()
    {
        Dock = DockStyle.Fill;
        BackColor = UiKit.Background;
        Padding = new Padding(16);
        _date.MaxDate = DateTime.Today;
        _weight.PlaceholderText = "e.g. 2.5";
        _search.PlaceholderText = "Search by sample ID, client or location";

        var tips = new ToolTip();
        tips.SetToolTip(_id, "Assigned automatically when you click Save.");
        tips.SetToolTip(_weight, "Sample weight in kilograms.");

        // ---- left: fields ----
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true, Padding = new Padding(0, 0, 16, 0) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(fields, "Sample ID", _id);
        AddRow(fields, "Client name", _client);
        AddRow(fields, "Source location", _location);
        AddRow(fields, "Date received", _date);
        AddRow(fields, "Weight (kg)", _weight);
        AddRow(fields, "Sample type", _type);
        AddRow(fields, "Status", _status);
        AddFiller(fields);

        // ---- right: search + grid ----
        _grid.Columns.AddRange(
            UiKit.Column("Sample ID", nameof(SampleRow.SampleCode), 110),
            UiKit.Column("Client", nameof(SampleRow.ClientName), 130),
            UiKit.Column("Location", nameof(SampleRow.SourceLocation), 130),
            UiKit.Column("Received", nameof(SampleRow.DateReceived), 80),
            UiKit.Column("Weight (kg)", nameof(SampleRow.WeightKg), 70, "0.###"),
            UiKit.Column("Type", nameof(SampleRow.SampleType), 90),
            UiKit.Column("Status", nameof(SampleRow.Status), 80));
        _grid.CellClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is SampleRow row)
                RowSelected?.Invoke(this, row.Id);
        };

        var searchBtn = UiKit.Action("Search");
        searchBtn.Click += (_, _) => DisplayClicked?.Invoke(this, EventArgs.Empty);
        _search.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; DisplayClicked?.Invoke(this, EventArgs.Empty); } };
        var searchRow = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, Height = 44 };
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        searchRow.Controls.Add(_search, 0, 0);
        searchRow.Controls.Add(searchBtn, 1, 0);
        searchBtn.Margin = new Padding(8, 3, 0, 0);

        var right = new Panel { Dock = DockStyle.Fill };
        right.Controls.Add(_grid);
        right.Controls.Add(searchRow);

        // ---- bottom: buttons (Save, Delete, Update, Display as on the sketch) ----
        var save = UiKit.Action("Save", primary: true);
        var delete = UiKit.Action("Delete", danger: true);
        var update = UiKit.Action("Update");
        var display = UiKit.Action("Display");
        var clear = UiKit.Action("Clear");
        save.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);
        delete.Click += (_, _) => DeleteClicked?.Invoke(this, EventArgs.Empty);
        update.Click += (_, _) => UpdateClicked?.Invoke(this, EventArgs.Empty);
        display.Click += (_, _) => DisplayClicked?.Invoke(this, EventArgs.Empty);
        clear.Click += (_, _) => ClearClicked?.Invoke(this, EventArgs.Empty);
        tips.SetToolTip(save, "Add the form as a new sample.");
        tips.SetToolTip(update, "Save changes to the sample selected in the table.");
        tips.SetToolTip(delete, "Delete the sample selected in the table.");
        tips.SetToolTip(display, "Reload the table from the database.");

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 12, 0, 0) };
        buttons.Controls.AddRange(new Control[] { save, delete, update, display, clear });

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

    /// <summary>Takes up the leftover height so the field rows stay together at the top.</summary>
    internal static void AddFiller(TableLayoutPanel t)
    {
        t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
    }

    // ---- ISampleView ----
    public int? SelectedId => _selectedId;
    public string ClientName => _client.Text;
    public string SourceLocation => _location.Text;
    public DateTime DateReceived => _date.Value;
    public string WeightText => _weight.Text;
    public string SampleType => _type.SelectedItem?.ToString() ?? "";
    public SampleStatus Status => (_status.SelectedItem as StatusItem)?.Value ?? SampleStatus.Received;
    public string SearchText => _search.Text;

    public void ShowSamples(IReadOnlyList<Sample> samples)
    {
        _grid.DataSource = samples.Select(s => new SampleRow(s)).ToList();
        _grid.ClearSelection();
        _message.ForeColor = UiKit.Muted;
        _message.Text = $"{samples.Count} sample(s) shown.";
    }

    public void FillFields(Sample s)
    {
        _selectedId = s.Id;
        _id.Text = s.SampleCode;
        _client.Text = s.ClientName;
        _location.Text = s.SourceLocation;
        _date.Value = s.DateReceived > _date.MaxDate ? _date.MaxDate : s.DateReceived;
        _weight.Text = s.WeightKg.ToString("0.###");
        _type.SelectedItem = Sample.SampleTypes.Contains(s.SampleType) ? s.SampleType : Sample.SampleTypes[^1];
        _status.SelectedIndex = (int)s.Status;
    }

    public void ClearFields()
    {
        _selectedId = null;
        _id.Clear(); _client.Clear(); _location.Clear(); _weight.Clear();
        _date.Value = DateTime.Today;
        _type.SelectedIndex = 0;
        _status.SelectedIndex = 0;
        _grid.ClearSelection();
        _client.Focus();
    }

    public void ShowInfo(string message)
    {
        _message.ForeColor = UiKit.Success;
        _message.Text = message;
    }

    public void ShowErrors(IEnumerable<string> errors) => UiKit.ShowErrors(this, errors);
    public bool Confirm(string message) => UiKit.Confirm(this, message);

    private sealed record StatusItem(SampleStatus Value)
    {
        public override string ToString() => Sample.StatusText(Value);
    }

    /// <summary>Flat row for the grid (formats the date and status for display).</summary>
    private sealed class SampleRow(Sample s)
    {
        public int Id { get; } = s.Id;
        public string SampleCode { get; } = s.SampleCode;
        public string ClientName { get; } = s.ClientName;
        public string SourceLocation { get; } = s.SourceLocation;
        public string DateReceived { get; } = s.DateReceived.ToString("yyyy-MM-dd");
        public decimal WeightKg { get; } = s.WeightKg;
        public string SampleType { get; } = s.SampleType;
        public string Status { get; } = Sample.StatusText(s.Status);
    }
}

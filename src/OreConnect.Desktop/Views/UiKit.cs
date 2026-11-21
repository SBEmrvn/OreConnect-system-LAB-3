namespace OreConnect.Desktop.Views;

/// <summary>Shared colours and control factories so every screen looks the same.</summary>
internal static class UiKit
{
    public static readonly Color Brand = Color.FromArgb(138, 90, 31);      // copper / ore
    public static readonly Color BrandDark = Color.FromArgb(104, 66, 20);
    public static readonly Color Surface = Color.White;
    public static readonly Color Background = Color.FromArgb(245, 244, 240);
    public static readonly Color Muted = Color.FromArgb(100, 106, 102);
    public static readonly Color Success = Color.FromArgb(31, 122, 74);
    public static readonly Color Danger = Color.FromArgb(179, 38, 30);

    public static readonly Font Body = new("Segoe UI", 10f);
    public static readonly Font Bold = new("Segoe UI Semibold", 10f);
    public static readonly Font Title = new("Segoe UI Semibold", 14f);

    public static Label FieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = Bold,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 8, 8, 4),
    };

    public static TextBox Input(bool readOnly = false) => new()
    {
        Font = Body,
        Dock = DockStyle.Fill,
        ReadOnly = readOnly,
        Margin = new Padding(0, 4, 0, 4),
        BackColor = readOnly ? Background : Surface,
    };

    public static ComboBox Dropdown(IEnumerable<object> items)
    {
        var cb = new ComboBox
        {
            Font = Body,
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 4, 0, 4),
        };
        cb.Items.AddRange(items.ToArray());
        if (cb.Items.Count > 0) cb.SelectedIndex = 0;
        return cb;
    }

    public static Button Action(string text, bool primary = false, bool danger = false)
    {
        var b = new Button
        {
            Text = text,
            Font = Bold,
            AutoSize = false,
            Size = new Size(104, 36),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 10, 0),
            BackColor = primary ? Brand : danger ? Danger : Surface,
            ForeColor = primary || danger ? Color.White : Color.Black,
        };
        b.FlatAppearance.BorderColor = primary ? BrandDark : danger ? Danger : Color.FromArgb(200, 200, 192);
        return b;
    }

    public static DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            BackgroundColor = Surface,
            BorderStyle = BorderStyle.FixedSingle,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            Font = Body,
            ColumnHeadersHeight = 34,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
        };
        g.ColumnHeadersDefaultCellStyle.BackColor = Background;
        g.ColumnHeadersDefaultCellStyle.Font = Bold;
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(243, 233, 220);
        g.DefaultCellStyle.SelectionForeColor = Color.Black;
        g.RowTemplate.Height = 30;
        return g;
    }

    public static DataGridViewTextBoxColumn Column(string header, string property, int weight = 100, string? format = null)
    {
        var c = new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            DataPropertyName = property,
            FillWeight = weight,
            SortMode = DataGridViewColumnSortMode.NotSortable,
        };
        if (format is not null)
        {
            c.DefaultCellStyle.Format = format;
            c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        return c;
    }

    public static void ShowErrors(IWin32Window owner, IEnumerable<string> errors) =>
        MessageBox.Show(owner, "Please fix the following:\n\n• " + string.Join("\n• ", errors),
            "OreConnect", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static bool Confirm(IWin32Window owner, string message) =>
        MessageBox.Show(owner, message, "OreConnect", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;
}

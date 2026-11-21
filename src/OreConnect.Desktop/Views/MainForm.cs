namespace OreConnect.Desktop.Views;

/// <summary>Main window: a header and one tab per screen.</summary>
public class MainForm : Form
{
    public SampleView SampleView { get; } = new();
    public ListingView ListingView { get; } = new();
    private readonly ToolStripStatusLabel _status = new();
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill, Font = UiKit.Bold, Padding = new Point(16, 6) };

    /// <summary>Raised when the user switches to a tab, so its controller can refresh.</summary>
    public event EventHandler<int>? TabChanged;

    public MainForm()
    {
        SuspendLayout();
        // Sizes below are written for 96 DPI; WinForms scales them up on high-DPI screens.
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = "OreConnect – Laboratory & Marketplace";
        MinimumSize = new Size(1000, 600);
        Size = new Size(1200, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiKit.Background;
        Font = UiKit.Body;

        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = UiKit.Brand,
            Padding = new Padding(20, 10, 20, 10),
        };
        var title = new Label
        {
            Text = "◆  OreConnect",
            Font = new Font("Segoe UI Semibold", 18f),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0),
        };
        var subtitle = new Label
        {
            Text = "Mineral sample registration and verified listings",
            Font = UiKit.Body,
            ForeColor = Color.FromArgb(243, 233, 220),
            AutoSize = true,
            Margin = new Padding(4, 0, 0, 0),
        };
        header.Controls.Add(title);
        header.Controls.Add(subtitle);

        var samplesTab = new TabPage("Samples") { BackColor = UiKit.Background };
        samplesTab.Controls.Add(SampleView);
        var listingsTab = new TabPage("Marketplace listings") { BackColor = UiKit.Background };
        listingsTab.Controls.Add(ListingView);
        _tabs.TabPages.Add(samplesTab);
        _tabs.TabPages.Add(listingsTab);
        _tabs.SelectedIndexChanged += (_, _) => TabChanged?.Invoke(this, _tabs.SelectedIndex);

        var statusBar = new StatusStrip { BackColor = Color.White };
        statusBar.Items.Add(_status);

        Controls.Add(_tabs);
        Controls.Add(header);
        Controls.Add(statusBar);
        ResumeLayout(true);
    }

    public void SetStatus(string text) => _status.Text = text;
}

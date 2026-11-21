using Npgsql;
using OreConnect.Desktop.Controllers;
using OreConnect.Desktop.Models.Data;
using OreConnect.Desktop.Views;

namespace OreConnect.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        Database db;
        try
        {
            db = Database.FromConfiguration();
            db.EnsureSchema();
        }
        catch (Exception ex) when (ex is InvalidOperationException or NpgsqlException or System.Net.Sockets.SocketException)
        {
            MessageBox.Show(
                "Could not connect to the database.\n\n" + ex.Message +
                "\n\nCheck that PostgreSQL is running and that appsettings.json has the right connection string.",
                "OreConnect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Wire Models, Views and Controllers together (MVC).
        var sampleRepo = new SampleRepository(db);
        var listingRepo = new ListingRepository(db);
        var form = new MainForm();
        var samples = new SampleController(form.SampleView, sampleRepo);
        var listings = new ListingController(form.ListingView, listingRepo, sampleRepo);

        samples.SamplesChanged += (_, _) => listings.RefreshSampleChoices();
        form.TabChanged += (_, tab) =>
        {
            if (tab == 0) samples.Display();
            else { listings.RefreshSampleChoices(); listings.Display(); }
        };
        form.Shown += (_, _) =>
        {
            samples.Display();
            listings.RefreshSampleChoices();
            listings.Display();
        };
        form.SetStatus("Connected to PostgreSQL");

        Application.Run(form);
    }
}

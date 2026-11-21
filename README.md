# OreConnect Desktop (Lab 3)

Windows Forms desktop version of my OreConnect project. Users manage mineral **samples** and
marketplace **listings** stored in a **PostgreSQL** database, through a GUI built with the
**MVC** pattern.

## Screens
- **Samples**: Sample ID (generated), client name, source location, date received, weight, sample type and status.
  Buttons: **Save** (insert), **Delete**, **Update**, **Display** (reload / search), **Clear**.
  Click a row in the table to load it into the form.
- **Marketplace listings**: mineral type, quantity, grade, location, price and linked sample.
  **Publish** works only when the linked sample is *Approved*.

## MVC structure
```
src/OreConnect.Desktop/
├── Models/              data + rules
│   ├── Sample.cs, Listing.cs        entities with Validate()
│   └── Data/                        PostgreSQL access (Npgsql, parameterised SQL)
│       ├── Database.cs              connection + creates tables on first run
│       ├── SampleRepository.cs
│       └── ListingRepository.cs
├── Views/               WinForms UI only, no SQL
│   ├── ISampleView.cs, IListingView.cs   what a view exposes to its controller
│   ├── SampleView.cs, ListingView.cs     the screens
│   ├── MainForm.cs                       window with tabs
│   └── UiKit.cs                          shared colours and controls
├── Controllers/         button logic: read view → validate → call model → update view
│   ├── SampleController.cs
│   └── ListingController.cs
└── Program.cs           wires Models, Views and Controllers together
tests/OreConnect.Desktop.Tests/   xUnit: controller tests with fake views + repository tests
```
Flow: the user clicks **Save** → the view raises `SaveClicked` → `SampleController.Save()` reads
the fields, validates them with `Sample.Validate()`, calls `SampleRepository.Add()`, then tells the
view to refresh the table.

## Setup
1. Install **.NET 10 SDK** and **PostgreSQL**, and create a database:
   ```sql
   CREATE DATABASE oreconnect_desktop;
   ```
2. Copy `src/OreConnect.Desktop/appsettings.example.json` to `appsettings.json` in the same folder and
   put your PostgreSQL password in it. (`appsettings.json` is git-ignored.)
3. Run:
   ```
   dotnet run --project src/OreConnect.Desktop
   ```
   Tables are created automatically on first start.

Or open `OreConnect.Desktop.slnx` in Visual Studio / VS Code and run the `OreConnect.Desktop` project.

## Tests
```
dotnet test
```
Repository (database) tests run only when `OREC_TEST_DB` is set to a connection string for an empty
test database, e.g. `Host=localhost;Database=oreconnect_desktop_test;Username=postgres;Password=...`.

using TheFipster.ActivityAggregator.App.Models;
using TheFipster.ActivityAggregator.App.Services;

namespace TheFipster.ActivityAggregator.App;

public partial class MainPage : ContentPage
{
    private readonly StorageService _storage;
    private List<WeightEntry> _entries = new();

    public MainPage()
    {
        InitializeComponent();
        _storage = new StorageService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _entries = await _storage.LoadAsync();
        RefreshList();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (double.TryParse(WeightEntry.Text, out var weight))
        {
            var entry = new WeightEntry
            {
                Weight = weight,
                Reason = ReasonEntry.Text,
                Date = DateTime.Now,
            };

            _entries.Add(entry);
            await _storage.SaveAsync(_entries);

            WeightEntry.Text = string.Empty;
            ReasonEntry.Text = string.Empty;

            RefreshList();
        }
        else
        {
            await DisplayAlert("Error", "Please enter a valid weight", "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is WeightEntry entry)
        {
            bool confirm = await DisplayAlert(
                "Delete Entry",
                $"Delete {entry.Weight}kg from {entry.Date}?",
                "Yes",
                "No"
            );
            if (confirm)
            {
                _entries.Remove(entry);
                await _storage.SaveAsync(_entries);
                RefreshList();
            }
        }
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        var filePath = _storage.GetFilePath();
        await Share.RequestAsync(
            new ShareFileRequest { Title = "Share Weight Log", File = new ShareFile(filePath) }
        );
    }

    private void RefreshList()
    {
        EntriesList.ItemsSource = null;
        EntriesList.ItemsSource = _entries.OrderByDescending(e => e.Date).ToList();
    }
}

using ArtistManagement.Models;
using ArtistManagement.Services;
using System.Collections.ObjectModel;

namespace ArtistManagement;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    public ObservableCollection<Artist> Artists { get; set; } = new();

    private int _artistCount;
    public int ArtistCount
    {
        get => _artistCount;
        set
        {
            if (_artistCount != value)
            {
                _artistCount = value;
                OnPropertyChanged(nameof(ArtistCount));
            }
        }
    }

    public MainPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        BindingContext = this;
        Artists.CollectionChanged += (s, e) => ArtistCount = Artists.Count;
        _ = LoadArtists();
    }

    private async Task LoadArtists()
    {
        var artists = await _databaseService.GetArtistsAsync();
        Artists.Clear();
        foreach (var artist in artists)
        {
            Artists.Add(artist);
        }
    }

    private async void OnAddArtistClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) || string.IsNullOrWhiteSpace(GenreEntry.Text))
        {
            await DisplayAlert("Error", "Name and Genre are required", "OK");
            return;
        }

        if (!int.TryParse(AlbumsSoldEntry.Text, out int albumsSold))
        {
            await DisplayAlert("Error", "Please enter a valid number for Albums Sold", "OK");
            return;
        }

        var artist = new Artist
        {
            Name = NameEntry.Text,
            Genre = GenreEntry.Text,
            AlbumsSold = albumsSold,
            ContractEndDate = ContractEndDatePicker.Date
        };

        await _databaseService.SaveArtistAsync(artist);
        await LoadArtists();

        // Clear the form
        NameEntry.Text = string.Empty;
        GenreEntry.Text = string.Empty;
        AlbumsSoldEntry.Text = string.Empty;
        ContractEndDatePicker.Date = DateTime.Today;
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var artist = button?.CommandParameter as Artist;
        
        if (artist != null)
        {
            bool answer = await DisplayAlert("Confirm Delete", 
                $"Are you sure you want to delete {artist.Name}?", "Yes", "No");
                
            if (answer)
            {
                await _databaseService.DeleteArtistAsync(artist);
                await LoadArtists();
            }
        }
    }
}

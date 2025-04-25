using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ArtistManagement.Models;
using ArtistManagement.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ArtistManagement.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<Artist> artists;

    [ObservableProperty]
    private string artistName = string.Empty;

    [ObservableProperty]
    private string genre = string.Empty;

    [ObservableProperty]
    private string albumsSold = string.Empty;

    [ObservableProperty]
    private DateTime contractEndDate = DateTime.Today;

    [ObservableProperty]
    private int artistCount;

    public MainPageViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        artists = new ObservableCollection<Artist>();
        artists.CollectionChanged += OnArtistsCollectionChanged;
        _ = InitializeAsync();
    }

    private void OnArtistsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ArtistCount = Artists.Count;
    }

    private async Task InitializeAsync()
    {
        await LoadArtistsCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadArtists()
    {
        try 
        {
            var loadedArtists = await _databaseService.GetArtistsAsync();
            Artists.Clear();
            foreach (var artist in loadedArtists)
            {
                Artists.Add(artist);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "Failed to load artists: " + ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task AddArtist()
    {
        if (string.IsNullOrWhiteSpace(ArtistName) || string.IsNullOrWhiteSpace(Genre))
        {
            await Shell.Current.DisplayAlert("Error", "Name and Genre are required", "OK");
            return;
        }

        if (!int.TryParse(AlbumsSold, out int albumsSoldValue))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a valid number for Albums Sold", "OK");
            return;
        }

        var artist = new Artist
        {
            Name = ArtistName,
            Genre = Genre,
            AlbumsSold = albumsSoldValue,
            ContractEndDate = ContractEndDate
        };

        await _databaseService.SaveArtistAsync(artist);
        await LoadArtists();

        // Clear the form
        ArtistName = string.Empty;
        Genre = string.Empty;
        AlbumsSold = string.Empty;
        ContractEndDate = DateTime.Today;
    }

    [RelayCommand]
    private async Task DeleteArtist(Artist artist)
    {
        if (artist != null)
        {
            bool answer = await Shell.Current.DisplayAlert("Confirm Delete", 
                $"Are you sure you want to delete {artist.Name}?", "Yes", "No");
                
            if (answer)
            {
                await _databaseService.DeleteArtistAsync(artist);
                await LoadArtists();
            }
        }
    }
}
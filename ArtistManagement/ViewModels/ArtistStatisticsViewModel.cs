using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ArtistManagement.Models;
using ArtistManagement.Services;
using System.Collections.ObjectModel;

namespace ArtistManagement.ViewModels;

public partial class ArtistStatisticsViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<string> filterResults = new();

    private string _selectedFilter = "Total";
    public string SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (SetProperty(ref _selectedFilter, value))
            {
                _ = ApplyFilter();
            }
        }
    }

    [ObservableProperty]
    private string filterSummary = string.Empty;

    public List<string> AvailableFilters { get; } = new()
    {
        "Total",
        "Names",
        "Genres",
        "Albums Sold",
        "Contract End Dates"
    };

    public ArtistStatisticsViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task InitializeAsync()
    {
        await ApplyFilter();
    }

    private async Task ApplyFilter()
    {
        if (string.IsNullOrEmpty(SelectedFilter)) return;

        try
        {
            var artists = await _databaseService.GetArtistsAsync();
            var newResults = new ObservableCollection<string>();

            switch (SelectedFilter)
            {
                case "Total":
                    AddTotalCount(artists, newResults);
                    break;

                case "Names":
                    AddNamesFilter(artists, newResults);
                    break;

                case "Genres":
                    AddGenresFilter(artists, newResults);
                    break;

                case "Albums Sold":
                    AddAlbumsSoldFilter(artists, newResults);
                    break;

                case "Contract End Dates":
                    AddContractEndDatesFilter(artists, newResults);
                    break;
            }

            FilterResults = newResults;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
    }

    private void AddTotalCount(List<Artist> artists, ObservableCollection<string> results)
    {
        results.Add($"Total number of artists: {artists.Count}");
        FilterSummary = "Artist Count";
    }

    private void AddNamesFilter(List<Artist> artists, ObservableCollection<string> results)
    {
        var names = artists.Select(a => a.Name)
                          .Where(n => !string.IsNullOrEmpty(n))
                          .OrderBy(n => n)
                          .Distinct();
        foreach (var name in names)
        {
            results.Add(name ?? "Unnamed Artist");
        }
        FilterSummary = "Artist Names";
    }

    private void AddGenresFilter(List<Artist> artists, ObservableCollection<string> results)
    {
        var genreGroups = artists.GroupBy(a => a.Genre?.Trim() ?? "Unspecified")
                               .OrderBy(g => g.Key);
        foreach (var group in genreGroups)
        {
            var artistList = string.Join(", ", group.Select(a => a.Name));
            results.Add($"{group.Key} ({group.Count()} artists):");
            results.Add($"Artists: {artistList}");
            results.Add(string.Empty);
        }
        FilterSummary = "Genres Summary";
    }

    private void AddAlbumsSoldFilter(List<Artist> artists, ObservableCollection<string> results)
    {
        var totalAlbums = artists.Sum(a => a.AlbumsSold);
        results.Add($"Total albums sold: {totalAlbums:N0}");
        
        var avgAlbums = artists.Any() ? artists.Average(a => a.AlbumsSold) : 0;
        results.Add($"Average albums per artist: {avgAlbums:N0}\n");

        // Top 5 sellers
        var topSellers = artists.OrderByDescending(a => a.AlbumsSold).Take(5);
        results.Add("Top 5 Selling Artists:");
        foreach (var artist in topSellers)
        {
            results.Add($"  {artist.Name}: {artist.AlbumsSold:N0} albums");
        }

        // Rest of the artists
        var remainingArtists = artists.OrderByDescending(a => a.AlbumsSold).Skip(5);
        if (remainingArtists.Any())
        {
            results.Add("\nOther Artists:");
            foreach (var artist in remainingArtists)
            {
                results.Add($"  {artist.Name}: {artist.AlbumsSold:N0} albums");
            }
        }
        
        FilterSummary = "Album Sales Statistics";
    }

    private void AddContractEndDatesFilter(List<Artist> artists, ObservableCollection<string> results)
    {
        var today = DateTime.Today;
        var thisYear = new DateTime(today.Year, 12, 31, 0, 0, 0, DateTimeKind.Local);
        var nextYear = thisYear.AddYears(1);

        var contractGroups = new[]
        {
            ("Expired Contracts", artists.Where(a => a.ContractEndDate < today).OrderByDescending(a => a.ContractEndDate)),
            ($"Ending This Year ({today.Year})", artists.Where(a => a.ContractEndDate >= today && a.ContractEndDate <= thisYear).OrderBy(a => a.ContractEndDate)),
            ($"Ending Next Year ({today.Year + 1})", artists.Where(a => a.ContractEndDate > thisYear && a.ContractEndDate <= nextYear).OrderBy(a => a.ContractEndDate)),
            ("Future Contract Endings", artists.Where(a => a.ContractEndDate > nextYear).OrderBy(a => a.ContractEndDate))
        };

        foreach (var (groupTitle, groupArtists) in contractGroups)
        {
            if (groupArtists.Any())
            {
                results.Add(groupTitle);
                foreach (var artist in groupArtists)
                {
                    results.Add($"  {artist.Name}: {artist.ContractEndDate:d}");
                }
                results.Add(string.Empty);
            }
        }
        
        FilterSummary = "Contract Status";
    }
}
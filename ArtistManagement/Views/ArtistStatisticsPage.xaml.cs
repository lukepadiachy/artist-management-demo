using ArtistManagement.ViewModels;

namespace ArtistManagement.Views;

public partial class ArtistStatisticsPage : ContentPage
{
    private readonly ArtistStatisticsViewModel _viewModel;

    public ArtistStatisticsPage(ArtistStatisticsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
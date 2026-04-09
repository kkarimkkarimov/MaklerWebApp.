namespace MaklerWebApp.MVC.Models;

public class FavoritesIndexViewModel
{
    public IReadOnlyList<FavoriteItemViewModel> Items { get; set; } = Array.Empty<FavoriteItemViewModel>();
}

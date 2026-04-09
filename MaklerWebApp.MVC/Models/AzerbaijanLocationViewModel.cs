namespace MaklerWebApp.MVC.Models;

public class AzerbaijanLocationViewModel
{
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<string> Districts { get; set; } = Array.Empty<string>();
}

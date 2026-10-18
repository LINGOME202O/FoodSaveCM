namespace FoodSaveCM.Models;

public class Shop
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Icon { get; set; } = "🏪";
    public string Quarter { get; set; } = "";
    public string OpenText { get; set; } = "";
    public double Distance { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public int OffersCount => Services.AppState.Products.Count(p => p.Shop == Name && p.Stock > 0);
    public string Line => $"{Type} · {Quarter} · {Distance:0.0} km";
    public string OffersText => $"{OffersCount} offre(s) anti-gaspi";
}

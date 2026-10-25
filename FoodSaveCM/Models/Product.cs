namespace FoodSaveCM.Models;

public class Product : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Details { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public decimal OldPrice { get; set; }
    public string Shop { get; set; } = "";
    public string ShopSub { get; set; } = "";
    public string Address { get; set; } = "";
    public double Lat { get; set; }
    public double Lon { get; set; }
    public double Distance { get; set; }
    public string PickupText { get; set; } = "Aujourd'hui 18h-20h";
    public int PickupEndHour { get; set; } = 20;
    public bool IsBasket { get; set; }
    public bool IsMine { get; set; }
    public int OwnerId { get; set; }
    public List<string> Contents { get; set; } = new();

    int _stock = 5;
    public int Stock
    {
        get => _stock;
        set { _stock = value; Raise(); Raise(nameof(StockText)); }
    }

    bool _fav;
    public bool IsFavorite
    {
        get => _fav;
        set { _fav = value; Raise(); Raise(nameof(HeartIcon)); }
    }

    public string HeartIcon => IsFavorite ? "❤️" : "🤍";
    public string PriceText => $"{Price:0} FCFA";
    public string OldPriceText => OldPrice > 0 ? $"{OldPrice:0} FCFA" : "";
    public bool HasDiscount => OldPrice > Price;
    public int Discount => HasDiscount ? (int)Math.Round((1 - Price / OldPrice) * 100) : 0;
    public string DiscountText => $"- {Discount}%";
    public string DistanceText => $"{Distance:0.0} km";
    public string ShopLine => $"🏪 {Shop}  ·  📍 {Address}  ·  {DistanceText}";
    public string StockText => Stock <= 0 ? "⛔ Épuisé" : Stock <= 3 ? $"🔥 Plus que {Stock} disponible(s) !" : $"✅ {Stock} disponibles";
}

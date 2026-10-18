using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FoodSaveCM.Models;

namespace FoodSaveCM.Services;

/// <summary>État global de l'application (données de démonstration en mémoire).
/// À remplacer plus tard par une vraie API / base de données.</summary>
public static class AppState
{
    public static ObservableCollection<Product> Products { get; } = new();
    public static ObservableCollection<CartItem> Cart { get; } = new();
    public static ObservableCollection<Reservation> Reservations { get; } = new();
    public static List<Shop> Shops { get; } = new();

    public static event Action? CartChanged;
    public static event Action? FavoritesChanged;
    public static void RaiseCartChanged() => CartChanged?.Invoke();

    static readonly CultureInfo Fr = new("fr-FR");

    static AppState() => Seed();

    // ------------------------------------------------------------ Session
    public const string DemoEmail = "landry@email.com";
    public const string DemoPassword = "123456";

    public static bool IsLoggedIn { get => Preferences.Get("logged_in", false); set => Preferences.Set("logged_in", value); }
    public static string FullName { get => Preferences.Get("user_name", "Lingome Collins Landry"); set => Preferences.Set("user_name", value); }
    public static string FirstName { get => Preferences.Get("user_first", "Landry"); set => Preferences.Set("user_first", value); }
    public static bool DarkMode { get => Preferences.Get("dark", false); set => Preferences.Set("dark", value); }
    public static bool Notifications { get => Preferences.Get("notif", true); set => Preferences.Set("notif", value); }

    static string Hash(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));

    public static bool IsValidEmail(string email)
        => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static bool Login(string email, string password)
    {
        if (email.Equals(DemoEmail, StringComparison.OrdinalIgnoreCase) && password == DemoPassword)
        {
            FullName = "Lingome Collins Landry"; FirstName = "Landry"; IsLoggedIn = true; return true;
        }
        var e = Preferences.Get("user_email", "");
        var p = Preferences.Get("user_pass", "");
        if (e != "" && e.Equals(email, StringComparison.OrdinalIgnoreCase) && p == Hash(password))
        {
            IsLoggedIn = true; return true;
        }
        return false;
    }

    public static void Register(string name, string email, string password)
    {
        Preferences.Set("user_email", email);
        Preferences.Set("user_pass", Hash(password));
        FullName = name;
        FirstName = name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;
        IsLoggedIn = true;
    }

    public static void Logout() => IsLoggedIn = false;

    // ------------------------------------------------------------ Panier
    public static bool AddToCart(Product p)
    {
        if (p.Stock <= 0) return false;
        var existing = Cart.FirstOrDefault(c => c.Product.Id == p.Id);
        if (existing != null) existing.Quantity++;
        else Cart.Add(new CartItem(p));
        RaiseCartChanged();
        return true;
    }

    public static decimal CartTotal => Cart.Sum(c => c.Total);

    // ------------------------------------------------------------ Favoris
    public static void ToggleFavorite(Product p)
    {
        p.IsFavorite = !p.IsFavorite;
        Preferences.Set("favs", string.Join(",", Products.Where(x => x.IsFavorite).Select(x => x.Id)));
        FavoritesChanged?.Invoke();
    }

    // ------------------------------------------------------------ Réservations
    public static Reservation? Reserve(Product p, int qty = 1)
    {
        if (p.Stock < qty || qty < 1) return null;
        p.Stock -= qty;
        var r = new Reservation
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Shop = p.Shop,
            Title = p.IsBasket ? "Panier anti-gaspi" : p.Name,
            Count = qty,
            Pickup = p.PickupText,
            Address = p.Address,
            Lat = p.Lat, Lon = p.Lon,
            Total = p.Price * qty,
            Saved = p.HasDiscount ? (p.OldPrice - p.Price) * qty : 0,
            DateText = "Aujourd'hui, " + DateTime.Now.ToString("d MMM", Fr),
            Code = "FS-" + Random.Shared.Next(100000, 999999),
            Status = ReservationStatus.Upcoming
        };
        Reservations.Insert(0, r);
        return r;
    }

    public static void Cancel(Reservation r)
    {
        r.Status = ReservationStatus.Cancelled;
        var p = Products.FirstOrDefault(x => x.Shop == r.Shop && (x.Name == r.Title || x.IsBasket));
        if (p != null) p.Stock += r.Count;
    }

    // ------------------------------------------------------------ Impact écologique (estimation indicative)
    public static int MealsSaved => Reservations.Where(r => r.Status != ReservationStatus.Cancelled).Sum(r => r.Count);
    public static double Co2Saved => MealsSaved * 2.5;
    public static decimal MoneySaved => Reservations.Where(r => r.Status != ReservationStatus.Cancelled).Sum(r => r.Saved);

    // ------------------------------------------------------------ Tri
    public static readonly string[] SortLabels = { "Pertinence", "Prix croissant", "Plus proche", "Meilleures promos" };

    public static IEnumerable<Product> Sort(IEnumerable<Product> src, int mode) => mode switch
    {
        1 => src.OrderBy(p => p.Price),
        2 => src.OrderBy(p => p.Distance),
        3 => src.OrderByDescending(p => p.Discount),
        _ => src
    };

    // ------------------------------------------------------------ Données de démo
    static void Seed()
    {
        Products.Add(new Product
        {
            Id = 1, Name = "Panier Boulangerie", Details = "Pain et viennoiseries du jour",
            Description = "Assortiment de produits de boulangerie invendus du jour et prêts à être récupérés, idéal pour le petit-déjeuner ou le goûter.",
            Image = "pain.jpg", Category = "Croissant", Price = 1500, OldPrice = 3500, Shop = "Boulangerie Paul",
            Address = "Bonapriso - Douala", Lat = 4.0483, Lon = 9.6990, Distance = 0.8,
            PickupText = "Aujourd'hui 18h-20h", PickupEndHour = 20, IsBasket = true, Stock = 4,
            Contents = { "3 Baguettes traditionnelles", "2 Croissants au beurre", "2 Pains au chocolat", "1 Fougasse aux olives" }
        });
        Products.Add(new Product
        {
            Id = 2, Name = "Tomate fraîche", Details = "Frais . 500g . Juteux",
            Description = "Tomates fraîches et juteuses, parfaites pour vos sauces et salades.",
            Image = "tomate.jpg", Category = "Légumes", Price = 300, OldPrice = 500, Shop = "Marché Mboppi",
            Address = "Mboppi - Douala", Lat = 4.0640, Lon = 9.7180, Distance = 2.1,
            PickupText = "Aujourd'hui 17h-19h", PickupEndHour = 19, Stock = 12
        });
        Products.Add(new Product
        {
            Id = 3, Name = "Avocat", Details = "Naturel . 1 tas . 350g",
            Description = "Un tas d'avocats naturels, à maturité idéale pour une consommation rapide.",
            Image = "avocat.jpg", Category = "Fruits", Price = 250, OldPrice = 400, Shop = "Marché Mboppi",
            Address = "Mboppi - Douala", Lat = 4.0640, Lon = 9.7180, Distance = 2.1,
            PickupText = "Aujourd'hui 17h-19h", PickupEndHour = 19, Stock = 8
        });
        Products.Add(new Product
        {
            Id = 4, Name = "Yaourt nature", Details = "Frais . 250ml . Onctueux",
            Description = "Yaourt nature sucré, date courte : à consommer rapidement.",
            Image = "yaourt.jpg", Category = "Boisson", Price = 300, OldPrice = 600, Shop = "Mini-Market Akwa",
            Address = "Akwa - Douala", Lat = 4.0511, Lon = 9.7043, Distance = 1.4,
            PickupText = "Aujourd'hui 18h-21h", PickupEndHour = 21, Stock = 6
        });
        Products.Add(new Product
        {
            Id = 5, Name = "Panier Légumes frais", Details = "Légumes de saison",
            Description = "Un panier de légumes frais en fin de journée, encore parfaits pour cuisiner.",
            Image = "tomate.jpg", Category = "Légumes", Price = 2000, OldPrice = 4500, Shop = "Carrefour Market",
            Address = "Akwa - Douala", Lat = 4.0530, Lon = 9.7060, Distance = 1.2,
            PickupText = "Aujourd'hui 19h-21h", PickupEndHour = 21, IsBasket = true, Stock = 3,
            Contents = { "1 kg de tomates", "500 g de courgettes", "2 poivrons", "1 botte de persil" }
        });
        Products.Add(new Product
        {
            Id = 6, Name = "Panier Fruits", Details = "Fruits mûrs du jour",
            Description = "Sélection de fruits très mûrs à consommer vite : un délice à petit prix.",
            Image = "avocat.jpg", Category = "Fruits", Price = 1800, OldPrice = 3600, Shop = "Marché Mboppi",
            Address = "Mboppi - Douala", Lat = 4.0640, Lon = 9.7180, Distance = 2.1,
            PickupText = "Aujourd'hui 17h-19h", PickupEndHour = 19, IsBasket = true, Stock = 5,
            Contents = { "4 avocats", "6 bananes", "2 ananas", "1 papaye" }
        });

        var favs = Preferences.Get("favs", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in Products) p.IsFavorite = favs.Contains(p.Id.ToString());

        Shops.AddRange(new[]
        {
            new Shop { Name = "Boulangerie Paul", Type = "Boulangerie", Icon = "🥖", Quarter = "Bonapriso", OpenText = "Ouvert jusqu'à 20h", Distance = 0.8, Lat = 4.0483, Lon = 9.6990 },
            new Shop { Name = "Mini-Market Akwa", Type = "Supérette", Icon = "🏪", Quarter = "Akwa", OpenText = "Ouvert jusqu'à 21h", Distance = 1.4, Lat = 4.0511, Lon = 9.7043 },
            new Shop { Name = "Carrefour Market", Type = "Supermarché", Icon = "🛒", Quarter = "Akwa", OpenText = "Ouvert jusqu'à 21h", Distance = 1.2, Lat = 4.0530, Lon = 9.7060 },
            new Shop { Name = "Marché Mboppi", Type = "Marché", Icon = "🧺", Quarter = "Mboppi", OpenText = "Ouvert jusqu'à 19h", Distance = 2.1, Lat = 4.0640, Lon = 9.7180 },
            new Shop { Name = "Restaurant Chez Mama", Type = "Restaurant", Icon = "🍽", Quarter = "Bonanjo", OpenText = "Ouvert jusqu'à 22h", Distance = 3.0, Lat = 4.0436, Lon = 9.6927 },
        });

        Reservations.Add(new Reservation
        {
            Id = "demo0001", Shop = "Boulangerie Paul - Pain et Patisseries", Title = "1x Panier anti-gaspi", Count = 3,
            DateText = "Aujourd'hui, 30 sept", Pickup = "Aujourd'hui 19h-20h", Address = "Bonapriso - Douala",
            Lat = 4.0483, Lon = 9.6990, Code = "FS-482915", Total = 1500, Saved = 2000, Status = ReservationStatus.Upcoming
        });
        Reservations.Add(new Reservation
        {
            Id = "demo0002", Shop = "Carrefour Market - Fruits et Légumes", Title = "1x Panier anti-gaspi", Count = 5,
            DateText = "Hier, 29 sept", Pickup = "Hier 19h-21h", Address = "Akwa - Douala",
            Lat = 4.0530, Lon = 9.7060, Code = "FS-120774", Total = 2000, Saved = 2500, Status = ReservationStatus.Past
        });
    }
}

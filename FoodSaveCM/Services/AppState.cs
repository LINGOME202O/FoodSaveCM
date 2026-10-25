using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FoodSaveCM.Models;
using SQLite;

namespace FoodSaveCM.Services;

/// <summary>
/// "Back end" local de l'application : base SQLite + logique métier.
/// Les pages utilisent toujours AppState (collections observables pour l'affichage),
/// et chaque modification est enregistrée dans la base de données.
/// </summary>
public static class AppState
{
    // ------------------------------------------------------------ Données observables (affichage)
    public static ObservableCollection<Product> Products { get; } = new();
    public static ObservableCollection<CartItem> Cart { get; } = new();
    public static ObservableCollection<Reservation> Reservations { get; } = new();
    public static List<Shop> Shops { get; } = new();

    public static event Action? CartChanged;
    public static event Action? FavoritesChanged;

    static readonly CultureInfo Fr = new("fr-FR");
    static readonly SQLiteConnection Db = new DatabaseService().Db;
    static bool _loading;
    static UserEntity? _user;

    static AppState()
    {
        SeedShops();
        EnsureSeed();
        LoadProducts();
        LoadUserData();
        Products.CollectionChanged += OnProductsChanged;
    }

    // ------------------------------------------------------------ Session / utilisateur
    public const string DemoEmail = "landry@email.com";
    public const string DemoPassword = "123456";

    public static int CurrentUserId
    {
        get => Preferences.Get("user_id", 0);
        private set => Preferences.Set("user_id", value);
    }

    public static bool IsLoggedIn => _user != null;
    public static string FullName => _user?.FullName ?? "Invité";
    public static string FirstName => _user?.FirstName ?? "";
    public static bool DarkMode { get => Preferences.Get("dark", false); set => Preferences.Set("dark", value); }
    public static bool Notifications { get => Preferences.Get("notif", true); set => Preferences.Set("notif", value); }

    public static bool IsValidEmail(string email)
        => Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static bool EmailExists(string email)
    {
        var e = (email ?? "").Trim().ToLowerInvariant();
        return Db.Table<UserEntity>().Where(u => u.Email == e).Count() > 0;
    }

    public static bool Login(string email, string password)
    {
        var e = (email ?? "").Trim().ToLowerInvariant();
        var u = Db.Table<UserEntity>().Where(x => x.Email == e).FirstOrDefault();
        if (u == null || !VerifyPassword(password, u)) return false;
        CurrentUserId = u.Id;
        LoadUserData();
        return true;
    }

    public static bool Register(string name, string email, string password)
    {
        if (EmailExists(email)) return false;
        var first = name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;
        var id = CreateUser(name, first, email, password);
        CurrentUserId = id;
        LoadUserData();
        return true;
    }

    public static void Logout()
    {
        CurrentUserId = 0;
        LoadUserData();
    }

    static int CreateUser(string fullName, string firstName, string email, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var u = new UserEntity
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            FirstName = firstName.Trim(),
            Salt = Convert.ToBase64String(salt),
            PasswordHash = HashPassword(password, salt)
        };
        Db.Insert(u);
        return u.Id;
    }

    static string HashPassword(string password, byte[] salt)
        => Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32));

    static bool VerifyPassword(string password, UserEntity u)
        => HashPassword(password, Convert.FromBase64String(u.Salt)) == u.PasswordHash;

    // ------------------------------------------------------------ Chargement depuis la base
    static void LoadProducts()
    {
        _loading = true;
        Products.Clear();
        foreach (var e in Db.Table<ProductEntity>().ToList().OrderBy(x => x.OwnerId > 0 ? -x.Id : x.Id))
            Products.Add(ToProduct(e));
        _loading = false;
    }

    static void LoadUserData()
    {
        _loading = true;
        Cart.Clear();
        Reservations.Clear();

        var uid = CurrentUserId;
        _user = uid > 0 ? Db.Find<UserEntity>(uid) : null;
        if (_user == null)
        {
            CurrentUserId = 0;
            foreach (var p in Products) { p.IsFavorite = false; p.IsMine = false; }
        }
        else
        {
            var favIds = Db.Table<FavoriteEntity>().Where(f => f.UserId == uid).ToList().Select(f => f.ProductId).ToHashSet();
            foreach (var p in Products) { p.IsFavorite = favIds.Contains(p.Id); p.IsMine = p.OwnerId == uid; }

            foreach (var c in Db.Table<CartEntity>().Where(x => x.UserId == uid).ToList())
            {
                var p = Products.FirstOrDefault(x => x.Id == c.ProductId);
                if (p == null) continue;
                var item = new CartItem(p);
                item.Quantity = c.Quantity;
                Cart.Add(item);
            }

            foreach (var r in Db.Table<ReservationEntity>().Where(x => x.UserId == uid).ToList().OrderByDescending(x => x.CreatedAt))
                Reservations.Add(ToReservation(r));
        }

        _loading = false;
        CartChanged?.Invoke();
        FavoritesChanged?.Invoke();
    }

    // ------------------------------------------------------------ Produits (publication / suppression)
    static void OnProductsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_loading) return;

        if (e.NewItems != null)
            foreach (Product p in e.NewItems)
            {
                p.OwnerId = CurrentUserId;
                p.IsMine = CurrentUserId > 0;
                Db.InsertOrReplace(ToEntity(p));
            }

        if (e.OldItems != null)
            foreach (Product p in e.OldItems)
            {
                Db.Delete<ProductEntity>(p.Id);
                Db.Execute("DELETE FROM Carts WHERE ProductId = ?", p.Id);
                Db.Execute("DELETE FROM Favorites WHERE ProductId = ?", p.Id);
                foreach (var c in Cart.Where(x => x.Product.Id == p.Id).ToList()) Cart.Remove(c);
                RaiseCartChanged();
            }
    }

    static void SaveProduct(Product p) => Db.InsertOrReplace(ToEntity(p));

    // ------------------------------------------------------------ Panier
    public static void RaiseCartChanged()
    {
        if (!_loading) PersistCart();
        CartChanged?.Invoke();
    }

    static void PersistCart()
    {
        var uid = CurrentUserId;
        if (uid == 0) return;
        Db.RunInTransaction(() =>
        {
            Db.Execute("DELETE FROM Carts WHERE UserId = ?", uid);
            foreach (var item in Cart)
                Db.Insert(new CartEntity { UserId = uid, ProductId = item.Product.Id, Quantity = item.Quantity });
        });
    }

    public static bool AddToCart(Product p)
    {
        if (p.Stock <= 0 || CurrentUserId == 0) return false;
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
        var uid = CurrentUserId;
        if (uid == 0) return;
        p.IsFavorite = !p.IsFavorite;
        if (p.IsFavorite)
            Db.Insert(new FavoriteEntity { UserId = uid, ProductId = p.Id });
        else
            Db.Execute("DELETE FROM Favorites WHERE UserId = ? AND ProductId = ?", uid, p.Id);
        FavoritesChanged?.Invoke();
    }

    // ------------------------------------------------------------ Réservations
    public static Reservation? Reserve(Product p, int qty = 1)
    {
        var uid = CurrentUserId;
        if (uid == 0 || qty < 1 || p.Stock < qty) return null;

        p.Stock -= qty;
        SaveProduct(p);

        var r = new Reservation
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            UserId = uid,
            ProductId = p.Id,
            Shop = p.Shop,
            ShopSub = p.ShopSub,
            Headline = p.IsBasket ? $"Panier {p.Shop}" : p.Name,
            Title = p.IsBasket ? $"{qty}x Panier anti-gaspi" : $"{qty}x {p.Name}",
            Count = qty,
            Pickup = p.PickupText,
            Address = p.Address,
            Lat = p.Lat, Lon = p.Lon,
            Total = p.Price * qty,
            Saved = p.HasDiscount ? (p.OldPrice - p.Price) * qty : 0,
            DateText = "Aujourd'hui, " + DateTime.Now.ToString("d MMM", Fr),
            Code = NewUniqueCode(),
            Status = ReservationStatus.Upcoming
        };
        Db.Insert(ToEntity(r));
        Reservations.Insert(0, r);
        return r;
    }

    static string NewUniqueCode()
    {
        string code;
        do { code = "FS-" + Random.Shared.Next(100000, 999999); }
        while (Db.Table<ReservationEntity>().Where(x => x.Code == code).Count() > 0);
        return code;
    }

    public static void Cancel(Reservation r)
    {
        if (r.Status != ReservationStatus.Upcoming) return;
        r.Status = ReservationStatus.Cancelled;
        Db.Update(ToEntity(r));

        var p = Products.FirstOrDefault(x => x.Id == r.ProductId);
        if (p != null) { p.Stock += r.Count; SaveProduct(p); }
    }

    /// <summary>Espace commerçant : valide un retrait à partir du code (ou du QR) présenté par le client.</summary>
    public static (bool Ok, string Message) ValidatePickup(string code)
    {
        code = (code ?? "").Trim().ToUpperInvariant();
        if (code.Length == 0) return (false, "Saisissez un code de retrait.");

        var ent = Db.Table<ReservationEntity>().Where(x => x.Code == code).FirstOrDefault();
        if (ent == null) return (false, "Code introuvable. Vérifiez le code du client.");
        if (ent.Status == (int)ReservationStatus.Cancelled) return (false, "Cette réservation a été annulée.");
        if (ent.Status == (int)ReservationStatus.Past) return (false, "Ce retrait a déjà été validé.");

        ent.Status = (int)ReservationStatus.Past;
        Db.Update(ent);

        var mem = Reservations.FirstOrDefault(x => x.Id == ent.Id);
        if (mem != null) mem.Status = ReservationStatus.Past;

        var pay = ent.IsPaid ? "Déjà payé en ligne." : $"À encaisser sur place : {ent.Total:0} FCFA.";
        return (true, $"✅ Retrait validé : {ent.Headline} ({ent.Count} article{(ent.Count > 1 ? "s" : "")}). {pay}");
    }

    // ------------------------------------------------------------ Paiement Mobile Money (SIMULÉ)
    public static (bool Ok, string Message) Pay(Reservation r, string method, string phone, string pin)
    {
        if (r.IsPaid) return (false, "Cette réservation est déjà payée.");
        if (r.Status != ReservationStatus.Upcoming) return (false, "Cette réservation n'est plus payable.");

        var digits = Regex.Replace(phone ?? "", @"\D", "");
        if (digits.StartsWith("237") && digits.Length == 12) digits = digits[3..];
        if (!Regex.IsMatch(digits, @"^6\d{8}$")) return (false, "Numéro invalide (exemple : 6XX XXX XXX).");
        if (!Regex.IsMatch(pin ?? "", @"^\d{4,5}$")) return (false, "Le code secret doit contenir 4 ou 5 chiffres.");
        if (pin == "0000") return (false, "Solde insuffisant (simulation).");

        var reference = "PAY-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        Db.Insert(new PaymentEntity
        {
            Id = reference, UserId = r.UserId, ReservationId = r.Id, Amount = r.Total, Method = method,
            PhoneMasked = $"{digits[..3]} *** **{digits[^2..]}", Status = "Payé"
        });

        r.PaymentMethod = method;
        r.PaymentRef = reference;
        r.IsPaid = true;
        Db.Update(ToEntity(r));
        return (true, reference);
    }

    // ------------------------------------------------------------ Mot de passe oublié (code de réinitialisation)
    static readonly Dictionary<string, (string Code, DateTime Expires)> _resetCodes = new();

    /// <summary>Génère un code à 6 chiffres valable 10 minutes. Retourne null si l'email n'existe pas.</summary>
    public static string? RequestPasswordReset(string email)
    {
        var e = (email ?? "").Trim().ToLowerInvariant();
        if (!EmailExists(e)) return null;
        var code = Random.Shared.Next(100000, 999999).ToString();
        _resetCodes[e] = (code, DateTime.UtcNow.AddMinutes(10));
        return code;
    }

    public static bool ResetPassword(string email, string code, string newPassword)
    {
        var e = (email ?? "").Trim().ToLowerInvariant();
        if (newPassword == null || newPassword.Length < 6) return false;
        if (!_resetCodes.TryGetValue(e, out var entry) || entry.Expires < DateTime.UtcNow || entry.Code != (code ?? "").Trim()) return false;

        var u = Db.Table<UserEntity>().Where(x => x.Email == e).FirstOrDefault();
        if (u == null) return false;
        var salt = RandomNumberGenerator.GetBytes(16);
        u.Salt = Convert.ToBase64String(salt);
        u.PasswordHash = HashPassword(newPassword, salt);
        Db.Update(u);
        _resetCodes.Remove(e);
        return true;
    }

    // ------------------------------------------------------------ Avis et notes des commerces
    public static (double Avg, int Count) ShopRating(string shop)
    {
        var list = Db.Table<ReviewEntity>().Where(r => r.Shop == shop).ToList();
        return list.Count == 0 ? (0, 0) : (list.Average(r => r.Stars), list.Count);
    }

    public static bool HasReview(string reservationId)
        => Db.Table<ReviewEntity>().Where(r => r.ReservationId == reservationId).Count() > 0;

    public static bool AddReview(Reservation r, int stars, string comment)
    {
        if (r.Status != ReservationStatus.Past || stars < 1 || stars > 5 || HasReview(r.Id)) return false;
        Db.Insert(new ReviewEntity { UserId = CurrentUserId, ReservationId = r.Id, Shop = r.Shop, Stars = stars, Comment = comment ?? "" });
        return true;
    }

    // ------------------------------------------------------------ Tableau de bord commerçant
    public static MerchantStats GetMerchantStats()
    {
        var uid = CurrentUserId;
        var stats = new MerchantStats();
        if (uid == 0) return stats;

        var mine = Db.Table<ProductEntity>().Where(p => p.OwnerId == uid).ToList();
        var ids = mine.Select(p => p.Id).ToHashSet();
        stats.Published = mine.Count;

        var res = Db.Table<ReservationEntity>().ToList().Where(r => ids.Contains(r.ProductId) && r.Status != (int)ReservationStatus.Cancelled).ToList();
        stats.Reservations = res.Count;
        stats.Pending = res.Count(r => r.Status == (int)ReservationStatus.Upcoming);
        stats.PickedUp = res.Count(r => r.Status == (int)ReservationStatus.Past);
        stats.Revenue = res.Sum(r => r.Total);
        stats.PaidOnline = res.Where(r => r.IsPaid).Sum(r => r.Total);

        stats.TopProducts = res.GroupBy(r => r.ProductId)
            .OrderByDescending(g => g.Sum(x => x.Count))
            .Take(3)
            .Select(g => $"{mine.First(p => p.Id == g.Key).Name} : {g.Sum(x => x.Count)} réservé(s)")
            .ToList();

        var shops = mine.Select(p => p.Shop).Distinct().ToList();
        var reviews = Db.Table<ReviewEntity>().ToList().Where(r => shops.Contains(r.Shop)).ToList();
        stats.RatingCount = reviews.Count;
        stats.Rating = reviews.Count == 0 ? 0 : reviews.Average(r => r.Stars);
        return stats;
    }

    // ------------------------------------------------------------ Alerte "dernière chance"
    public static int EndingSoonCount => Products.Count(p =>
        p.Stock > 0 && (DateTime.Today.AddHours(p.PickupEndHour) - DateTime.Now).TotalMinutes is > 0 and <= 120);

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

    // ------------------------------------------------------------ Conversions modèle <-> base
    static Product ToProduct(ProductEntity e) => new()
    {
        Id = e.Id, Name = e.Name, Details = e.Details, Description = e.Description, Image = e.Image,
        Category = e.Category, Price = e.Price, OldPrice = e.OldPrice, Shop = e.Shop, ShopSub = e.ShopSub,
        Address = e.Address, Lat = e.Lat, Lon = e.Lon, Distance = e.Distance, PickupText = e.PickupText,
        PickupEndHour = e.PickupEndHour, IsBasket = e.IsBasket, OwnerId = e.OwnerId, Stock = e.Stock,
        Contents = (e.ContentsText ?? "").Split('|', StringSplitOptions.RemoveEmptyEntries).ToList()
    };

    static ProductEntity ToEntity(Product p) => new()
    {
        Id = p.Id, Name = p.Name, Details = p.Details, Description = p.Description, Image = p.Image,
        Category = p.Category, Price = p.Price, OldPrice = p.OldPrice, Shop = p.Shop, ShopSub = p.ShopSub,
        Address = p.Address, Lat = p.Lat, Lon = p.Lon, Distance = p.Distance, PickupText = p.PickupText,
        PickupEndHour = p.PickupEndHour, IsBasket = p.IsBasket, OwnerId = p.OwnerId, Stock = p.Stock,
        ContentsText = string.Join("|", p.Contents)
    };

    static Reservation ToReservation(ReservationEntity e) => new()
    {
        Id = e.Id, UserId = e.UserId, ProductId = e.ProductId, Shop = e.Shop, ShopSub = e.ShopSub,
        Headline = e.Headline, Title = e.Title, DateText = e.DateText, Pickup = e.Pickup, Address = e.Address,
        Code = e.Code, Count = e.Count, Lat = e.Lat, Lon = e.Lon, Total = e.Total, Saved = e.Saved,
        PaymentMethod = e.PaymentMethod, PaymentRef = e.PaymentRef, IsPaid = e.IsPaid,
        Status = (ReservationStatus)e.Status
    };

    static ReservationEntity ToEntity(Reservation r) => new()
    {
        Id = r.Id, UserId = r.UserId, ProductId = r.ProductId, Shop = r.Shop, ShopSub = r.ShopSub,
        Headline = r.Headline, Title = r.Title, DateText = r.DateText, Pickup = r.Pickup, Address = r.Address,
        Code = r.Code, Count = r.Count, Lat = r.Lat, Lon = r.Lon, Total = r.Total, Saved = r.Saved,
        IsPaid = r.IsPaid, PaymentMethod = r.PaymentMethod, PaymentRef = r.PaymentRef,
        Status = (int)r.Status
    };

    // ------------------------------------------------------------ Données de démonstration (1er lancement)
    static void SeedShops()
    {
        Shops.AddRange(new[]
        {
            new Shop { Name = "Boulangerie Paul", Type = "Boulangerie", Icon = "🥖", Quarter = "Bonapriso", OpenText = "Ouvert jusqu'à 20h", Distance = 0.8, Lat = 4.0483, Lon = 9.6990 },
            new Shop { Name = "Mini-Market Akwa", Type = "Supérette", Icon = "🏪", Quarter = "Akwa", OpenText = "Ouvert jusqu'à 21h", Distance = 1.4, Lat = 4.0511, Lon = 9.7043 },
            new Shop { Name = "Carrefour Market", Type = "Supermarché", Icon = "🛒", Quarter = "Akwa", OpenText = "Ouvert jusqu'à 21h", Distance = 1.2, Lat = 4.0530, Lon = 9.7060 },
            new Shop { Name = "Marché Mboppi", Type = "Marché", Icon = "🧺", Quarter = "Mboppi", OpenText = "Ouvert jusqu'à 19h", Distance = 2.1, Lat = 4.0640, Lon = 9.7180 },
            new Shop { Name = "Restaurant Chez Mama", Type = "Restaurant", Icon = "🍽", Quarter = "Bonanjo", OpenText = "Ouvert jusqu'à 22h", Distance = 3.0, Lat = 4.0436, Lon = 9.6927 },
        });
    }

    static void EnsureSeed()
    {
        if (Db.Table<UserEntity>().Count() > 0) return;

        var uid = CreateUser("Lingome Collins Landry", "Landry", DemoEmail, DemoPassword);

        var seed = new List<Product>
        {
            new()
            {
                Id = 1, Name = "Panier Boulangerie", Details = "Pain et viennoiseries du jour",
                Description = "Assortiment de produits de boulangerie invendus du jour et prets à être récupérées et idéal pour le petit-déjeuner ou le goûter.",
                Image = "pain.jpg", Category = "Croissant", Price = 1500, OldPrice = 3500, Shop = "Boulangerie Paul", ShopSub = "Pain et Patisseries",
                Address = "Bonapriso - Douala", Lat = 4.0483, Lon = 9.6990, Distance = 0.8,
                PickupText = "Aujourd'hui 18-20h", PickupEndHour = 20, IsBasket = true, Stock = 4, OwnerId = uid,
                Contents = { "3 Baguettes traditionnelles", "2 Croissants au beurre", "2 Pains au chocolat", "1 Fougasse aux olives" }
            },
            new()
            {
                Id = 2, Name = "Tomate fraiche", Details = "Frais . 500g . Juteux",
                Description = "Tomates fraîches et juteuses, parfaites pour vos sauces et salades.",
                Image = "tomate.jpg", Category = "Légumes", Price = 300, OldPrice = 500, Shop = "Marché Mboppi", ShopSub = "Fruits et Légumes",
                Address = "Mboppi - Douala", Lat = 4.0640, Lon = 9.7180, Distance = 2.1,
                PickupText = "Aujourd'hui 17h-19h", PickupEndHour = 19, Stock = 12
            },
            new()
            {
                Id = 3, Name = "Avocat", Details = "Naturel . 1 tas .350g",
                Description = "Un tas d'avocats naturels, à maturité idéale pour une consommation rapide.",
                Image = "avocat.jpg", Category = "Fruits", Price = 250, OldPrice = 400, Shop = "Marché Mboppi", ShopSub = "Fruits et Légumes",
                Address = "Mboppi - Douala", Lat = 4.0640, Lon = 9.7180, Distance = 2.1,
                PickupText = "Aujourd'hui 17h-19h", PickupEndHour = 19, Stock = 8
            },
            new()
            {
                Id = 4, Name = "Yaourt nature", Details = "Frais . 250ml . Juteux",
                Description = "Yaourt nature sucré, date courte : à consommer rapidement.",
                Image = "yaourt.jpg", Category = "Boisson", Price = 300, OldPrice = 600, Shop = "Mini-Market Akwa", ShopSub = "Produits frais",
                Address = "Akwa - Douala", Lat = 4.0511, Lon = 9.7043, Distance = 1.4,
                PickupText = "Aujourd'hui 18h-21h", PickupEndHour = 21, Stock = 6
            },
            new()
            {
                Id = 5, Name = "Panier Légumes frais", Details = "Légumes de saison",
                Description = "Un panier de légumes frais en fin de journée, encore parfaits pour cuisiner.",
                Image = "tomate.jpg", Category = "Légumes", Price = 2000, OldPrice = 4500, Shop = "Carrefour Market", ShopSub = "Fruits et Légumes",
                Address = "Akwa - Douala", Lat = 4.0530, Lon = 9.7060, Distance = 1.2,
                PickupText = "Aujourd'hui 19h-21h", PickupEndHour = 21, IsBasket = true, Stock = 3,
                Contents = { "1 kg de tomates", "500 g de courgettes", "2 poivrons", "1 botte de persil" }
            },
            new()
            {
                Id = 6, Name = "Panier Fruits", Details = "Fruits mûrs du jour",
                Description = "Sélection de fruits très mûrs à consommer vite : un délice à petit prix.",
                Image = "avocat.jpg", Category = "Fruits", Price = 1800, OldPrice = 3600, Shop = "Marché Mboppi", ShopSub = "Fruits et Légumes",
                Address = "Mboppi - Douala", Lat = 4.0640, Lon = 9.7180, Distance = 2.1,
                PickupText = "Aujourd'hui 17h-19h", PickupEndHour = 19, IsBasket = true, Stock = 5,
                Contents = { "4 avocats", "6 bananes", "2 ananas", "1 papaye" }
            },
        };
        foreach (var p in seed) Db.Insert(ToEntity(p));

        var seedReviews = new (string Shop, int Stars, string Comment)[]
        {
            ("Boulangerie Paul", 5, "Pain encore chaud, très bon !"), ("Boulangerie Paul", 4, "Super panier pour le prix."),
            ("Boulangerie Paul", 5, "Je recommande."), ("Boulangerie Paul", 4, ""),
            ("Carrefour Market", 4, "Légumes frais."), ("Carrefour Market", 3, ""),
            ("Marché Mboppi", 5, "Avocats parfaits."), ("Marché Mboppi", 4, ""), ("Marché Mboppi", 4, ""),
            ("Mini-Market Akwa", 4, "Yaourt bien frais."),
        };
        for (int i = 0; i < seedReviews.Length; i++)
            Db.Insert(new ReviewEntity { UserId = 0, ReservationId = "seed" + i, Shop = seedReviews[i].Shop, Stars = seedReviews[i].Stars, Comment = seedReviews[i].Comment });

        Db.Insert(ToEntity(new Reservation
        {
            Id = "demo0001", UserId = uid, ProductId = 1, Shop = "Boulangerie Paul", ShopSub = "Pain et Patisseries",
            Headline = "Panier Boulangerie Paul", Title = "1x Panier anti-gaspi", Count = 3,
            DateText = "Aujourd'hui, 30 sept", Pickup = "Aujourd'hui 19h-20h", Address = "Bonapriso - Douala",
            Lat = 4.0483, Lon = 9.6990, Code = "FS-482915", Total = 1500, Saved = 2000, Status = ReservationStatus.Upcoming
        }));
        Db.Insert(ToEntity(new Reservation
        {
            Id = "demo0002", UserId = uid, ProductId = 5, Shop = "Carrefour Market", ShopSub = "Fruits et Légumes",
            Headline = "Panier Carrefour Market", Title = "1x Panier anti-gaspi", Count = 5,
            DateText = "Hier, 29 sept", Pickup = "Hier 19h-21h", Address = "Akwa - Douala",
            Lat = 4.0530, Lon = 9.7060, Code = "FS-120774", Total = 2000, Saved = 2500, Status = ReservationStatus.Past
        }));
    }
}

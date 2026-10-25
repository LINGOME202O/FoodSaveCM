using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class HomePage : ContentPage
{
    string _cat = "";   // "" = aucune catégorie sélectionnée
    string _query = "";
    int _sort = 0;

    // (Libellé affiché, catégorie interne, icône)
    static readonly (string Label, string Cat, string Icon)[] Cats =
    {
        ("Fruits", "Fruits", "🍎"),
        ("Legumes", "Légumes", "🥬"),
        ("Croissant", "Croissant", "🥐"),
        ("Boisson", "Boisson", "🥤"),
    };

    static bool _alerted;

    public HomePage()
    {
        InitializeComponent();
        BuildChips();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Hello.Text = $"Bonjour,{AppState.FirstName}";
        Refresh();

        // Alerte "dernière chance" (une seule fois par session, si les alertes sont activées)
        if (!_alerted && AppState.Notifications && AppState.IsLoggedIn)
        {
            _alerted = true;
            var n = AppState.EndingSoonCount;
            if (n > 0) _ = Ui.Show($"⏳ Dernière chance : {n} offre(s) se terminent dans moins de 2h !");
        }
    }

    void BuildChips()
    {
        Chips.Children.Clear();
        foreach (var (label, cat, icon) in Cats)
        {
            var selected = cat == _cat;
            var chip = new Border
            {
                Style = (Style)Application.Current!.Resources["Chip"],
                Padding = new Thickness(10, 5),
                BackgroundColor = selected ? Color.FromArgb("#9FE3B5") : Color.FromArgb("#E6F7E9"),
                Content = new Label { Text = $"{icon} {label}", FontSize = 13, TextColor = Colors.Black, VerticalOptions = LayoutOptions.Center }
            };
            var tap = new TapGestureRecognizer();
            var c = cat;
            tap.Tapped += (_, _) => { _cat = _cat == c ? "" : c; BuildChips(); Refresh(); };
            chip.GestureRecognizers.Add(tap);
            Chips.Children.Add(chip);
        }
    }

    void Refresh()
    {
        var items = AppState.Products.Where(p =>
            p.Stock > 0 &&
            (_cat == "" ? !p.IsBasket : p.Category == _cat) &&
            (string.IsNullOrWhiteSpace(_query) ||
             p.Name.Contains(_query, StringComparison.OrdinalIgnoreCase) ||
             p.Shop.Contains(_query, StringComparison.OrdinalIgnoreCase)));

        List.ItemsSource = AppState.Sort(items, _sort).ToList();
    }

    void OnSearch(object? sender, TextChangedEventArgs e) { _query = e.NewTextValue ?? ""; Refresh(); }

    async void OnFilter(object? sender, TappedEventArgs e)
    {
        var choice = await DisplayActionSheet("Trier par", "Annuler", null, AppState.SortLabels);
        var idx = Array.IndexOf(AppState.SortLabels, choice);
        if (idx >= 0) { _sort = idx; Refresh(); }
    }

    void OnAllCategories(object? sender, TappedEventArgs e) { _cat = ""; BuildChips(); Refresh(); }

    async void OnSeeAll(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//main/boutique");
}

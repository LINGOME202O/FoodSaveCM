using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class HomePage : ContentPage
{
    string _cat = "Fruits";
    string _query = "";
    int _sort = 0;

    static readonly (string Name, string Icon)[] Cats =
    {
        ("Fruits", "🍎"), ("Légumes", "🥬"),
        ("Croissant", "🥐"), ("Boisson", "🥤"), ("Paniers", "🧺")
    };

    public HomePage()
    {
        InitializeComponent();
        BuildChips();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Hello.Text = $"Bonjour, {AppState.FirstName}";
        Refresh();
    }

    void BuildChips()
    {
        Chips.Children.Clear();
        foreach (var (name, icon) in Cats)
        {
            var selected = name == _cat;
            var btn = new Button
            {
                Text = $"{icon} {name}",
                FontSize = 13,
                HeightRequest = 38,
                CornerRadius = 19,
                Padding = new Thickness(14, 0),
                BackgroundColor = selected ? Color.FromArgb("#009B4D") : Color.FromArgb("#E8F8EC"),
                TextColor = selected ? Colors.White : Color.FromArgb("#1B5E20")
            };
            btn.Clicked += (_, _) => { _cat = name; BuildChips(); Refresh(); };
            Chips.Children.Add(btn);
        }
    }

    void Refresh()
    {
        var items = AppState.Products.Where(p =>
            p.Stock > 0 &&
            (_cat == "Fruits" || p.Category == _cat || (_cat == "Paniers" && p.IsBasket)) &&
            (string.IsNullOrWhiteSpace(_query) ||
             p.Name.Contains(_query, StringComparison.OrdinalIgnoreCase) ||
             p.Shop.Contains(_query, StringComparison.OrdinalIgnoreCase)));

        List.ItemsSource = AppState.Sort(items, _sort).ToList();
    }

    void OnSearch(object? sender, TextChangedEventArgs e) { _query = e.NewTextValue ?? ""; Refresh(); }

    async void OnFilter(object? sender, EventArgs e)
    {
        var choice = await DisplayActionSheet("Trier par", "Annuler", null, AppState.SortLabels);
        var idx = Array.IndexOf(AppState.SortLabels, choice);
        if (idx >= 0) { _sort = idx; Refresh(); }
    }

    void OnAllCategories(object? sender, TappedEventArgs e) { _cat = "Fruits"; BuildChips(); Refresh(); }

    async void OnSeeAll(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//main/boutique");
}

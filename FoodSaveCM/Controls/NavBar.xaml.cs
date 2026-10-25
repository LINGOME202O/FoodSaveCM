using CommunityToolkit.Maui.Behaviors;

namespace FoodSaveCM.Controls;

/// <summary>Barre de navigation grise du bas (Accueil, Boutique, Carte, Favoris, Profil).</summary>
public partial class NavBar : ContentView
{
    public static readonly BindableProperty CurrentProperty = BindableProperty.Create(
        nameof(Current), typeof(string), typeof(NavBar), "Accueil",
        propertyChanged: (b, _, _) => ((NavBar)b).Build());

    public string Current { get => (string)GetValue(CurrentProperty); set => SetValue(CurrentProperty, value); }

    record Item(string Label, string Icon, string Route);

    static readonly Item[] Items =
    {
        new("Accueil", "nav_home", "//main/home"),
        new("Boutique", "nav_bag", "//main/boutique"),
        new("Carte", "nav_cart", "//main/cart"),
        new("Favoris", "nav_heart", "//main/favoris"),
        new("Profil", "nav_person", "//main/profil"),
    };

    public NavBar()
    {
        InitializeComponent();
        Build();
    }

    void Build()
    {
        Row.Children.Clear();
        for (int i = 0; i < Items.Length; i++)
        {
            var it = Items[i];
            var selected = it.Label == Current;

            var icon = new Image
            {
                Source = selected ? it.Icon + "_f.png" : it.Icon + ".png",
                WidthRequest = 28,
                HeightRequest = 28,
                HorizontalOptions = LayoutOptions.Center
            };
            var tint = new IconTintColorBehavior();
            tint.SetAppThemeColor(IconTintColorBehavior.TintColorProperty, Colors.Black, Colors.White);
            icon.Behaviors.Add(tint);

            var label = new Label
            {
                Text = it.Label,
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Center,
                FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None
            };
            label.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);

            var cell = new VerticalStackLayout { Spacing = 2, Children = { icon, label } };
            var route = it.Route;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) => await Shell.Current.GoToAsync(route);
            cell.GestureRecognizers.Add(tap);

            Grid.SetColumn(cell, i);
            Row.Children.Add(cell);
        }
    }
}

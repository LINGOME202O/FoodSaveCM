namespace FoodSaveCM.Pages;

public record OnboardingSlide(string Icon, string Title, string Text);

public partial class OnboardingPage : ContentPage
{
    readonly List<OnboardingSlide> _slides = new()
    {
        new("🍀", "Bienvenue sur FoodSaveCM", "Réduisez le gaspillage alimentaire à Douala en sauvant les invendus de vos commerces proches."),
        new("🧺", "Des paniers à prix réduit", "Boulangeries, marchés et restaurants proposent leurs invendus jusqu'à -60 %. Réservez en quelques secondes."),
        new("📲", "Retirez avec votre QR code", "Payez en ligne ou sur place, puis présentez votre QR code au commerçant sur le créneau choisi."),
    };

    public OnboardingPage()
    {
        InitializeComponent();
        Slides.ItemsSource = _slides;
    }

    void OnPositionChanged(object? sender, PositionChangedEventArgs e)
        => NextBtn.Text = e.CurrentPosition >= _slides.Count - 1 ? "Commencer" : "Suivant";

    async void OnNext(object? sender, EventArgs e)
    {
        if (Slides.Position < _slides.Count - 1) Slides.Position++;
        else await Finish();
    }

    async void OnSkip(object? sender, TappedEventArgs e) => await Finish();

    static async Task Finish()
    {
        Preferences.Set("onboarded", true);
        await Shell.Current.GoToAsync("//login");
    }
}

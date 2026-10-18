using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage() => InitializeComponent();

    void ShowError(string msg) { ErrorLabel.Text = msg; ErrorLabel.IsVisible = true; }

    async void OnLogin(object? sender, EventArgs e)
    {
        var email = EmailField.Text?.Trim() ?? "";
        var pass = PassField.Text ?? "";

        if (!AppState.IsValidEmail(email)) { ShowError("Adresse email invalide."); return; }
        if (pass.Length < 6) { ShowError("Le mot de passe doit contenir au moins 6 caractères."); return; }

        if (AppState.Login(email, pass))
        {
            ErrorLabel.IsVisible = false;
            await Shell.Current.GoToAsync("//main/home");
        }
        else ShowError("Email ou mot de passe incorrect.");
    }

    async void OnForgot(object? sender, TappedEventArgs e)
    {
        var mail = await DisplayPromptAsync("Mot de passe oublié", "Entrez votre email pour recevoir un lien de réinitialisation :",
            "Envoyer", "Annuler", "exemple@email.com", keyboard: Keyboard.Email);
        if (!string.IsNullOrWhiteSpace(mail))
            await DisplayAlert("Email envoyé", $"Un lien de réinitialisation a été envoyé à {mail}. (simulation)", "OK");
    }

    async void OnRegister(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//register");
}

using FoodSaveCM.Services;

namespace FoodSaveCM.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage() => InitializeComponent();

    void ShowError(string msg) { ErrorLabel.Text = msg; ErrorLabel.IsVisible = true; }

    async void OnRegister(object? sender, EventArgs e)
    {
        var name = NameField.Text?.Trim() ?? "";
        var email = EmailField.Text?.Trim() ?? "";
        var pass = PassField.Text ?? "";

        if (name.Length < 3) { ShowError("Entrez votre nom complet."); return; }
        if (!AppState.IsValidEmail(email)) { ShowError("Adresse email invalide."); return; }
        if (pass.Length < 6) { ShowError("Le mot de passe doit contenir au moins 6 caractères."); return; }
        if (pass != ConfirmField.Text) { ShowError("Les mots de passe ne correspondent pas."); return; }

        AppState.Register(name, email, pass);
        await Ui.Show($"Bienvenue {AppState.FirstName} 🍀");
        await Shell.Current.GoToAsync("//main/home");
    }

    async void OnLogin(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//login");
}

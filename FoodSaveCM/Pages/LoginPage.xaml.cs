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
        var mail = await DisplayPromptAsync("Mot de passe oublié", "Entrez l'email de votre compte :",
            "Continuer", "Annuler", "exemple@email.com", keyboard: Keyboard.Email);
        if (string.IsNullOrWhiteSpace(mail)) return;

        var code = AppState.RequestPasswordReset(mail);
        if (code == null)
        {
            await DisplayAlert("Compte introuvable", "Aucun compte ne correspond à cet email.", "OK");
            return;
        }

        // Sans serveur d'envoi d'email, le code est affiché ici (simulation).
        await DisplayAlert("Code de réinitialisation",
            $"Simulation d'envoi par email : votre code est {code} (valable 10 minutes).", "OK");

        var entered = await DisplayPromptAsync("Vérification", "Saisissez le code reçu :", "Valider", "Annuler",
            "000000", maxLength: 6, keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(entered)) return;

        var newPass = await DisplayPromptAsync("Nouveau mot de passe", "Au moins 6 caractères :", "Enregistrer", "Annuler",
            "nouveau mot de passe");
        if (string.IsNullOrWhiteSpace(newPass) || newPass.Length < 6)
        {
            await DisplayAlert("Mot de passe trop court", "Il doit contenir au moins 6 caractères.", "OK");
            return;
        }

        if (AppState.ResetPassword(mail, entered, newPass))
            await DisplayAlert("Succès", "Votre mot de passe a été modifié. Vous pouvez vous connecter.", "OK");
        else
            await DisplayAlert("Code invalide", "Le code est incorrect ou a expiré.", "OK");
    }

    async void OnRegister(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//register");
}

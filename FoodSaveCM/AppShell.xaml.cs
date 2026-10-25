using FoodSaveCM.Pages;
using FoodSaveCM.Services;

namespace FoodSaveCM;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages "poussées" (hors barre d'onglets)
        Routing.RegisterRoute("detail", typeof(DetailPage));
        Routing.RegisterRoute("publish", typeof(PublishPage));
        Routing.RegisterRoute("reservations", typeof(ReservationsPage));
        Routing.RegisterRoute("confirmation", typeof(ConfirmationPage));
        Routing.RegisterRoute("publications", typeof(PublicationsPage));
        Routing.RegisterRoute("shops", typeof(ShopsPage));
        Routing.RegisterRoute("validate", typeof(ValidatePickupPage));
        Routing.RegisterRoute("payment", typeof(PaymentPage));
        Routing.RegisterRoute("dashboard", typeof(DashboardPage));

        // Connexion automatique si la session est déjà ouverte
        if (AppState.IsLoggedIn)
            Dispatcher.Dispatch(async () => await GoToAsync("//main/home"));
        else if (!Preferences.Get("onboarded", false))
            Dispatcher.Dispatch(async () => await GoToAsync("//onboarding"));
    }
}

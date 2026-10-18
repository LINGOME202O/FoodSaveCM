# FoodSaveCM – Front-end .NET MAUI

## 1. Créer le projet
1. Visual Studio 20226 → **Créer un projet** → **Application .NET MAUI** → nom **`FoodSaveCM`** (le nom doit être exactement celui-ci, c'est le namespace utilisé).
2. Choisissez **.NET 10** (recommandé).
3. **Supprimez** du projet généré par défaut : `MainPage.xaml` et `MainPage.xaml.cs`.
4. **Modifiez les fichiers** (remplacer les fichiers existants) :
   `App.xaml`, `App.xaml.cs`, `AppShell.xaml`, `AppShell.xaml.cs`, `MauiProgram.cs`,
   et créez les dossiers `Models/`, `Services/`, `Controls/`, `Pages/`.
5. Mettez les images de `Resources/Images/` dans le dossier du même nom
   (pain.jpg, tomate.jpg, avocat.jpg, yaourt.jpg, profil.jpg).

## 2. Packages NuGet (clic droit projet → Gérer les packages NuGet)
- `CommunityToolkit.Maui` **9.1.1** (pour .NET 8)  — toasts
- `QRCoder` **1.4.3**                               — génération des QR codes

(Si vous êtes en .NET 9 : prenez `CommunityToolkit.Maui` 12.x ; si une erreur de version apparaît, laissez NuGet proposer la version compatible.)

## 3. Si vous utilisez .NET 9
Dans `App.xaml.cs`, remplacez `MainPage = new AppShell();` par :
```csharp
protected override Window CreateWindow(IActivationState? activationState) => new Window(new AppShell());
```

## 4. Android – permissions (Platforms/Android/AndroidManifest.xml)
```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.READ_MEDIA_IMAGES" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" android:maxSdkVersion="32" />
```

## 5. Compte de test
`landry@email.com` / `123456`  (ou créez un compte via « S'inscrire »)

## Fonctionnalités
Connexion/Inscription (validation) · Accueil (recherche, catégories, tri) · Boutique (paniers) ·
Détail (compte à rebours du retrait, partage, itinéraire) · Panier (quantités) · Favoris (persistants) ·
Réservation + QR code + code de retrait · Mes réservations (À venir / Passées / Annulées, annulation) ·
Publier un invendu (photo, calcul de la réduction) · Mes publications (glisser pour supprimer) ·
Carte des commerces · Profil (impact écologique, mode sombre, alertes, déconnexion).

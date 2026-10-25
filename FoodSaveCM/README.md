# 🍀 FoodSaveCM

Application mobile **.NET MAUI** de lutte contre le gaspillage alimentaire à Douala : les restaurants,
boulangeries et marchés publient leurs invendus à prix réduit, les clients proches les réservent et les retirent avec un QR code.

## Fonctionnalités
**Client**
- Inscription / connexion sécurisée (mots de passe chiffrés PBKDF2), mot de passe oublié par code
- Accueil avec recherche, catégories et tri ; boutique de paniers anti-gaspi
- Détail d'un panier : contenu, créneau de retrait, compte à rebours, note du commerce, partage, itinéraire
- Panier, favoris, réservation avec **QR code** et code de retrait unique
- **Paiement Mobile Money (simulé)** : Orange Money / MTN MoMo
- Mes réservations (à venir, passées, annulées), annulation avec remise en stock
- Avis et notes (1 à 5 étoiles) sur les commerces
- Impact écologique (repas sauvés, CO₂ évité, économies), mode sombre, alertes « dernière chance »

**Commerçant**
- Publication d'un invendu (photo, prix, réduction, créneau) et gestion de ses publications
- **Validation d'un retrait** par scan du QR code (caméra / galerie) ou saisie du code
- **Tableau de bord** : réservations, chiffre d'affaires, paiements en ligne, produits les plus réservés, note moyenne

## Architecture
```
Pages/      Écrans (XAML + code-behind) : Login, Home, Detail, Cart, Payment, Dashboard...
Controls/   Composants réutilisables : Field, ProductCard, NavBar
Models/     Modèles d'affichage (Product, Reservation, CartItem) + tables SQLite (Entities.cs)
Services/   AppState (logique métier + accès base), DatabaseService (SQLite), QrScanner, QrHelper
Resources/  Images et styles
```
Back end local : **SQLite** (sqlite-net-pcl) avec tables Users, Products, Reservations, Carts, Favorites, Payments, Reviews.

## Technologies
.NET MAUI (.NET 10) · SQLite · CommunityToolkit.Maui · QRCoder (génération) · ZXing.Net + SkiaSharp (lecture QR)

## Lancer le projet
1. Visual Studio 2026 avec la charge de travail « Développement d'applications mobiles avec .NET »
2. Restaurer les packages NuGet, puis lancer sur Android (émulateur ou téléphone) ou Windows
3. Compte de démonstration : `landry@email.com` / `123456`

## Limites et évolutions
Les données sont stockées sur l'appareil. Évolution prévue : API ASP.NET Core pour partager les données entre commerçants et clients,
paiement Mobile Money réel via un opérateur, envoi d'emails réels.

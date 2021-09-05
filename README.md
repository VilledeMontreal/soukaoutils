([Français](#french-version))

<a id='english-version' class='anchor' aria-hidden='true'/>

# Soukaoutils
Soukaoutils is a tools sharing space where users can let out any of their belongings for a stated daily fee. A registered member can reserve a tool for any period of time when the tool is available.

## OAuth 2.0 and OIDC

The application is composed of three components:
-	An authorization server
-	A resources server
-	A relying party (RP)

The authorization server allows for any type of authentication that implements OIDC. It also provides its own OIDC authentication and OAuth 2.0 authorization flows. The identity itself along with the applications roles are provided using ASP.Net Core Identity. 


### Details

A user is asked to authenticate with the identity server. The identity server issues an authorization code which the client application can after that exchange for an access token. Scopes required to access a given resource and roles of the current user are added to the access token. When the token is sent over to the resource server, and before any response is prepared, the token will be introspected and validated to ensure that: 
-	It is a valid token
-	The client application is granted the required scope
-	The user initiating the request has the required role(s)

### Installation
Clone the repository and run the follwoing commands:
```
dotnet restore 
dotnet run
```
Create the ASP.Net Core Identity tables in the database:
```
dotnet ef migrations add initial_migration
dotnet ef database update

```
### Testing
Run all 3 components in your environment:
```
https://localhost:5001/   -- Identiy Server
https://localhost:5002/   -- Auth Client (App)
https://localhost:6001/   -- Resource Server (Open Data)
```
Then register a new account on the Identity server itself or using and external (Google, Azure for now, more could be added).
The migrations populates the database with 3 pre-configured users:

`admin@soukaoutils.com`       A user with an *Admin* role 

`manager@soukaoutils.com`     A user with an *Manager* role 

`joe@soukaoutils.com`        No specific roles 

Password is `P@ssw0rd`

### Adding OIDC authentication providers
You can add other OIDC providers. For that end, you will have to register an app with that provider first then inject the OIDC provider's service using the ClientId and ClientSecret as follows:
```
                .AddOpenIdConnect("oidc", "Identité Ville MTL", options =>
                {
                    options.Authority = <Authority URL>;

                    options.ClientId = <ClientID>;
                    options.ClientSecret = <ClientSecret>; 
                    options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                    // Required scopes
                    options.Scope.Add("profile");
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    ....

```

### License

The source code of this project is distributed under the [MIT License](LICENSE).

### Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md#english-version).

### Code of Conduct

Participation in this poject is governed by the [Code of Conduct](CODE_OF_CONDUCT.md).

______________________

([English](#english-version))

<a id='french-version' class='anchor' aria-hidden='true'/>

# Soukaoutils
Soukaoutils est une application de partage d’outils. Un utilisateur peut afficher ses outils pour location et préciser le montant à payer par jour.

## OAuth 2.0 et OIDC

Le service de partage est assuré par trois composants :
-	Un serveur d’autorisation
-	Un serveur de ressources disponibles comme service API
-	Un portail client pour gérer les locations 

![Image of OAuth 2.0](https://github.com/VilledeMontreal/soukaoutils/raw/master/OAuth20-MeC.GIF)

Le serveur d’autorisation reconnait les identités de plusieurs fournisseurs. Il est aussi extensible pour accepter n’importe quel autre fournisseur qui implémente le protocole OIDC. Il permet aussi de définir les rôles des utilisateurs et les scopes des applications clientes. Les identités des fournisseurs externes sont associées à une identité locale construite avec l’identité ASP.Net Core.

### Détails

Un utilisateur sera redirigé vers le serveur d’autorisation pour acquérir un code d’autorisation. Le code sera par la suite échangé contre un jeton d’accès incluant les scopes nécessaire pour l’accès à une ressource donnée. Les rôles de l’utilisateur qui a initié l’appel seront aussi ajoutés au jeton d’accès.
Le jeton d’accès est envoyé au serveur de ressource qui en fait l’introspection et valide que le scope est accessible au client et l’utilisateur détient les rôles requis avant de générer la réponse.

### Installer
Copier le dépôt et exécuter les commandes:
```
dotnet restore 
dotnet run
```
Créer et initier la base de données:
```
dotnet ef migrations add initial_migration
dotnet ef database update

```
### Essais
Démarrer les 3 composants:
```
https://localhost:5001/   -- Serveur d'identité unique
https://localhost:5002/   -- Application cliente
https://localhost:6001/   -- Serveur de ressources (Open Data)
```
Commencer par la création d'un nouveau compte utilisateur. Il est possible d'utiliser un compte d'un autre fournisseur OIDC (Google, Azure sont déjà configurés).
Les 3 utilisateurs suivants sont préconfigurés:

`admin@soukaoutils.com`       Rôle *Admin*

`manager@soukaoutils.com`     Rôle *Manager*

`joe@soukaoutils.com`        Pas de rôle en particulier

Utiliser le mot de passe `P@ssw0rd`

### Ajouter un fournisseur d'authentification OIDC
Cette POC démontre aussi comment ajouter un nouveau fournisseur d'identité qui implémente OIDC. Il faut commencer par se procurer une application avec ce fournisseur. Ensuite, injecter le service d'authentification en utilisant le ClientId et le ClientSecret:
```
                .AddOpenIdConnect("oidc", "Identité Ville MTL", options =>
                {
                    options.Authority = <Authority URL>;

                    options.ClientId = <ClientID>;
                    options.ClientSecret = <ClientSecret>; 
                    options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                    // Required scopes
                    options.Scope.Add("profile");
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    ....

```

### Contribuer

Voir [CONTRIBUTING.md](CONTRIBUTING.md#french-version)

### Licence et propriété intellectuelle

Le code source de ce projet est libéré sous la licence [MIT License](LICENSE).

### Code de Conduite

La participation à ce projet est réglementée part le [Code de Conduite](CODE_OF_CONDUCT.md#french-version)

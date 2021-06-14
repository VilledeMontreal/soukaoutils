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

### License

The source code of this project is distributed under the [MIT License](LICENSE).

### Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md#english-version).

### Code of Conduct

Participation in this poject is governed by the [Code of Conduct](CODE_OF_CONDUCT.md).

______________________

([English](#english-version))

<a id='french-version' class='anchor' aria-hidden='true'/>

# Gabarit pour dépôts de code source libre de la Ville de Montréal

## Gabarit pour README.md

Description du projet

### Détails

- Comment fonctionne le produit?
- À qui s'adresse le produit?

### Bâtir

### Installer

### Tester

### Contribuer

Voir [CONTRIBUTING.md](CONTRIBUTING.md#french-version)

### Licence et propriété intellectuelle

Le code source de ce projet est libéré sous la licence [MIT License](LICENSE).

### Code de Conduite

La participation à ce projet est réglementée part le [Code de Conduite](CODE_OF_CONDUCT.md#french-version)

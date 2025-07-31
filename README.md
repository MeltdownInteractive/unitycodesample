# unitycodesample

This simple project shows a UI that allows the user to press a login button to login, and a logout button to logout.

This project uses the Reflex Dependency Injection Framework to inject dependencies into classes that need them.

This project also uses Mail Man - A pooled event dispatcher for Unity that allows game objects to be decoupled from each other, while generating minimal garbage allocations. 

## Key classes/folders

### Configuration
ProjectInstaller.cs - The entry point for the project scope and DI framework initialisation.
All project related configuration can be set here, and RealAuthenticationService or MockAuthenticationService can be easily swapped out

### EventMessaging
xMail.cs - Mail Man generated classes containing event dispatch data
MailSender.cs - A static class used to dispatch Mail Man events

### Models 
A collection of models, enums, and interfaces

### Services
Services used to handle business logic, these services are typically injected into gameObjects that require them.

MockAuthenticationService - A mock class for testing the authentication service
RealAuthenticationService - A real class that makes an API request to authenticate the user

### Repositories
Repositories are used to talk to the data layer

PlayerPrefsRepository - A class that handles all the player prefs keys and reading/writing key data, this could easily be extended to talk to an external database.

### UI
AuthenticationController - A MonoBehaviour used to handle the authentication logic

PanelLoginView - A UI view used to handle the UI interactions of the Login and Logout Buttons, as well as update the UI state.
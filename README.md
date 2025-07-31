# Greg Quinn - Unity Code Sample

This simple project shows a UI that allows the user to press a login button to login, and a logout button to logout.
The project is built around clean architecture principles, with logic, services, dependencies and UI management decoupled from each other.

## Dependencies

### Reflex
A lightweight dependency injection framework for Unity, so we can inject dependencies only into the classes that need them.

### Mail Man
A pooled event dispatcher for Unity that allows game objects to be decoupled from each other, while generating minimal garbage allocations. 

### Rest Client
A Simple HTTP and REST client for Unity based on promises.

### Further improvements

Add Sentry Unity SDK to capture any exceptions or error logs.

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
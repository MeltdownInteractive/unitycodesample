using System;

namespace Models.Interfaces
{
    public interface IAuthenticationService
    {
        void Authenticate(Action<bool, string> callback);
        
        void Logout(Action callback);
    }
}
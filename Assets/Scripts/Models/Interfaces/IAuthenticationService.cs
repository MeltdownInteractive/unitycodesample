using System;

namespace Models.Interfaces
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates the user
        /// </summary>
        /// <param name="callback"></param>
        void Authenticate(Action<bool, string> callback);
        
        /// <summary>
        /// Logs the user out
        /// </summary>
        /// <param name="callback"></param>
        void Logout(Action callback);
    }
}
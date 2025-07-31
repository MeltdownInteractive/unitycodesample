using System;
using Models.Interfaces;

namespace Services
{
    public class MockAuthenticationService : IAuthenticationService 
    {
        public void Authenticate(Action<bool, string> callback)
        {
            callback(true, "MOCK_ACCESS_TOKEN");
        }

        public void Logout(Action callback)
        {
            callback();
        }
    }
}
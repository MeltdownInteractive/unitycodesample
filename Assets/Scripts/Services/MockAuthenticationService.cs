using System;
using EventMessaging;
using Models.Interfaces;

namespace Services
{
    public class MockAuthenticationService : IAuthenticationService 
    {
        public void Authenticate(Action<bool, string> callback)
        {
            MailSender.SendLoginSuccess();
            callback(true, "MOCK_ACCESS_TOKEN");
        }

        public void Logout(Action callback)
        {
            MailSender.SendLogoutSuccess();
            callback();
        }
    }
}
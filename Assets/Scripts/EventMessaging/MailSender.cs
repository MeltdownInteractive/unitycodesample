using BigBenchGames.Tools.MailmanDispatcher;

namespace EventMessaging
{
    public static class MailSender
    {
        public static void SendLoginRequest()
        {
            var mail = Mailman.FetchPooledMail<LoginRequestMail>();
            Mailman.SendMail<LoginRequestMail>(mail);
        }
        
        public static void SendLogoutRequest()
        {
            var mail = Mailman.FetchPooledMail<LogoutRequestMail>();
            Mailman.SendMail<LogoutRequestMail>(mail);
        }
        
        public static void SendLoginSuccess()
        {
            var mail = Mailman.FetchPooledMail<LoginSuccessMail>();
            Mailman.SendMail<LoginSuccessMail>(mail);
        }
        
        public static void SendLogoutSuccess()
        {
            var mail = Mailman.FetchPooledMail<LogoutSuccessMail>();
            Mailman.SendMail<LogoutSuccessMail>(mail);
        }
        
        public static void SendLoginFail()
        {
            var mail = Mailman.FetchPooledMail<LoginFailMail>();
            Mailman.SendMail<LoginFailMail>(mail);
        }
    }
}
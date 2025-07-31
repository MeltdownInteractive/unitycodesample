using BigBenchGames.Tools.MailmanDispatcher;
using EventMessaging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class PanelLoginView : MonoBehaviour
    {
        [SerializeField] private Button _buttonLogin;
        [SerializeField] private Button _buttonLogout;
        [SerializeField] private TMP_Text _textLoginStatus;
    
        void Start()
        {
            Mailman.AddListener<LoginSuccessMail>(OnLoginSuccess);
            Mailman.AddListener<LogoutSuccessMail>(OnLogoutSuccess);
            Mailman.AddListener<LoginFailMail>(OnLoginFail);
        }
    
        void OnDestroy()
        {
            Mailman.RemoveListener<LoginSuccessMail>(OnLoginSuccess);
            Mailman.RemoveListener<LogoutSuccessMail>(OnLogoutSuccess);
            Mailman.RemoveListener<LoginFailMail>(OnLoginFail);
        }

        private void OnLoginSuccess(LoginSuccessMail mail)
        {
            _buttonLogin.interactable = false;
            _buttonLogout.interactable = true;
            _textLoginStatus.text = "Logged in";
        }
    
        private void OnLoginFail(LoginFailMail mail)
        {
            _buttonLogin.interactable = true;
            _buttonLogout.interactable = false;
            _textLoginStatus.text = "Login failed";
        }

        private void OnLogoutSuccess(LogoutSuccessMail mail)
        {
            _buttonLogin.interactable = true;
            _buttonLogout.interactable = false;
            _textLoginStatus.text = "Logged out";
        }

        public void OnLoginButtonPressed()
        {
            MailSender.SendLoginRequest();
        }
    
        public void OnLogoutButtonPressed()
        {
            MailSender.SendLogoutRequest();
        }
    }
}

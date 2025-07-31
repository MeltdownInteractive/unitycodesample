using BigBenchGames.Tools.MailmanDispatcher;
using EventMessaging;
using Models.Interfaces;
using Reflex.Attributes;
using UnityEngine;

namespace UI.Controllers
{
    public class AuthenticationController : MonoBehaviour
    {
        [Inject] private readonly IAuthenticationService _authenticationService;
        [Inject] private readonly IPlayerPrefsRepository _playerPrefsRepository;
        
        private void Start()
        {
            Mailman.AddListener<LoginRequestMail>(OnLoginRequested);
            Mailman.AddListener<LogoutRequestMail>(OnLogoutRequested);
            
            Initialise();
        }
        
        void OnDestroy()
        {
            Mailman.RemoveListener<LoginRequestMail>(OnLoginRequested);
            Mailman.RemoveListener<LogoutRequestMail>(OnLogoutRequested);
        }

        private void Initialise()
        {
            if (_authenticationService == null)
            {
                Debug.LogError("Authentication service is null");
                return;
            }
            
            if (_playerPrefsRepository == null)
            {
                Debug.LogError("Player prefs repository is null");
                return;
            }
            
            Debug.Log("Initialised");
        }
        
        private void OnLoginRequested(LoginRequestMail mail)
        {
            _authenticationService.Authenticate((success, accessToken) =>
            {
                if (success)
                {
                    MailSender.SendLoginSuccess();
                    _playerPrefsRepository.WriteString(_playerPrefsRepository.UserAccessTokenKey, accessToken);
                    Debug.Log("Login successful");
                }
                else
                {
                    MailSender.SendLoginFail();
                    _playerPrefsRepository.WriteString(_playerPrefsRepository.UserAccessTokenKey, "");
                    Debug.LogError("Login failed");
                }
            });
        }
        
        private void OnLogoutRequested(LogoutRequestMail mail)
        {
            MailSender.SendLogoutSuccess();
            Debug.Log("Logout successful");
            _playerPrefsRepository.WriteString(_playerPrefsRepository.UserAccessTokenKey, "");
        }
    }
}
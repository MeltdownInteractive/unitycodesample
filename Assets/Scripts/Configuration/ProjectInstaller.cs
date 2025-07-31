using Models.Enums;
using Models.Interfaces;
using Models.Repositories;
using Reflex.Core;
using Services;
using UnityEngine;

namespace Configuration
{
    public class ProjectInstaller : MonoBehaviour, IInstaller
    {
        [Header("API Configuration")]
        [SerializeField] private ApiEnvironment _apiEnvironment;
        [SerializeField] private string _apiBaseUrlDevelopment = "http://localhost:7136/api";
        [SerializeField] private string _apiBaseUrlTestStaging = "https://af-dream-forge-test-stage.azurewebsites.net/api";
        [SerializeField] private string _apiBaseUrlTest = "https://af-dream-forge-test.azurewebsites.net/api";
        [SerializeField] private string _apiBaseUrlStaging = "https://af-dream-forge-stage.azurewebsites.net/api";
        [SerializeField] private string _apiBaseUrlProduction = "https://api.dreamforge.io/api";
        
        private string _apiBaseUrl;
        
        public void InstallBindings(ContainerBuilder builder)
        {
            _apiBaseUrl = _apiEnvironment switch
            {
                ApiEnvironment.Development => _apiBaseUrlDevelopment,
                ApiEnvironment.TestStaging => _apiBaseUrlTestStaging,
                ApiEnvironment.Test => _apiBaseUrlTest,
                ApiEnvironment.Staging => _apiBaseUrlStaging,
                ApiEnvironment.Production => _apiBaseUrlProduction,
                _ => _apiBaseUrl
            };
            
            // Services
            // var authenticationService = new RealAuthenticationService(_apiBaseUrl);
            var authenticationService = new MockAuthenticationService();
            var playerPrefsRepository = new PlayerPrefsRepository();
            
            builder.AddSingleton(authenticationService, typeof(IAuthenticationService));
            builder.AddSingleton(playerPrefsRepository, typeof(IPlayerPrefsRepository));
            
            // Initialise any default values such as volume, graphics settings etc..
            playerPrefsRepository.Initialize();
        }
    }
}

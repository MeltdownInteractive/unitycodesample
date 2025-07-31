using System;
using EventMessaging;
using Models.Interfaces;
using Models.Requests;
using Models.Responses;
using Proyecto26;
using UnityEngine;

namespace Services
{
    public class RealAuthenticationService : IAuthenticationService
    {
        private readonly string _apiBaseUrl;

        public RealAuthenticationService(string apiBaseUrl)
        {
            _apiBaseUrl = apiBaseUrl;
        }
        
        public void Authenticate(Action<bool, string> callback)
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            
            RestClient.ClearDefaultParams();
            var helper = new RequestHelper
            {
                Uri = _apiBaseUrl + "/authenticateDeviceId",
                Body = new AuthenticateDeviceIdRequest
                {
                    deviceId = deviceId
                }
            };

            RestClient.Post<AuthenticationResponse>(helper)
                .Then(result =>
                {
                    Debug.Log($"--- USER ACCESS TOKEN ---\r\n{result.accessToken}\r\n--- USER ACCESS TOKEN ---");
                    callback(true, result.accessToken.ToString());
                })
                .Catch(err =>
                {
                    Debug.LogError($"AuthenticateDeviceId : Authentication Error : {err.Message}");
                    callback(false, err.Message);
                });
        }

        public void Logout(Action callback)
        {
            callback();
        }
    }
}
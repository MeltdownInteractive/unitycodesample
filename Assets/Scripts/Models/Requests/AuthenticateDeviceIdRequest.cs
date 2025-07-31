using System;

namespace Models.Requests
{
    [Serializable]
    public class AuthenticateDeviceIdRequest
    {
        public string deviceId;

        public override string ToString(){
            return UnityEngine.JsonUtility.ToJson (this, true);
        }
    }
}
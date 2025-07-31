using Models.Interfaces;
using UnityEngine;

namespace Models.Repositories
{
    public class PlayerPrefsRepository : IPlayerPrefsRepository
    {
        public string UserAccessTokenKey => "accessToken";

        // Initialise default values
        public void Initialize()
        {
            if(!KeyExists(UserAccessTokenKey)) WriteString(UserAccessTokenKey, "");
        }
        
        public bool KeyExists(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public string GetString(string key)
        {
            return PlayerPrefs.GetString(key);
        }

        public int GetInt(string key)
        {
            return PlayerPrefs.GetInt(key);
        }

        public float GetFloat(string key)
        {
            return PlayerPrefs.GetFloat(key);
        }

        public void WriteString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
        
        public void WriteInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
        
        public void WriteFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
        
        public void Clear()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
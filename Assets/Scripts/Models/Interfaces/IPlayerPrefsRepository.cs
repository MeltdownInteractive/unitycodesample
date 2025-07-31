namespace Models.Interfaces
{
    public interface IPlayerPrefsRepository
    {
        public string UserAccessTokenKey { get; }
        
        bool KeyExists(string key);
        string GetString(string key);
        int GetInt(string key);
        float GetFloat(string key);
        void WriteString(string key, string value);
        void WriteInt(string key, int value);
        void WriteFloat(string key, float value);
        void Delete(string key);
        void Clear();
    }
}
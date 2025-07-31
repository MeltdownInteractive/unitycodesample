namespace Models.Interfaces
{
    public interface IPlayerPrefsRepository
    {
        /// <summary>
        /// They player prefs key for the user access token
        /// </summary>
        public string UserAccessTokenKey { get; }
        
        /// <summary>
        /// Checks if a key exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool KeyExists(string key);
        
        /// <summary>
        /// Gets the string value of a key 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetString(string key);
        
        /// <summary>
        /// Gets the int value of a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        int GetInt(string key);
        
        /// <summary>
        /// Gets the float value of a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        float GetFloat(string key);
        
        /// <summary>
        /// Writes a string value to a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void WriteString(string key, string value);
        
        /// <summary>
        /// Writes an int value to a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void WriteInt(string key, int value);
        
        /// <summary>
        /// Writes a float value to a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void WriteFloat(string key, float value);
        
        /// <summary>
        /// Deletes a key
        /// </summary>
        /// <param name="key"></param>
        void Delete(string key);
        
        /// <summary>
        /// Clears all keys
        /// </summary>
        void Clear();
    }
}
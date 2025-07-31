using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigBenchGames.Tools.MailmanDispatcher
{
    /// <summary>
    /// An attribute used to store the cached hash of a mail class
    /// </summary>
    public class CachedHashAttribute : System.Attribute
    {
        /// <summary>
        /// The cached hash of a mail class, used for dictionary lookups in mailman
        /// </summary>
        public int CachedHash;

        /// <summary>
        /// The constructor for the CachedHashAttribute class
        /// </summary>
        /// <param name="_cachedHash">The cached hash</param>
        public CachedHashAttribute(int _cachedHash)
        {
            CachedHash = _cachedHash;
        }
    }
}
using System;
using System.Runtime.CompilerServices;

namespace BigBenchGames.Tools.MailmanDispatcher
{
    /// <summary>
    /// The abstract mail class
    /// </summary>
    public abstract class Mail
    {
        /// <summary>
        /// The cached hash of the mail
        /// </summary>
        public abstract int CachedHash { get; }

        /// <summary>
        /// The function used to clean up the mail after it has been used
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// A function used to get the location of the class within the project
        /// </summary>
        /// <returns>The file path to the class</returns>
        public abstract string GetSourcePath();

        /// <summary>
        /// Returns the value of the <see cref="ReadOnlyAttribute"/>
        /// </summary>
        /// <param name="type">The mail class type</param>
        /// <returns>True or false if the mail class is marked as read only</returns>
        public bool GetReadOnlyAttribute(Type type)
        {
            return (Attribute.GetCustomAttribute(type, typeof(ReadOnlyAttribute)) as ReadOnlyAttribute).ReadOnly;
        }

        /// <summary>
        /// Returns the value of the <see cref="VersionAttribute"/>
        /// </summary>
        /// <param name="type">The mail class type</param>
        /// <returns>The version number of the mail class</returns>
        public int GetVersionAttribute(Type type)
        {
            VersionAttribute va = (Attribute.GetCustomAttribute(type, typeof(VersionAttribute)) as VersionAttribute);
            return va != null ? va.Version : -1;
        }
    }
}

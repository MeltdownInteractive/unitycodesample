namespace BigBenchGames.Tools.MailmanDispatcher
{
    /// <summary>
    /// An attribute that marks the version of the mail class
    /// </summary>
    public class VersionAttribute : System.Attribute
    {
        /// <summary>
        /// The version of the mail, used to track mail class version over package updates
        /// </summary>
        public int Version;

        /// <summary>
        /// Constructor for the Version attribute
        /// </summary>
        /// <param name="version">The version of the mail class</param>
        public VersionAttribute(int version)
        {
            Version = version;
        }
    }
}
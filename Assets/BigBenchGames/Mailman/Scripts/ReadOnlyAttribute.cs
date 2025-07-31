namespace BigBenchGames.Tools.MailmanDispatcher
{
    /// <summary>
    /// An attribute that marks a class as read only or not
    /// </summary>
    public class ReadOnlyAttribute : System.Attribute
    {
        /// <summary>
        /// Is the mail class read only or not
        /// </summary>
        public bool ReadOnly;

        /// <summary>
        /// The constructor for the ReadOnly Attribute
        /// </summary>
        /// <param name="readOnly">True or False if the mail class is read only</param>
        public ReadOnlyAttribute(bool readOnly)
        {
            ReadOnly = readOnly;
        }
    }
}
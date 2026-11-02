namespace GCFoundation.Components.Enums
{
    /// <summary>
    /// Defines the roles that categorize an alert and determine its visual style.
    /// </summary>
    public enum AlertType
    {
        /// <summary>
        /// Indicates a danger alert for critical information or an error.
        /// </summary>
        danger,

        /// <summary>
        /// Indicates an informational alert.
        /// </summary>
        info,

        /// <summary>
        /// Indicates a success alert that confirms a successful action or outcome.
        /// </summary>
        success,

        /// <summary>
        /// Indicates a warning alert for information that requires attention.
        /// </summary>
        warning
    }
}
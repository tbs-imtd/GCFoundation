namespace GCFoundation.Components.Enums
{
    /// <summary>
    /// Defines the visual and semantic states available for a modal component.
    /// </summary>
    public enum ModalState
    {
        /// <summary>
        /// Delivers important information or context that users should understand
        /// before proceeding. Highlights details that support decision-making
        /// without implying risk.
        /// </summary>
        info,

        /// <summary>
        /// Used for standard tasks that require user attention or confirmation.
        /// Provides a neutral presentation for everyday interactions.
        /// </summary>
        regular,

        /// <summary>
        /// Alerts users to a high-impact or irreversible action.
        /// </summary>
        warning
    }
}
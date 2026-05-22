using System.Diagnostics.CodeAnalysis;

namespace GCFoundation.Components.Attributes
{
    /// <summary>
    /// Specifies the format for a date property in a class.
    /// This attribute can be applied to properties to indicate how the date should be formatted when displayed or serialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DateFormatAttribute : Attribute
    {
        /// <summary>
        /// Gets the date format string.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
        public string Format { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateFormatAttribute"/> class with the specified date format.
        /// </summary>
        /// <param name="format">The date format string to be applied to the property.</param>
        public DateFormatAttribute([StringSyntax(StringSyntaxAttribute.DateTimeFormat)]string format)
        {
            Format = format;
        }
    }
}

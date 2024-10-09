using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beep.Nugget.Logic
{
    /// <summary>
    /// Represents a definition of a nugget with extended information.
    /// </summary>
    public class NuggetDefinition
    {
        /// <summary>
        /// Gets or sets the nugget's unique identifier.
        /// </summary>
        public string NuggetName { get; set; }

        /// <summary>
        /// Gets or sets the name of the nugget.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the nugget.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category or type of the nugget.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the version of the nugget.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the author or creator of the nugget.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the date when the nugget was created or added.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the nugget is active or deprecated.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets any additional metadata or tags related to the nugget.
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// Indicates whether the nugget is installed in the runtime application.
        /// </summary>
        public bool Installed { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="NuggetDefinition"/> class.
        /// </summary>
        public NuggetDefinition()
        {
            Tags = new List<string>();
            IsActive = true;  // Default value
            CreatedDate = DateTime.UtcNow;  // Automatically set to current date/time
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NuggetDefinition"/> class with specified values.
        /// </summary>
        /// <param name="nuggetName">The nugget's unique identifier.</param>
        /// <param name="name">The name of the nugget.</param>
        /// <param name="description">The description of the nugget.</param>
        /// <param name="category">The category or type of the nugget.</param>
        /// <param name="version">The version of the nugget.</param>
        /// <param name="author">The author or creator of the nugget.</param>
        /// <param name="isActive">Indicates whether the nugget is active or deprecated.</param>
        /// <param name="tags">A list of additional tags or metadata for the nugget.</param>
        public NuggetDefinition(string nuggetName, string name, string description, string category, string version, string author, bool isActive, List<string> tags)
        {
            NuggetName = nuggetName;
            Name = name;
            Description = description;
            Category = category;
            Version = version;
            Author = author;
            IsActive = isActive;
            Tags = tags ?? new List<string>();
            CreatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Provides a string representation of the nugget, useful for debugging or logging.
        /// </summary>
        /// <returns>A string representing the nugget's key information.</returns>
        public override string ToString()
        {
            return $"{NuggetName} ({Name}) v{Version} by {Author} - {Description}";
        }

        /// <summary>
        /// Validates that the nugget has essential properties like name, version, and author.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(NuggetName) &&
                   !string.IsNullOrEmpty(Name) &&
                   !string.IsNullOrEmpty(Version) &&
                   !string.IsNullOrEmpty(Author);
        }
    }
}


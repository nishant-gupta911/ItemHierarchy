using System;
using System.ComponentModel.DataAnnotations;

namespace ItemProcessingSystemCore.Models
{
    /// <summary>
    /// Represents an item in the system with validation attributes
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Unique identifier for the item
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Name of the item with length constraint
        /// </summary>
        [Required(ErrorMessage = "Item name is required")]
        [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Weight of the item in kilograms with range validation
        /// </summary>
        [Required(ErrorMessage = "Weight is required")]
        [Range(0.1, 10000, ErrorMessage = "Weight must be between 0.1 and 10000")]
        public double Weight { get; set; }

        /// <summary>
        /// Timestamp when the item was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
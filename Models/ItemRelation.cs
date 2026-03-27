namespace ItemProcessingSystemCore.Models
{
    /// <summary>
    /// Represents a hierarchical relationship between two items
    /// </summary>
    public class ItemRelation
    {
        /// <summary>
        /// Unique identifier for the relationship record
        /// </summary>
        public int RelationId { get; set; }

        /// <summary>
        /// ID of the parent item in the relationship
        /// </summary>
        public int ParentItemId { get; set; }

        /// <summary>
        /// ID of the child item in the relationship
        /// </summary>
        public int ChildItemId { get; set; }
    }
}
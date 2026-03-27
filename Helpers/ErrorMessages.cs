namespace ItemProcessingSystemCore.Helpers
{
    public static class ErrorMessages
    {
        public const string ItemNameRequired = "Item name is required";
        public const string ItemNameTooLong = "Item name cannot exceed 100 characters";
        public const string ItemWeightRequired = "Weight is required";
        public const string ItemWeightInvalid = "Weight must be between 0.1 and 10000";

        public const string ParentChildSelectionError = "Please select a parent and at least one child item";
        public const string NoChildrenSelected = "Please select at least one child item";
        public const string SameParentChildError = "Cannot select the same item as both parent and child";
        public const string DuplicateRelationError = "A relation between these items already exists";
        public const string CircularDependencyError = "Creating this relation would result in a circular dependency";

        public const string ItemCreateError = "An error occurred while creating the item. Please try again";
        public const string ItemUpdateError = "An error occurred while updating the item. Please try again";
        public const string ItemDeleteError = "An error occurred while deleting the item. Please try again";
        public const string RelationCreateError = "An error occurred while creating the relation. Please try again";
        public const string ItemNotFoundError = "Item not found"; 

        public const string ItemCreatedSuccess = "Item created successfully!";
        public const string ItemUpdatedSuccess = "Item updated successfully!";
        public const string ItemDeletedSuccess = "Item deleted successfully!";
        public const string RelationsLinkedSuccess = "Successfully linked child item(s) to parent";

        public const string NoItemsFound = "No items found in the database";
        public const string FailedToLoadItem = "Failed to load item from database";
        public const string FailedToLoadRelations = "Failed to load relations from database";
    }
}

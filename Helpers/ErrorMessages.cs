namespace ItemProcessingSystemCore.Helpers
{
    public static class ErrorMessages
    {
        // validation errors
        public const string ItemNameRequired = "Item name is required";
        public const string ItemWeightRequired = "Weight is required";
        public const string ItemWeightInvalid = "Weight must be between 0.1 and 10000";

        // relation errors
        public const string DuplicateRelationError = "This relation already exists";
        public const string CircularDependencyError = "This would create a circular dependency";
        public const string NoChildrenSelected = "Please select at least one child";

        // success messages
        public const string ItemCreatedSuccess = "Item created!";
        public const string ItemUpdatedSuccess = "Item updated!";
        public const string ItemDeletedSuccess = "Item deleted!";
    }
}

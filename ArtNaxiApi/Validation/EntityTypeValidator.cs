using ArtNaxiApi.Constants;

namespace ArtNaxiApi.Validation
{
    public static class EntityTypeValidator
    {
        private static readonly HashSet<string> ValidEntities = new HashSet<string>
        {
            EntityTypes.Image
        };

        public static bool IsValidEntity(string entityType)
        {
            return ValidEntities.Contains(entityType);
        }
    }
}

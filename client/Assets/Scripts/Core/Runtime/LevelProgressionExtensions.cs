namespace Core.Runtime
{
    public static class LevelProgressionExtensions
    {
        public static bool CanPlayLevel(this LevelProgression progression, int levelId)
        {
            return levelId <= progression.CurrentLevel;
        }
    }
}
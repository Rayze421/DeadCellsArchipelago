using dc.achievements;

namespace DeadCellsArchipelago {
    public static class AchievementManager
    {
        public static bool RemoveShouldDisplayInGameNotification(Hook_SteamAchievementManager.orig_shouldDisplayInGameNotification orig, SteamAchievementManager self)
        {
            //disable achievement pop up
            return false;
        }

        public static void RemoveUnlock(Hook_SteamAchievementManager.orig_unlock orig, SteamAchievementManager self, EAchievement achievement)
        {
            //remove steam achievement
        }

        public static bool RemoveIsUnlocked(Hook_SteamAchievementManager.orig_isUnlocked orig, SteamAchievementManager self, EAchievement achievement)
        {
            //say that the player doesn't have achievements
            return false;
        }
    }
 }
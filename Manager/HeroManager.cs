using static DeadCellsArchipelago.ItemManager;
using static DeadCellsArchipelago.RoomManager;
using static DeadCellsArchipelago.ItemQueue;
using dc.en;
using Serilog;

namespace DeadCellsArchipelago {
    public static class HeroManager
    {
        public static void OnHeroDie(Hook_Hero.orig_onDie orig, Hero self)
        {
            lastLevel = null;
            heroJustDead = true;
            Log.Warning("=== It's a death ==="); //test for reset and complete run (I want to do it on reset but not on complete), and what happend when he quit
            orig(self);
            heroJustDead = false;
            aspectsToIter = 0;
        }

        public static void OnHeroInit(Hook_Hero.orig_init orig, Hero self)
        {
            orig(self);
            HERO = self;
            
            Log.Information("=== Hero initialized ! ===");
        }
    }
}
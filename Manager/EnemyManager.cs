using dc.en;
using HaxeProxy.Runtime;
using ModCore.Utilities;
using static DeadCellsArchipelago.ItemManager;

namespace DeadCellsArchipelago {
    public static class EnemyManager
    {
        public static void SpawnMobOnPlayer(string id, bool elite)
        {
            if (HERO != null)
            {
                int lifeTier = HERO._level.map.mobLifeTier;
                Mob mob = Mob.Class.create(id.AsHaxeString(), HERO._level, HERO.cx, HERO.cy, HERO._level.map.mobDmgTier, new Ref<int>(ref lifeTier));
                if (elite)
                {
                    mob.setElite(false);
                }
                double reductionValue = 0;
                mob.setAffectS(8, 1, new Ref<double>(ref reductionValue),true);
            }
        }
    }
}
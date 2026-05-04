using dc.en;
using HaxeProxy.Runtime;
using ModCore.Utilities;
using static DeadCellsArchipelago.ItemManager;
using static DeadCellsArchipelago.RuneManager;

namespace DeadCellsArchipelago {
    public static class EnemyManager
    {
        public static void SpawnMobOnPlayer(string id, bool elite, bool anger)
        {
            if (HERO != null)
            {
                int lifeTier = HERO._level.map.mobLifeTier;
                Mob mob = Mob.Class.create(id.AsHaxeString(), HERO._level, HERO.cx, HERO.cy, HERO._level.map.mobDmgTier, new Ref<int>(ref lifeTier));
                if (elite)
                {
                    mob.setElite(false);
                }
                if (anger)
                {
                    bool bump = false;
                    mob.eliteAnger(new Ref<bool>(ref bump));
                }
                double reductionValue = 0;
                mob.setAffectS(8, 1, new Ref<double>(ref reductionValue), true);
            }
        }

        public static void EliteTrap()
        {
            List<List<string>> teams = [
                ["Demon", "Curser"],
                ["FlyingShooter", "FatZombie"],
                ["Golem", "Defender"],
                ["Hammer", "Necromant"],
                ["Shielder", "Comboter", "Shocker"],
                ["PirateChief", "Harpy"],
                ["Hurler", "Fogger", "ClusterGrenader"],
            ];
            List<string> selectedTeam = teams[new Random().Next(0, teams.Count)];
            foreach (string mob in selectedTeam)
            {
                SpawnMobOnPlayer(mob, true, true);
            }
        }

        public static void OnGenerateLootOnMobs(dc.level.Hook_LootGen.orig_generateLootOnMobs orig, dc.level.LootGen self)
        {
            useOriginalHasPermanentItem = false;
            orig(self);
            useOriginalHasPermanentItem = true;
        }
    }
}
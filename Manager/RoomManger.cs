using dc.level.@struct;
using static DeadCellsArchipelago.RuneManager;

using static DeadCellsArchipelago.ItemManager;
using dc.en.inter.door;
using dc.en;
using dc.en.inter;
using HaxeProxy.Runtime;

namespace DeadCellsArchipelago {
    public static class RoomManager
    {
        public static void InitializeRoomHooks()
        {
            Hook_PrisonCourtyard.buildMainRooms += (orig, self) => { useOriginalHasPermanentItem=false; var res=orig(self); useOriginalHasPermanentItem=true; return res;};
            Hook_PrisonRoof.buildMainRooms += (orig, self) => { useOriginalHasPermanentItem=false; var res=orig(self); useOriginalHasPermanentItem=true; return res;};
            Hook_Crypt.finalize += (orig, self) => { useOriginalHasPermanentItem=false;orig(self); useOriginalHasPermanentItem=true; };
            Hook_SewerShort.buildSecondaryRooms += (orig, self) => { useOriginalHasPermanentItem=false; orig(self); useOriginalHasPermanentItem=true; };
            Hook_AncientTemple.buildSecondaryRooms += (orig, self) => { useOriginalHasPermanentItem=false; orig(self); useOriginalHasPermanentItem=true; };
            Hook_Ossuary.buildMainRooms += (orig, self) => { useOriginalHasPermanentItem=false; var res=orig(self); useOriginalHasPermanentItem=true; return res;};
            Hook_Ossuary.buildSecondaryRooms += (orig, self) => { useOriginalHasPermanentItem=false; orig(self); useOriginalHasPermanentItem=true; };
        }

        public static void OnTriggeredDoorActivate(Hook_TriggeredDoor.orig_onActivate orig, TriggeredDoor self, Hero by, bool lp)
        {
            if(USER != null)
            {   //allow the player to open the mutation door in collector's transition
                if (USER.game.curLevel.map.getRoomAt(self.cx, self.cy).rTemplate.ToString() == "PerkShop")
                {
                    self.openFast(self.cx - by.cx >= 0 ? 1 : -1, null);
                    return;
                }
            }
            orig(self, by, lp);
        }

        public static void OnDoorCloseFast(Hook_Door.orig_closeFast orig, Door self, HlAction cb)
        {   //without this, the mutation door in collector's transition will close automatically
            if(USER != null)
            {
                if (USER.game.curLevel.map.getRoomAt(self.cx, self.cy).rTemplate.ToString() == "PerkShop")
                {
                    return;
                }
            }
            orig(self, cb);
        }
    }
}
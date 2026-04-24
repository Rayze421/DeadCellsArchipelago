using dc.pow;

using static DeadCellsArchipelago.ItemManager;

namespace DeadCellsArchipelago {
    public static class PokeManager
    {
        public static void OnRemoveItem(Hook_Pokecharge.orig_removeItem orig, Pokecharge self)
        {
            orig(self);
            DropItemToPlayer("Pokebomb");
        }
    }
}
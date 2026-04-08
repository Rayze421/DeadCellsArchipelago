using Serilog;

using static DeadCellsArchipelago.ItemManager;
using static DeadCellsArchipelago.Translator;

namespace DeadCellsArchipelago
{
    public static class ItemQueue
    {
        private static List<string> pendingItems = [];

        public static void AddItemToQueue(string itemName)
        {
            Log.Information($"=== Item reçu d'Archipelago: {itemName} ===");
            pendingItems.Add(itemName);
        }

        public static void GiveItemInQueue()
        {
            if(IsQueueEmpty()) { return; }
            if(SAVED_DATA == null) { return; }

            var itemName = pendingItems[0];

            if (NameToIdKeyExist(itemName))
            {
                itemName = GetId(itemName);
            }

            //if not prog, check classic list OR prog, check prog list
            if((!IsItemProgressive(itemName) && !SAVED_DATA.IsItemRecieved(itemName)) ||
                (IsItemProgressive(itemName) && !SAVED_DATA.IsProgressionItemRecieved(itemName)))
            {
                if (GiveItemFromArchipelago(itemName))
                {
                    SAVED_DATA.SaveItemRecieved(itemName);
                }
                Log.Information($"=== Item {itemName} given ===");
            }
            else
            {
                Log.Error($"=== Item {itemName} already given ===");
            }
            pendingItems.RemoveAt(0);
        }

        public static bool IsQueueEmpty()
        {
            return pendingItems.Count == 0;
        }
    }
}
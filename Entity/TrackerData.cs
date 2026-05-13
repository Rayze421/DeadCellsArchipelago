using System.Text.Json;
using static DeadCellsArchipelago.ItemManager;

namespace DeadCellsArchipelago
{
    public static class TrackerData
    {
        public static Dictionary<string, List<string>> StartCalculate()
        {
            Dictionary<string, Entry>? data = JsonSerializer.Deserialize<Dictionary<string, Entry>>(GetTrackerDataFilePath());
            return CalculateTraker(data);
        }

        public static Dictionary<string, List<string>> CalculateTraker(Dictionary<string, Entry>? data)
            {
                Dictionary<string, List<string>> res = [];

                if (SAVED_DATA == null || data == null) return res;

                res["all"] = new List<string>();
                foreach (KeyValuePair<string, Entry> entry in data)
                {
                    string key = "";
                    bool added = false;
                    
                    if(CanGoDLC(entry.Value.dlc) && CanCosmetics(entry.Value.type))
                    {
                        if (entry.Value.type == "aspect")
                        {
                            key = "Aspect";
                            IncToDict(ref res, key, entry.Key);
                            added = true;
                        }
                        else
                        {
                            foreach (Source source in entry.Value.sources)
                            {
                                if(CanGoDLC(source.dlc))
                                {
                                    for (int i = source.min_bc; i <= Math.Min(source.max_bc, SAVED_DATA.bscLevelToWin); i++)
                                    {
                                        key = source.biome + i;
                                        IncToDict(ref res, key, entry.Key);
                                        added = true;
                                    }
                                }
                            }
                        }
                        if(added) res["all"].Add(entry.Key);
                    }
                }
                return res;
            }

            public static bool CanGoDLC(string dlc)
            {
                if (ARCHIPELAGO == null) return false;
                switch (dlc)
                {
                    case "":
                        return true;
                    case "RiseOfTheGiant":
                        return ARCHIPELAGO.riseOfTheGiant;
                    case "TheBadSeed":
                        return ARCHIPELAGO.theBadSeed;
                    case "FatalFalls":
                        return ARCHIPELAGO.fatalFalls;
                    case "TheQueenAndTheSea":
                        return ARCHIPELAGO.theQueenAndTheSea;
                    case "Purple":
                        return ARCHIPELAGO.returnToCastlevania;
                }
                return false;
            }

            public static bool CanCosmetics(string type)
            {
                if (ARCHIPELAGO == null) return false;
                if (type == "skin" || type == "head") return ARCHIPELAGO.includeCosmetics;
                return true;
            }

            public static void IncToDict(ref Dictionary<string, List<string>> res, string key, string check)
            {
                if (key != "" && !res.ContainsKey(key))
                {
                    res[key] = new List<string>();
                }
                res[key].Add(check);
            }

            public static string GetTrackerDataFilePath()
            {
                return Path.Combine(AppContext.BaseDirectory, "..", "..", "mods", "DeadCellsArchipelago", "trackerData.json");
            }
    }
        public class Source
        {
            public string biome { get; set; } = "";
            public int min_bc { get; set; }
            public int max_bc { get; set; }
            public string dlc { get; set; } = "";
            public string? mob { get; set; }
        }

        public class Entry
        {
            public string type { get; set; } = "";
            public string dlc { get; set; } = "";
            public string? rarity { get; set; }
            public List<Source> sources { get; set; } = [];
        }
}
"""
Dead Cells - Archipelago World
rules.py

Location-level access rules that go beyond region entrance conditions.
These are applied after create_regions() in __init__.py via set_rules().

Rule categories:
  - BSC level requirements (based on ProgBossRune count)
  - Item requirements (runes, AP items in inventory)
  - Boss kill requirements (specific location checks completed)
  - Skin count requirements (number of Skin-category items received)
  - Biome exit requirements (one of several biome_exit checks reached)
  - Combined conditions (AND / OR)

Adding new rules:
  Append entries to LOCATION_RULES below. Each entry is a dict:
    {
        "location": "ExactLocationName",   # from LOCATION_TABLE
        "requires": { ... }                # see RuleType constants below
    }
  Then re-run set_rules() — no other file needs changing.
"""

from typing import TYPE_CHECKING, List
from worlds.generic.Rules import add_rule

if TYPE_CHECKING:
    from . import DeadCellsWorld


# ─────────────────────────────────────────────────────────────────────────────
# Skin item names (used for count-based rules)
# ─────────────────────────────────────────────────────────────────────────────
SKIN_ITEMS = [
    "PrisonerGold", "PrisonerSonGoku", "PrisonerBlack", "PrisonerGhost",
    "PrisonerSewers", "PrisonerSanta", "PrisonerStilt", "PrisonerSkeleton",
    "PrisonerCarduus", "PrisonerAphrodite", "PrisonerShaman", "PrisonerCloud",
    "PrisonerHyperlight", "PrisonerAladdin", "PrisonerBison", "PrisonerWarrior",
    "PrisonerMage", "PrisonerNeon", "PrisonerBobby", "PrisonerDemon",
    "PrisonerSylvanian", "PrisonerSand", "PrisonerGOG", "PrisonerFrench",
    "PrisonerXmas", "PrisonerGardener", "PrisonerFriendlyHardy", "PrisonerMushroom",
    "PrisonerFugitive", "PrisonerBlowgunner", "PrisonerTick", "RoyalGardener",
    "PrisonerRetro", "FreemanSkin", "PrisonerJavelinSnake", "PrisonerNecromant",
    "PrisonerBootleg", "PrisonerKamikaze", "PrisonerCrossbowMan", "PrisonerKillBill",
    "KingDefault", "KingWhite",
    "BehemothDefault", "Behemoth1", "Behemoth2", "Behemoth3", "Behemoth4",
    "BeholderDefault", "Beholder1", "Beholder2", "Beholder3", "Beholder4",
    "AssassinDefault", "Assassin1", "Assassin2", "Assassin3", "Assassin4",
    "GiantDefault", "Giant1", "Giant2", "Giant3", "Giant4",
    "HotkDefault", "Hotk1", "Hotk2", "Hotk3", "Hotk4",
    "Statue", "CollectorDefault",
    "TickDefault", "Tick1", "Tick2", "Tick3", "Tick4", "TickSacrifice",
    "GardenerDefault", "Gardener1", "Gardener2", "Gardener3", "Gardener4", "Cultist",
    "FlawlessBehemoth", "FlawlessBeholder", "FlawlessAssassin", "FlawlessGiant",
    "FlawlessHotk", "FlawlessTick", "FlawlessGardener",
    "Snowman", "SantaKLOS",
    "HollowKnight", "Blasphemous", "Guacamelee", "Skul", "HyperLightDrifter",
    "CurseofTheDeadGods", "SoulKnight",
    "Staphy", "ANCHORGUY",
    "ServanteDefault", "Servante1", "Servante2", "Servante3", "Servante4",
    "FlawlessServante", "QueenDefault", "Queen1", "Queen2", "Queen3", "Queen4",
    "FlawlessQueen", "BlueErinaceus",
    "Banker", "GuillainThief", "Bobby",
    "ShovelKnight", "HotlineMiamiChicken", "KatanaZero", "RiskOfRain",
    "Terraria", "SlayTheSpire",
    "Mentor", "Mentor1", "Mentor2", "Mentor3",
    "VictoriusBeheaded", "VictoriusBeheaded1", "VictoriusBeheaded2", "VictoriusBeheaded3",
    "Simon", "Alucard", "RichterROB", "Sypha", "Trevor", "Maria", "Hector",
    "HauntedArmor",
    "AdeleDefault", "Adele1", "Adele2", "Adele3", "Adele4", "FlawlessAdele",
    "DookuDefault", "Dooku1", "Dooku2", "Dooku3", "Dooku4", "FlawlessDooku",
]



# ─────────────────────────────────────────────────────────────────────────────
# Location rules table
#
# Each entry: ("location_name", rule_lambda_factory)
# The factory receives (world) and returns a state -> bool lambda.
#
# Helpers at the bottom of this file build the lambdas cleanly.
# ─────────────────────────────────────────────────────────────────────────────

def _has(item):
    """Rule: player has item in inventory."""
    return lambda world: (
        lambda state: state.has(item, world.player)
    )

def _has_all(*items):
    return lambda world: (
        lambda state: state.has_all(items, world.player)
    )

def _has_any(*items):
    """Rule: player has ANY of the listed items."""
    return lambda world: (
        lambda state: any(state.has(i, world.player) for i in items)
    )

def _has_all_any(all_items, any_of=()):
    flat_items = tuple(
        item for sub in all_items
        for item in (sub if isinstance(sub, (list, tuple)) else (sub,))
    )

    return lambda world: (
        lambda state:
            state.has_all(flat_items, world.player)
            and (not any_of or any(state.has(i, world.player) for i in any_of))
    )

def _bsc(level: int):
    return lambda world: (
        lambda state: get_bc_level(state, world.player) >= level
    )

def _has_and_bsc(item: str, level: int):
    return lambda world: (
        lambda state:
            state.has(item, world.player)
            and get_bc_level(state, world.player) >= level
    )

def _skin_count(count: int):
    """Rule: player has received at least `count` Skin-category items."""
    return lambda world: (
        lambda state: sum(
            1 for skin in SKIN_ITEMS
            if state.has(skin, world.player)
        ) >= count
    )

def _boss_killed(location: str):
    """Rule: specific boss location has been checked."""
    return lambda world: (
        lambda state: state.can_reach_location(location, world.player)
    )

def _any_biome_exit(*biomes):
    """Rule: player has completed the biome_exit check of at least one biome."""
    return lambda world: (
        lambda state: any(
            state.can_reach_location(f"{b}_Exit", world.player)
            for b in biomes
        )
    )

def _has_and_boss(item: str, boss_loc: str):
    """Rule: has item AND boss kill completed."""
    return lambda world: (
        lambda state: (
            state.has(item, world.player)
            and state.can_reach_location(boss_loc, world.player)
        )
    )

def get_bc_level(state, player):
    return state.count("ProgBossRune", player)

def set_rules(world):
    player = world.player

    for loc_name, loc_data in LOCATION_TABLE.items():
        location = world.multiworld.get_location(loc_name, player)

        sources = loc_data.get("sources", [])

        def rule(state, sources=sources):
            bc = get_bc_level(state, player)

            return any(
                state.can_reach(s["biome"], "Region", player)
                and s["min_bc"] <= bc <= s["max_bc"]
                for s in sources
            )

        add_rule(location, rule)

# ─────────────────────────────────────────────────────────────────────────────
# The main rules table
# Format: (location_name, rule_factory)
# ─────────────────────────────────────────────────────────────────────────────
LOCATION_RULES = [

    # ── Gameplay items ────────────────────────────────────────────────────────
    ("Blueprint_P_Disengage", _has("HomKey")),
    ("Money5",  _has("HomKey")),
    
    # P_Health rule is applied on the enemy check below, not on P_Wishes
    # P_Health is a Perk dropped by Lancer (Castle biome), not a floor blueprint
    # Rule applied via enemy check: Castle_Blueprint_P_Health_from_Lancer

    ("Blueprint_SpeedBlade",    _has("ScoringKey")),
    ("Blueprint_DamageAura",    _has("ScoringKey")),
    ("Blueprint_DashSword",     _has("ScoringKey")),

    # MultiKickBoots requires 1+ BSC (dropped by Ninja in PrisonCourtyard)


    # ── LighthouseKey ─────────────────────────────────────────────────────────
    # LighthouseKey only appears in StiltVillage if player visited SewerShort
    ("Blueprint_LighthouseKey", lambda world: (
        lambda state: state.can_reach("SewerShort", "Region", world.player)
    )),
    # ── PrisonCourtyard mid-biome gate (LadderKey) ────────────────────────────
    # These items only drop from enemies in the locked zone of PrisonCourtyard
    ("Blueprint_BackBlink", _has("LadderKey")),
    ("Blueprint_BumpBoots", _has("LadderKey")),
    ("Blueprint_DamageBuff", _has("LadderKey")),
    ("Blueprint_Decoy", _has("LadderKey")),
    ("Blueprint_ExplosiveGrenade", _has("LadderKey")),
    ("Blueprint_GroundSaw", _has("LadderKey")),
    ("Blueprint_KnivesCircle", _has("LadderKey")),
    ("Blueprint_LowHealth", _has("LadderKey")),
    ("Blueprint_MultiCrossBow", _has("LadderKey")),
    ("Blueprint_MultiKickBoots", _has("LadderKey")),
    ("Blueprint_OilSword", _has("LadderKey")),
    ("Blueprint_P_DeathShield", _has("LadderKey")),
    ("Blueprint_PrisonerAphrodite", _has("LadderKey")),
    ("Blueprint_PrisonerBlack", _has("LadderKey")),
    ("Blueprint_PrisonerKamikaze", _has("LadderKey")),
    ("Blueprint_PrisonerNeon", _has("LadderKey")),
    ("Blueprint_PrisonerWarrior", _has("LadderKey")),
    ("Blueprint_Shockwave", _has("LadderKey")),
    
    ("Blueprint_LongBow", _has("LadderKey")),
    ("Blueprint_ExplosiveCrossBow",
      _has_all_any(
     ["LadderKey", "BreakableGroundKey"], any_of=["WallJumpKey", "HomKey"]
 )),
    ("PrisonCourtyard_Exit", _has("LadderKey")),
    # ── Lighthouse access (no LighthouseKey item yet) ─────────────────────────
    # Requires having visited SewerShort AND StiltVillage
    # More precise: SewerShort biome_enter AND StiltVillage biome_enter

    # ── Boss skin series (BSC gates) ──────────────────────────────────────────

    # ── Prisoner skin BSC gates ───────────────────────────────────────────────

    # ── Collab skins (item requirements) ─────────────────────────────────────
    ("Blueprint_HollowKnight", _has("PureNail")),
    ("Blueprint_Blasphemous", _has("FaceFlask")),
    # Guacamelee: requires visiting Crypt + having a kick-type boot
    # Boots (MultiKickBoots etc.) are useful items — promote MultiKickBoots to PROG
    # and simplify rule to just require Crypt + MultiKickBoots
    ("Blueprint_Guacamelee", lambda world: (
        lambda state: (
            state.can_reach("Crypt", "Region", world.player)
            and state.has("MultiKickBoots", world.player)
        )
    )),
    ("Blueprint_HyperLightDrifter",
        _has_all("HardLightSword", "PrisonerHyperlight")),
    ("Blueprint_CurseofTheDeadGods", lambda world: (
        lambda state: any(
            state.can_reach_location(loc, world.player)
            for loc in ["Throne_Exit", "QueenArena_Exit", "DookuArena_Exit"]
        )
    )),
    # Terraria: requires BackpackUnlock — rule disabled for now to avoid fill errors
    # TODO: re-enable once BackpackUnlock is confirmed as progression item
    ("Blueprint_Terraria",
    lambda world: (
        lambda state:
             state.has("BackpackUnlock", world.player) and
             state.can_reach_location("Boss_KingsHand", world.player)
 )
),
    ("Blueprint_HotlineMiamiChicken", _has("BaseballBat")),
    ("Blueprint_KatanaZero", _has("Katana")),
    ("Blueprint_ShovelKnight", _has("Shovel")),

    # ── Scissor / Comb (skin count gates) ────────────────────────────────────
    # Scissor/Comb: skin count gates disabled for now
    # AP cannot guarantee enough skins are reachable before these locations
    # TODO: re-enable with a more sophisticated count that respects BC/DLC filters
    # ("Blueprint_Scissor",   _skin_count(16)),
    # ("Blueprint_Comb",      _skin_count(51)),

    # ── KingDefault / KingWhite (boss kill chain) ─────────────────────────────
    ("Blueprint_KingDefault", _boss_killed("Boss_Collector")),
    ("Blueprint_KingWhite", _has("KingDefault")),

    # ── TickSacrifice ─────────────────────────────────────────────────────────
    ("Blueprint_TickSacrifice", _has("SpawnFriendlyHardy")),
    
    # ── PokebombUnlock ────────────────────────────────────────────────────────
    ("Blueprint_PrisonerGold", _has("PokebombUnlock")),

    # ── TODO: Cemetery -> Cavern rule (KingsHand vs bsc1 + HomKey) ────────────
    # Currently KingsHand boss kill is used in transition.json.
    # Needs in-game testing to confirm if HomKey is also required.
    # Uncomment and adjust once confirmed:
    # TODO: Cemetery->Cavern may also require HomKey — needs in-game testing
    
    # ── Heads ────────────────────────────────────────────────────────────────
    # TODO: (red/blue/green black hole heads) && (VortexFoundry, Pecheur, GlitchyHeadDeepSpace) && Flawless
    ("Item_StaphyHead", _has("SpawnLilStaphy")),
    ("Item_MushroomBoi", _has("SpawnFriendlyHardy")),
    ("Item_BlobbyFlameMagma", _boss_killed("Boss_KingsHand")),
    ("Item_BlowTorchRed", _has("FlameThrower")),
    ("Item_BossCellHead", _has("ProgBossRune:5")),
]



# ─────────────────────────────────────────────────────────────────────────────
# Grouped check OR rules
# Applied in set_rules() — each Blueprint_X check is accessible if the player
# can reach ANY of the biomes where that item drops, at the right BC level.
# ─────────────────────────────────────────────────────────────────────────────

def _build_grouped_rules(world):
    if not hasattr(world, "grouped_location_sources"):
        return
    player = world.player
    multiworld = world.multiworld

    enabled_dlcs = set()

    if world.options.dlc_rise_of_the_giant.value:
        enabled_dlcs.add("RiseOfTheGiant")

    if world.options.dlc_the_bad_seed.value:
        enabled_dlcs.add("TheBadSeed")

    if world.options.dlc_fatal_falls.value:
        enabled_dlcs.add("FatalFalls")

    if world.options.dlc_the_queen_and_the_sea.value:
        enabled_dlcs.add("TheQueenAndTheSea")

    if world.options.dlc_return_to_castlevania.value:
        enabled_dlcs.add("Purple")

    existing_regions = {r.name for r in multiworld.regions if r.player == player}

    for location, sources in world.grouped_location_sources.items():
        # Filter valid sources once
        valid_sources = [
            s for s in sources
            if s["biome"] in existing_regions
        ]

        def make_rule(source_data):
            def rule(state):
                bc = get_bc_level(state, player)

                return any(
                    (s["dlc"] == "" or s["dlc"] in enabled_dlcs)
                    and s["min_bc"] <= bc <= s["max_bc"]
                    and state.can_reach(s["biome"], "Region", player)
                    for s in source_data
                )
            return rule
        try:
            loc_obj = multiworld.get_location(location, player)
        except KeyError:
            print(f"[GroupedRules] Missing location: {location}")
            continue

        biomes = {s["biome"] for s in valid_sources}

# ─────────────────────────────────────────────────────────────────────────────
# Main entry point — called from __init__.py set_rules()
# ─────────────────────────────────────────────────────────────────────────────
def set_rules(world: "DeadCellsWorld") -> None:
    """
    Apply all location-level rules to the multiworld.
    Skips rules for locations that don't exist in this world
    (e.g. DLC disabled, BC out of range).
    """
    multiworld = world.multiworld
    player = world.player

    # Build OR access rules for all grouped checks
    _build_grouped_rules(world)

    # Deduplicate: if a location appears multiple times, AND the rules together
    from collections import defaultdict
    rule_map: dict = defaultdict(list)
    for loc_name, rule_factory in LOCATION_RULES:
        rule_map[loc_name].append(rule_factory(world))

    for loc_name, rules in rule_map.items():
        try:
            location = multiworld.get_location(loc_name, player)
        except KeyError:
            continue

        add_rule(location, lambda state, rs=rules: all(r(state) for r in rs))
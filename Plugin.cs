using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace TMS_Plugin;

[BepInPlugin("de.kraisie.TMS_Cheats", "TMS Cheats", "0.0.1")]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        // Plugin startup logic
        Harmony.CreateAndPatchAll(typeof(CookingMods));
        Harmony.CreateAndPatchAll(typeof(CleaningMods));
        Harmony.CreateAndPatchAll(typeof(ResourceMods));
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    /// <summary>
    /// Mods that change the cooking behaviour.
    /// </summary>
    private static class CookingMods
    {
        // not going to change minigame stuff but if you want to do that maybe take a look at Food#setQuality

        /// <summary>
        /// Sets the value for the fire duration to its max time on each update (each frame).
        /// </summary>
        /// <param name="__instance">The instance of the class.</param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FuelController), nameof(FuelController.Update))]
        private static void InfiniteFire(ref FuelController __instance)
        {
            __instance.curTime = __instance.maxTime;
        }

        /// <summary>
        /// Returns true on the check if there are enough resources to cook a recipe. Thus enabling cooking without
        /// having the ingredients in the inventory. Also works for putting cheese or sausage onto the shelf, and for
        /// filling mugs/trinkets with ale or wine.
        /// </summary>
        /// <param name="__result">The original return value of the HaveResources method of the game.</param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StationController), nameof(StationController.HaveResources))]
        private static void AlwaysHasResources(ref bool __result /*, int count*/)
        {
            __result = true;
        }
    }

    /// <summary>
    /// Mods that change the cleaning behaviour.
    /// </summary>
    private static class CleaningMods
    {
        /// <summary>
        /// Fills the water level in the washer to its max level to have infinite water.
        /// </summary>
        /// <param name="__instance">The instance of the washer.</param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Washer), nameof(Washer.Start))]
        [HarmonyPatch(typeof(Washer), nameof(Washer.CanServantUse))]
        private static void InfiniteWater(ref Washer __instance)
        {
            __instance.waterCounter.currentLevel = __instance.waterCounter.maxLevel;
            __instance.waterGui.Update();
        }

        // TODO: DirtManager.objectList to get list of DirtObject -> OnWipe or OnDestroy
    }

    /// <summary>
    /// Mods that are related to resources and their consumption
    /// </summary>
    private static class ResourceMods
    {
        /// <summary>
        /// Fills the FairyHouse feeder slots when a fairy eats something.
        /// </summary>
        /// <param name="__instance">The instance of the ServantTaskFeed.</param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ServantTaskFeed), nameof(ServantTaskFeed.Grab))]
        private static void AutoRefillFairyFood(ref ServantTaskFeed __instance)
        {
            var servantPersonality = __instance.owner.person;
            servantPersonality.feederFill = 3;
        }

        /// <summary>
        /// Always have chopped wood in the woodpile.
        /// </summary>
        /// <param name="__instance">The Woodpile instance.</param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Woodpile), nameof(Woodpile.Start))]
        [HarmonyPatch(typeof(Woodpile), nameof(Woodpile.Activate))]
        private static void InfiniteChoppedWood(ref Woodpile __instance)
        {
            __instance.counter.currentLevel = 10; // above 6 there are no visual changes anymore
        }

        /// <summary>
        /// Always have water and horse food in the trough
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TavHorse), nameof(TavHorse.Feed))]
        private static void InfiniteHorseFeed(ref TavHorse __instance)
        {
            // one quest has filling food as a task, which is not possible as the max level for food is 5.
            // to do this task either disable the mod or set it to a value smaller than 5.
            __instance.feeder.water.counter.currentLevel = 
                __instance.feeder.water.counter.maxLevel != -1 ? __instance.feeder.water.counter.maxLevel : 100; // maxLevel is -1 on the big one and 5 on the small one
            __instance.feeder.food.counter.currentLevel = __instance.feeder.food.counter.maxLevel;
        }

        /// <summary>
        /// Instead of 25 seconds you have 10 minutes to catch a thief. Thieves also get a speech bubble like food
        /// critics so they are harder to miss.
        /// </summary>
        /// <param name="__result">The return value of the original method.</param>
        /// <param name="__instance">The BehThief instance.</param>
        /// <param name="owner">The Customer object that represents the thief.</param>
        /// <returns>Changes the return value of the original method to true. Returns false in order to skip the original method.</returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehThief), nameof(BehThief.OnLanded))]
        private static bool SlowThieves(ref bool __result, ref BehThief __instance, Customer owner)
        {
            __instance.waitTime = 600; // 10 minutes
            Log.LogInfo($"There is a thief! You have {__instance.waitTime} seconds.");

            // marks the thief with speech bubble
            var dialog = owner.dialogController;
            dialog.TryArmDialog(__instance.dialogFreeName, true, true);
            owner.timer.StartTimer(__instance.waitTime);

            // TODO: raise an event / add an incident to show that there is a thief ingame

            __result = true;
            return false;
        }

        /// <summary>
        /// Doubles the gold you receive as a payment from customers (or pick up from the ground).
        /// Edit FinanceManager.goldCount for direct gold changes.
        /// </summary>
        /// <param name="add"></param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FinanceManager), nameof(FinanceManager.AddGold))]
        [HarmonyPatch(new Type[] { typeof(int), typeof(FinanceManager.GoldSource) })]
        private static void DoubleIncome(ref int add, FinanceManager.GoldSource source)
        {
            if (source == FinanceManager.GoldSource.unknown) // do not double on credit or start gold
            {
                add *= 2;
            }
        }
    }

    private static class FairyMods
    {
        // TODO: make work
        /// <summary>
        /// Doubles the speed of fairies. Well, not as of right now, no Fairy method seems to work
        /// Default:
        ///     speedEmpty = 2.5;
        ///     speedCarry = 1.5;
        ///     speedMultPerLevel = 0.1;
        /// </summary>
        /// <param name="__instance">The instance of the Fairy.</param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fairy), nameof(Fairy.Update))]
        private static void FasterFairies(ref Fairy __instance)
        {
            Log.LogInfo($"Fairy#Update: [{__instance.speedEmpty}, {__instance.speedCarry}, {__instance.speedMultPerLevel}]*2");
            __instance.speedEmpty = 5f;
            __instance.speedCarry = 3f;
        }


        // TODO: fairies never get hungry (Steward.hungerTime, hungerWaitingMult, hungerFreeMult, Servant.hunger)
        // hungerTime = 20.0; hungerWaitingMult = 0.6; hungerFreeMult = 0.4;
    }
}
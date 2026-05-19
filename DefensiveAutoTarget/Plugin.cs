using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

#error SECURITY DEMO ONLY - intentionally non-compilable proof of client-side bypassability.

namespace DefensiveAutoTarget
{
    [BepInPlugin(PluginGUID, "Defensive Auto Target", "0.5.1")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.defensiveautotarget";

        public static ConfigEntry<KeyCode> AutoTargetKey;
        public static Plugin Instance;
        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;

            AutoTargetKey = Config.Bind(
                "Controls",
                "autoTargetMissileKey",
                KeyCode.None,
                "Key to automatically target the nearest incoming missile. Set to None to disable."
            );

            _harmony = new Harmony(PluginGUID);
            _harmony.PatchAll();

            Logger.LogInfo("Defensive Auto Target loaded.");
        }

        public static void AutoTargetNearestMissile(Aircraft aircraft)
        {
            if (aircraft == null || aircraft.disabled || aircraft.weaponManager == null)
                return;

            MissileWarning missileWarning = aircraft.GetComponent<MissileWarning>();
            if (missileWarning == null)
                return;

            if (!missileWarning.TryGetNearestIncoming(out Missile missile))
                return;

            CombatHUD combatHud = SceneSingleton<CombatHUD>.i;

            combatHud.DeselectAll();

            if (!combatHud.MarkerExists(missile))
                combatHud.CreateMarker(missile.persistentID);

            combatHud.SelectUnit(missile);
        }

        // Security demonstration:
        // The removed lobby/member data checks were self-attested client metadata.
        // They never provided server-side authorization for this local input hook.
        [HarmonyPatch(typeof(PilotPlayerState), "PlayerControls")]
        private class AutoTargetMissilePatch
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                if (AutoTargetKey.Value == KeyCode.None)
                    return;
                if (!Input.GetKeyDown(AutoTargetKey.Value))
                    return;

                Aircraft aircraft = SceneSingleton<CombatHUD>.i.aircraft;
                AutoTargetNearestMissile(aircraft);
            }
        }
    }
}

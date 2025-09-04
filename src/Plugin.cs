using System;
using BepInEx;
using BepInEx.Logging;
using HUD;
using UnityEngine;
using static TheAlchemist.Vars;

namespace TheAlchemist
{
    [BepInDependency("slime-cubed.slugbase")]
    [BepInDependency("customslugcatutils")]
    [BepInDependency("com.dual.improved-input-config")]
    [BepInPlugin(MOD_ID, "The Alchemist", MOD_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable InconsistentNaming
        public const string MOD_ID = "nuclear.TheAlchemist";
        public const string MOD_VERSION = "0.8.6";
        // ReSharper restore InconsistentNaming
        // ReSharper restore MemberCanBePrivate.Global
        
        internal new static ManualLogSource Logger;
        
        public void OnEnable()
        {
            Logger = base.Logger;
            
            Logger.LogInfo($"Plugin version {MOD_VERSION} is awake.");

            Logger.LogInfo("Registering Improved Input Config keybinds");
            
            try
            {
                ConvertToMatterKey =
                    Utils.RegisterKeybind("convertToMatter", "Convert To Matter", KeyCode.V, KeyCode.None);
                ConvertMatterToFoodKey =
                    Utils.RegisterKeybind("convertToFood", "Convert Matter To Food", KeyCode.B, KeyCode.None);
                SynthesisKey =
                    Utils.RegisterKeybind("synthesis", "Synthesize Object", KeyCode.N, KeyCode.None);
                NitrousKey =
                    Utils.RegisterKeybind("nitrous", "Activate nitrous", KeyCode.X, KeyCode.None);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            
            Logger.LogInfo("Keybinds registered");
            
            On.HUD.HUD.ctor += InitAlchemistHUD;
            
            OracleHooks.Apply();
            PlayerHooks.Apply();
        }

        private static void InitAlchemistHUD(On.HUD.HUD.orig_ctor orig, HUD.HUD self, FContainer[] fcontainers, RainWorld rainworld, IOwnAHUD owner)
        {
            orig(self, fcontainers, rainworld, owner);

            self.AddPart(new AlchemistHUD(self, rainworld));
        }
    }
}
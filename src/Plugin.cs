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
        public const string MOD_ID = "nuclear.TheAlchemist";
        public const string MOD_VERSION = "0.3.0";
        
        internal new static ManualLogSource Logger;
        
        public void OnEnable()
        {
            Logger = base.Logger;
            
            Logger.LogInfo($"Plugin version {MOD_VERSION} awake");

            Logger.LogInfo("Registering Improved Input Config keybinds");
            
            try
            {
                EatItemInStomachKey =
                    Utils.RegisterKeybind("eatStomachItem", "Convert Stomach Item", KeyCode.V, KeyCode.None);
                ConvertFoodToMatterKey =
                    Utils.RegisterKeybind("convertFoodToMateter", "Convert Food", KeyCode.B, KeyCode.None);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            
            Logger.LogInfo("Keybinds registered");
            
            PlayerHooks.Apply();
            
            On.HUD.HUD.ctor += HUDInit;
        }

        private static void HUDInit(On.HUD.HUD.orig_ctor orig, HUD.HUD self, FContainer[] fcontainers, RainWorld rainworld, IOwnAHUD owner)
        {
            orig(self, fcontainers, rainworld, owner);

            self.AddPart(new AlchemistMatterLabels(self, rainworld));
        }
    }
}
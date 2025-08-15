using System;
using BepInEx;
using BepInEx.Logging;
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
        public const string MOD_VERSION = "0.2.4";
        
        internal new static ManualLogSource Logger;
        
        public void OnEnable()
        {
            Logger = base.Logger;
            
            Logger.LogInfo($"Plugin version {MOD_VERSION} awake");

            Logger.LogInfo("Registering Improved Input Config keybinds");
            
            try
            {
                EatItemInStomachKey =
                    Utils.RegisterKeybind("eatStomachItem", "Eat Stomach Item", KeyCode.V, KeyCode.None);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            
            Logger.LogInfo("Keybinds registered");
            
            PlayerHooks.Apply();
            
            //On.RoomCamera.ctor += RoomCameraInit;
        }

        private static void RoomCameraInit(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
        {
            orig(self, game, cameraNumber);

            foreach (var info in InfoList)
            {
                
            }
        }
    }
}
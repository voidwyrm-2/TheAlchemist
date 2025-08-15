using ImprovedInput;

namespace TheAlchemist;

using static Vars;
using static Plugin;

public static class PlayerHooks
{
    private static int _playerCount;
    
    internal static void Apply()
    {
        On.Player.ctor += PlayerInit;
        On.Player.CanBeSwallowed += PlayerAllowItemSwallowing;
        On.Player.Update += PlaterUpdate;
    }

    private static void PlaterUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        
        if (self.dead || !self.IsAlchem())
            return;
        
        var info = self.GetInfo();

        if (self.IsPressed(EatItemInStomachKey) && self.objectInStomach != null)
        {
            info.StomachEatTicker++;

            if (info.StomachEatTicker >= 20)
            {
                var originalMatter = info.Matter;
                
                info.StomachEatTicker = 0;
                
                var obj = self.objectInStomach;
                
                self.objectInStomach = null;
                
                info.Matter += obj.type.GetMatterValueForItem();
                
                Logger.LogDebug($"Stomach item (which was a {obj.type}) consumed, matter was {originalMatter}, it's now {info.Matter}");
            }
        }
        else
        {
            info.StomachEatTicker = 0;
        }
    }

    private static bool PlayerAllowItemSwallowing(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject obj)
    {
        if (self.IsAlchem() && obj is Spear)
            return true;
        
        return orig(self, obj);
    }

    private static void PlayerInit(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (!self.IsAlchem())
            return;
        
        Logger.LogInfo($"Alchemist Player found with id {_playerCount}, initializing...");

        AlchemistInfo info = new();
        Alchemists.Add(self, info);
        InfoList.Add(info);

        Logger.LogInfo($"Player {_playerCount} initialized");
        _playerCount++;
    }
}
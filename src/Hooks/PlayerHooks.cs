using System;
using ImprovedInput;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheAlchemist;

using static Vars;
using static Plugin;

public static class PlayerHooks
{
    internal static void Apply()
    {
        On.Player.ctor += PlayerInit;
        On.Player.CanBeSwallowed += AllowItemSwallowing;
        On.Player.Update += OnUpdate;
        On.ShelterDoor.Close += SavePlayerData;
        On.Player.SwallowObject += OnSwallowObject;
        On.Player.GrabUpdate += OnGrabUpdate;
        On.Player.PermaDie += OnPermaDie;
    }

    private static void OnPermaDie(On.Player.orig_PermaDie orig, Player self)
    {
        if (self.IsAlchem())
        {
            var info = self.GetInfo();

            info.Matter -= MatterLostOnDeath;
            
            if (info.Matter < 0)
                info.Matter = 0;
        }
            
        orig(self);
    }

    private static bool IsSwallowableCreature(Player player, Creature crit)
    {
        if (player.IsAlchem() && (crit.State.meatLeft <= 0 || player.FoodInStomach >= player.MaxFoodInStomach) &&
            crit.abstractCreature.IsSwallowable() && crit.dead)
            return true;
        
        return false;
    }

    private static void OnGrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        PhysicalObject grasp = null;

        if (self.grasps[0] != null)
        {
            grasp = self.grasps[0].grabbed;
        }
        else if (self.grasps[1] != null)
        {
            grasp = self.grasps[1].grabbed;
        }

        var foodValue = -1;

        if (grasp is Creature crit && IsSwallowableCreature(self, crit))
        {
            foodValue = crit.Template.meatPoints;
            crit.Template.meatPoints = 0;
        }

        try
        {
            orig(self, eu);
        }
        finally
        {
            if (foodValue > -1)
                (grasp as Creature)!.Template.meatPoints = foodValue;
        }
    }

    private static void OnSwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        if (self.IsAlchem())
            Logger.LogDebug($"Alchemist swallowing {self.grasps[grasp].grabbed.abstractPhysicalObject.Format()}");

        orig(self, grasp);
    }

    private static void SavePlayerData(On.ShelterDoor.orig_Close orig, ShelterDoor self)
    {
        foreach (var info in InfoMap.Values)
        {
            Alchemists.Remove(info.Owner);
            
            if (info.Saved || !self.room.game.IsStorySession)
                continue;
                
            Logger.LogInfo($"Saving info for player {info.PlayerNumber}");
            var save = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData);
            info.Save(save);
            Logger.LogInfo($"Saved info for player {info.PlayerNumber}");
        }
        
        InfoMap.Clear();

        orig(self);
    }

    private static void OnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.dead && self.IsAlchem())
        {
            var info = self.GetInfo();

            if (self.IsPressed(ConvertToMatterKey) && (self.objectInStomach != null || self.FoodInStomach > 0))
            {
                info.StomachEatTicker++;
                
                self.Blink(2);

                if (info.StomachEatTicker >= 33)
                {
                    self.Blink(4);
                    
                    info.StomachEatTicker = 0;
                    
                    var originalMatter = info.Matter;

                    if (self.objectInStomach != null)
                    {
                        var obj = self.objectInStomach;

                        self.objectInStomach = null;

                        var matter = Utils.GetMatterValueForObject(obj);

                        if (matter > 0)
                            info.Matter += matter;
                        
                        Logger.LogDebug($"Consumed stomach item (which was a {obj.Format()}) for {matter} matter; matter was {originalMatter}, it's now {info.Matter}");
                    }
                    else
                    {
                        self.SubtractFood(1);
                        info.Matter += 20;
                        Logger.LogDebug($"Consumed 1 food pip; matter was {originalMatter}, it's now {info.Matter}");
                    }
                    
                    var color = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);

                    var vel = new Vector2(Random.value, Random.value) * 2;
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, vel, color, null, 30, 50));
                    
                    var vel2 = new Vector2(Random.value * 2, Random.value) * 3;
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, vel2, color, null, 37, 60));
                }
            }
            else
            {
                info.StomachEatTicker = 0;
            }
            
            if (self.IsPressed(ConvertMatterToFoodKey) && self.FoodInStomach < self.MaxFoodInStomach && info.Matter >= FoodPipMatterCost)
            {
                info.MatterToFoodTicker++;
                
                self.Blink(2);

                if (info.MatterToFoodTicker >= 33)
                {
                    self.Blink(4);

                    info.MatterToFoodTicker = 0;

                    var originalMatter = info.Matter;

                    info.Matter -= FoodPipMatterCost;

                    self.AddFood(1);

                    Logger.LogDebug($"Added 1 food pip for {FoodPipMatterCost} matter; matter was {originalMatter}, it's now {info.Matter}");

                    var color = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);

                    var vel = new Vector2(Random.value, Random.value) * 2;
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, vel, color, null, 40, 70));

                    //self.room.AddObject(new Lightning(self.room, 2f + Random.value * 3, false));
                    //self.room.AddObject(new KarmicArmor.EnergyStrand(4 + (int)(Random.value * 10), 0.5f + Random.value * 2)
                    //{
                    //    pos = self.mainBodyChunk.pos
                    //});
                }
            }
            else
            {
                info.MatterToFoodTicker = 0;
            }

            if (info.SynthCodeKeyCooldown > 0)
                info.SynthCodeKeyCooldown--;

            if (self.IsPressed(SynthesisKey))
            {
                if (info.SynthCodeKeyCooldown == 0)
                {
                    var digit = $"{Input.inputString}".Trim().Trim('n');

                    if (digit.Length > 0 && digit[0] >= '0' && digit[0] <= '9')
                    {
                        info.SynthCode += digit[0];
                        Logger.LogDebug($"Digit added: '{digit}', synthCode is now '{info.SynthCode}'");
                    }

                    info.SynthCodeKeyCooldown = 2;
                }
            }
            else if (self.objectInStomach == null && info.SynthCode.Length > 0)
            {
                var stringCode = info.SynthCode;
                info.SynthCode = "";
                
                int synthCode;

                try
                {
                    synthCode = int.Parse(stringCode);
                }
                catch (FormatException e)
                {
                    Logger.LogError($"Cannot parse synthesis code: {e.Message}");
                    return;
                }
                catch (OverflowException e)
                {
                    Logger.LogError($"Cannot parse synthesis code: {e.Message}");
                    return;
                }

                AbstractPhysicalObject obj = null;

                if (synthCode >= 0 && synthCode < SynthItems.Length)
                {
                    var world = self.room.world;
                    var pos = self.room.GetWorldCoordinate(self.mainBodyChunk.pos);
                    var id = self.room.game.GetNewID();
                    obj = SynthItems[synthCode](world, pos, id);
                }

                Logger.LogDebug($"synthesis code: '{synthCode}', valid? {obj != null}");

                if (obj != null)
                {
                    var cost = Utils.GetMatterValueForObject(obj);

                    if (info.Matter >= cost)
                    {
                        var originalMatter = info.Matter;
                        
                        info.Matter -= cost;
                        self.objectInStomach = obj;
                        
                        Logger.LogDebug($"Created a {obj.Format()} for {cost} matter; matter was {originalMatter}, it's now {info.Matter}");
                    }
                    else
                    {
                        Logger.LogDebug($"Not enough matter to create {obj.Format()}; need {cost}, but have {info.Matter}");
                    }
                }
            }
            else
            {
                info.SynthCode = "";
            }
        }
    }

    private static bool CanSwallowThing(Player player, PhysicalObject obj)
    {
        if (player.objectInStomach == null)
        {
            if (obj is Spear or VultureMask)
                return true;
            
            if (obj is Creature crit && IsSwallowableCreature(player, crit))
                return true;

            if (player.FoodInStomach == player.MaxFoodInStomach && obj is IPlayerEdible)
                return true;
        }

        return false;
    }

    private static bool AllowItemSwallowing(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject obj)
    {
        if (self.input[0].pckp)
        {
            if (self.IsAlchem())
                Logger.LogDebug(
                    $"Alchemist is trying to swallow an object (which is a {obj.abstractPhysicalObject.Format()})");

            if (self.IsAlchem() && CanSwallowThing(self, obj))
            {
                Logger.LogDebug("Item can be swallowed");
                return true;
            }

            if (self.IsAlchem())
                Logger.LogDebug("Swallowing failed");
        }

        return orig(self, obj);
    }

    private static void PlayerInit(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (self.IsAlchem())
        {
            var playerNumber = self.playerState.playerNumber;
            
            Logger.LogInfo($"Alchemist Player found with id {playerNumber}, initializing...");

            if (InfoMap.ContainsKey(playerNumber))
            {
                InfoMap.Remove(playerNumber);
                Logger.LogInfo($"Player {playerNumber} was already initialized, previous info cleared");
            }

            AlchemistInfo info;
            bool loaded;

            if (self.room.game.IsStorySession)
            {
                var save = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData);
                
                (info, loaded) = AlchemistInfo.LoadFromSave(save, self);
            }
            else
            {
                loaded = false;
                info = new AlchemistInfo(self);
            }

            Alchemists.Add(self, info);
            InfoMap.Add(playerNumber, info);

            if (loaded)
                Logger.LogInfo($"Player {playerNumber} info loaded from save");
            else
                Logger.LogInfo($"Player {playerNumber} info initialized");
        }
    }
}
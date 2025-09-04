using System;
using ImprovedInput;
using RWCustom;
using Smoke;
using UnityEngine;

namespace TheAlchemist;

using static Vars;
using static Plugin;

public static class PlayerHooks
{
    internal static void Apply()
    {
        On.Player.ctor += PlayerInit;
        On.Player.Update += OnUpdate;
        On.ShelterDoor.Close += SavePlayerData;
        On.Player.PermaDie += OnPermaDie;
        On.Player.MovementUpdate += OnMovementUpdate;
        On.Player.NewRoom += OnNewRoom;
    }

    private static void OnNewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
    {
        FireSmoke smoke = null;
        
        if (self.IsAlchem() && self.TryGetInfo(out var info) && info.NitrousSmoke != null)
        {
            smoke = info.NitrousSmoke;
            info.NitrousSmoke.RemoveFromRoom();
        }
        
        orig(self, newRoom);

        if (smoke != null)
            newRoom.AddObject(smoke);
    }

    private static void OnMovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.dead && self.IsAlchem() && self.TryGetInfo(out var info))
        {
            if (info.NitrousActive && self.input[0].AnyDirectionalInput && self.canJump > 0 &&
                self.firstChunk.vel != Vector2.zero &&
                (self.bodyMode == Player.BodyModeIndex.Default || self.bodyMode == Player.BodyModeIndex.Stand))
            {
                if (info.NitrousMatterTicker == NitrousMatterTicks)
                {
                    info.Matter -= NitrousMatterCost;
                    info.NitrousMatterTicker = 0;
                }
                else
                {
                    info.NitrousMatterTicker++;
                }

                //var a = Custom.RNV();
                
                //self.room.AddObject(new Spark(self.bodyChunks[1].pos + a * 5f,
                //    (a - self.firstChunk.vel).normalized * self.firstChunk.vel.magnitude * 1.3f, Color.white, null, 6,
                //    12));

                if (self.room.ViewedByAnyCamera(self.mainBodyChunk.pos, 300f))
                {
                    var catColor = PlayerGraphics.SlugcatColor((self.State as PlayerState)!.slugcatCharacter);
                    info.NitrousSmoke?.EmitSmoke(self.mainBodyChunk.pos, Custom.RNV(), catColor, 25);
                    info.NitrousSmoke?.EmitSmoke(self.mainBodyChunk.pos, Custom.RNV(), catColor, 30);
                }
            }
        }
    }

    private static void OnPermaDie(On.Player.orig_PermaDie orig, Player self)
    {
        if (self.IsAlchem() && self.TryGetInfo(out var info))
        {
            info.Matter -= MatterLostOnDeath;
            
            if (info.Matter < 0)
                info.Matter = 0;
        }
            
        orig(self);
    }

    private static void SavePlayerData(On.ShelterDoor.orig_Close orig, ShelterDoor self)
    {
        if (self.room.game.IsStorySession)
        {
            foreach (var info in InfoMap.Values)
            {
                Alchemists.Remove(info.Owner);

                if (info.Saved)
                    continue;

                Logger.LogInfo($"Saving info for player {info.PlayerNumber}");
                var save = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState
                    .miscWorldSaveData);
                info.Save(save);
                Logger.LogInfo($"Saved info for player {info.PlayerNumber}");
            }
        }

        InfoMap.Clear();

        orig(self);
    }

    private static void OnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.dead && self.IsAlchem() && self.TryGetInfo(out var info))
        {
            if (self.IsPressed(ConvertToMatterKey) && (self.OccupiedHand() > -1 || self.FoodInStomach > 0 || self.Debug()))
            {
                info.ObjectToMatterTicker++;
                
                self.Blink(2);

                if (info.ObjectToMatterTicker >= ObjectToMatterTicks || self.Debug())
                {
                    self.Blink(4);
                    
                    info.ObjectToMatterTicker = 0;
                    
                    var originalMatter = info.Matter;

                    if (self.Debug())
                    {
                        info.Matter += 100;
                    }
                    else if (self.OccupiedHand() > -1)
                    {
                        var index = self.OccupiedHand();
                        var obj = self.grasps[index].grabbed;
                        
                        info.Meta.CheckForMetaItem(self, obj);

                        var matter = Utils.GetMatterValueForObject(obj);
                        
                        self.SpawnObjectToMatterEffect(obj.firstChunk.pos);
                        self.ReleaseGrasp(index);
                        obj.RemoveFromRoom();
                        obj.Destroy();

                        if (matter > 0)
                            info.Matter += matter;
                        
                        Logger.LogDebug($"Consumed stomach item (which was a {obj.abstractPhysicalObject.Format()}) for {matter} matter; matter was {originalMatter}, it's now {info.Matter}");
                    }
                    else
                    {
                        self.SubtractFood(1);
                        info.Matter += 20;
                        self.SpawnFoodToMatterEffect(self.mainBodyChunk.pos);
                        Logger.LogDebug($"Consumed 1 food pip; matter was {originalMatter}, it's now {info.Matter}");
                    }
                }
            }
            else
            {
                info.ObjectToMatterTicker = 0;
            }
            
            if (self.IsPressed(ConvertMatterToFoodKey) && self.FoodInStomach < self.MaxFoodInStomach && info.Matter >= FoodPipMatterCost)
            {
                info.MatterToFoodTicker++;
                
                self.Blink(2);

                if (info.MatterToFoodTicker >= FoodToMatterTicks)
                {
                    self.Blink(4);

                    info.MatterToFoodTicker = 0;

                    var originalMatter = info.Matter;

                    info.Matter -= FoodPipMatterCost;

                    self.AddFood(1);

                    Logger.LogDebug($"Added 1 food pip for {FoodPipMatterCost} matter; matter was {originalMatter}, it's now {info.Matter}");

                    self.SpawnFoodToMatterEffect(self.mainBodyChunk.pos);
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
            else if (self.FreeHand() > -1 && info.SynthCode.Length > 0)
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
                    var pos = self.abstractCreature.pos;
                    var id = self.room.game.GetNewID();
                    obj = SynthItems[synthCode](world, pos, id);
                }

                Logger.LogDebug($"synthesis code: '{synthCode}', valid? {obj != null}");

                if (obj != null)
                {
                    var cost = Utils.GetMatterValueForAbstractObject(obj);

                    if (info.Matter >= cost)
                    {
                        var originalMatter = info.Matter;

                        self.room.abstractRoom.AddEntity(obj);

                        try
                        {
                            obj.RealizeInRoom();
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                obj.Abstractize(obj.pos);
                            }
                            finally
                            {
                                self.room.abstractRoom.RemoveEntity(obj);
                            }

                            Logger.LogError(
                                $"Error while realizing synthesized object (info: code? {synthCode}, obj? {obj.Format()}, matter? {info.Matter}): {e.Message}");
                            return;
                        }

                        self.SlugcatGrab(obj.realizedObject, self.FreeHand());

                        self.SpawnMatterToObjectEffect(obj.realizedObject.firstChunk.pos);

                        info.Matter -= cost;

                        Logger.LogDebug(
                            $"Created a {obj.Format()} for {cost} matter; matter was {originalMatter}, it's now {info.Matter}");
                    }
                    else
                    {
                        Logger.LogDebug(
                            $"Not enough matter to create {obj.Format()}; need {cost}, but have {info.Matter}");
                    }
                }
            }
            else
            {
                info.SynthCode = "";
            }

            if (self.IsPressed(NitrousKey) && info.Matter >= NitrousMatterCost)
            {
                if (!info.NitrousActive)
                {
                    info.State.Save(self);
                    info.NitrousActive = true;
                    
                    const float groundIncreaseFac = 2.5f;
                    const float poleIncreaseFac = 2.7f;

                    self.slugcatStats.runspeedFac *= groundIncreaseFac;
                    self.slugcatStats.poleClimbSpeedFac *= poleIncreaseFac;
                    self.slugcatStats.corridorClimbSpeedFac *= poleIncreaseFac;
                    self.slugcatStats.loudnessFac *= groundIncreaseFac;

                    if (info.NitrousSmoke == null)
                        info.NitrousSmoke = new FireSmoke(self.room);
                }
            }
            else if (info.NitrousActive)
            {
                info.State.Load(self);
                info.NitrousActive = false;
            }
        }
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
                info = new AlchemistInfo(self);
                loaded = false;
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
using SlugBase.SaveData;
using Smoke;

namespace TheAlchemist;

internal class AlchemistInfo
{
    internal AlchemistMeta Meta = new();
    internal readonly AlchemistState State = new();
    
    internal int Matter;
    internal int ObjectMatterTicker = 0;
    internal int FoodMatterTicker = 0;
    internal int SynthCodeKeyCooldown = 0;
    internal string SynthCode = "";

    internal int HyperspeedMatterTicker = 0;
    internal bool HyperspeedActive = false;

    internal FireSmoke HyperspeedSmoke;

    internal AbstractPhysicalObject.AbstractObjectType LastConsumed;
    
    internal bool Saved { get; private set; }
    internal readonly int PlayerNumber;
    internal readonly Player Owner;

    internal AlchemistInfo(Player owner)
    {
        Owner = owner;
        PlayerNumber = owner.playerState.playerNumber;
    }

    internal static (AlchemistInfo Info, bool Loaded) LoadFromSave(SlugBaseSaveData save, Player owner)
    {
        AlchemistInfo info = new(owner);
        
        if (save == null)
            return (info, false);
        
        var loaded = false;

        if (save.TryGet(GetMatterSaveKey(owner.playerState.playerNumber), out int matter))
        {
            info.Matter = matter;
            loaded = true;
        }
        
        if (save.TryGet(GetMatterSaveKey(owner.playerState.playerNumber), out AbstractPhysicalObject.AbstractObjectType objt))
            info.LastConsumed = objt;
        
        var (meta, loadedMeta) = AlchemistMeta.LoadFromSave(save, info.PlayerNumber);

        info.Meta = meta;

        return (info, loaded || loadedMeta);
    }

    private static string GetMatterSaveKey(int playerNumber) =>
        $"p{playerNumber}-matter";
    
    private static string GetLastConsumedSaveKey(int playerNumber) =>
        $"p{playerNumber}-lastConsumed";

    internal void Save(SlugBaseSaveData save)
    {
        save.Set(GetMatterSaveKey(PlayerNumber), Matter);

        if (LastConsumed is null)
            save.Remove(GetLastConsumedSaveKey(PlayerNumber));
        else
            save.Set(GetLastConsumedSaveKey(PlayerNumber), LastConsumed);
        
        Saved = true;
    }
}
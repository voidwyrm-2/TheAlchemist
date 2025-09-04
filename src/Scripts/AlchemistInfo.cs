using SlugBase.SaveData;
using Smoke;

namespace TheAlchemist;

internal class AlchemistInfo
{
    internal AlchemistMeta Meta = new();
    internal readonly AlchemistState State = new();
    
    internal int Matter;
    internal int ObjectToMatterTicker = 0;
    internal int MatterToFoodTicker = 0;
    internal int SynthCodeKeyCooldown = 0;
    internal string SynthCode = "";

    internal int NitrousMatterTicker = 0;
    internal bool NitrousActive = false;

    internal FireSmoke NitrousSmoke;
    
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
        
        var (meta, loadedMeta) = AlchemistMeta.LoadFromSave(save, info.PlayerNumber);

        info.Meta = meta;

        return (info, loaded || loadedMeta);
    }

    private static string GetMatterSaveKey(int playerNumber) =>
        $"p{playerNumber}-matter";

    internal void Save(SlugBaseSaveData save)
    {
        save.Set(GetMatterSaveKey(PlayerNumber), Matter);
        Saved = true;
    }
}
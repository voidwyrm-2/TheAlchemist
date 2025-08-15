using SlugBase.SaveData;

namespace TheAlchemist;

internal class AlchemistInfo
{
    internal int Matter;
    internal int StomachEatTicker = 0;
    internal int MatterToFoodTicker = 0;
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
        var loaded = false;

        if (save.TryGet(GetMatterSaveKey(owner.playerState.playerNumber), out int matter))
        {
            info.Matter = matter;
            loaded = true;
        }

        return (info, loaded);
    }

    private static string GetMatterSaveKey(int playerNumber) =>
        $"p{playerNumber}-matter";

    internal void Save(SlugBaseSaveData save)
    {
        save.Set(GetMatterSaveKey(PlayerNumber), Matter);
        Saved = true;
    }
}
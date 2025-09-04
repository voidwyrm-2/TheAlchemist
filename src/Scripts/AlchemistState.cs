namespace TheAlchemist;

internal class AlchemistState
{
    private float _runspeedFac;
    private float _poleClimbSpeedFac;
    private float _corridorClimbSpeedFac;
    private float _loudnessFac;
    private int _throwingSkill;

    internal void Save(Player player)
    {
        _runspeedFac = player.slugcatStats.runspeedFac;
        _poleClimbSpeedFac = player.slugcatStats.poleClimbSpeedFac;
        _corridorClimbSpeedFac = player.slugcatStats.corridorClimbSpeedFac;
        _loudnessFac = player.slugcatStats.loudnessFac;
        _throwingSkill = player.slugcatStats.throwingSkill;
    }

    internal void Load(Player player)
    {
        player.slugcatStats.runspeedFac = _runspeedFac;
        player.slugcatStats.poleClimbSpeedFac = _poleClimbSpeedFac;
        player.slugcatStats.corridorClimbSpeedFac = _corridorClimbSpeedFac;
        player.slugcatStats.loudnessFac = _loudnessFac;
        player.slugcatStats.throwingSkill = _throwingSkill;
    }
}
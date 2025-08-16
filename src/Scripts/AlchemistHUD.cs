using System;
using HUD;
using RWCustom;

namespace TheAlchemist;

public class AlchemistHUD : HudPart
{
    private readonly HUD.HUD _hud;
    private FLabel[] _matterLabels;
    private FLabel[] _codeLabels;
    
    public AlchemistHUD(HUD.HUD hud, RainWorld rainworld) : base(hud)
    {
        _hud = hud;
        _matterLabels = new FLabel[Vars.InfoMap.Count];
        _codeLabels = new FLabel[Vars.InfoMap.Count];
        
        var y = rainworld.screenSize.y - 40;

        for (var i = 0; i < Vars.InfoMap.Count; i++)
        {
            var info = Vars.InfoMap[i];

            FLabel matterLabel = new(Custom.GetFont(), $"{info.Matter}")
            {
                color = PlayerGraphics.SlugcatColor((info.Owner.State as PlayerState)!.slugcatCharacter),
                scale = 2f,
                x = 40,
                y = y
            };

            FLabel codeLabel = new(Custom.GetFont(), Vars.InfoMap[i].SynthCode)
            {
                color = PlayerGraphics.SlugcatColor((info.Owner.State as PlayerState)!.slugcatCharacter),
                scale = 2f,
                x = 40,
                y = y - 20
            };

            y -= 60;

            _hud.fContainers[1].AddChild(matterLabel);
            _matterLabels[i] = matterLabel;
            _hud.fContainers[1].AddChild(codeLabel);
            _codeLabels[i] = codeLabel;
        }
    }

    public override void Update()
    {
        base.Update();

        for (var i = 0; i < Vars.InfoMap.Count; i++)
            _matterLabels[i].text = $"{Vars.InfoMap[i].Matter}";

        for (var i = 0; i < Vars.InfoMap.Count; i++)
            _codeLabels[i].text = Vars.InfoMap[i].SynthCode;
    }

    public override void ClearSprites()
    {
        base.ClearSprites();
        
        foreach (var label in _matterLabels)
            label.RemoveFromContainer();
        
        _matterLabels = Array.Empty<FLabel>();
        
        foreach (var label in _codeLabels)
            label.RemoveFromContainer();
        
        _codeLabels = Array.Empty<FLabel>();
        
        _hud.parts.Remove(this);
    }
}
using System;
using HUD;
using RWCustom;

namespace TheAlchemist;

public class AlchemistMatterLabels : HudPart
{
    private readonly HUD.HUD _hud;
    private FLabel[] _labels;
    
    public AlchemistMatterLabels(HUD.HUD hud, RainWorld rainworld) : base(hud)
    {
        _hud = hud;
        _labels = new FLabel[Vars.InfoMap.Count];
        
        var y = rainworld.screenSize.y - 40;

        for (var i = 0; i < Vars.InfoMap.Count; i++)
        {
            var info = Vars.InfoMap[i];
            
            FLabel label = new(Custom.GetFont(), $"{info.Matter}")
            {
                color = PlayerGraphics.SlugcatColor((info.Owner.State as PlayerState)!.slugcatCharacter),
                scale = 2f,
                x = 40,
                y = y
            };

            y -= 30;
            
            _hud.fContainers[1].AddChild(label);
            _labels[i] = label;
        }
    }

    public override void Update()
    {
        base.Update();

        for (var i = 0; i < Vars.InfoMap.Count; i++)
            _labels[i].text = $"{Vars.InfoMap[i].Matter}";
    }

    public override void ClearSprites()
    {
        base.ClearSprites();
        
        foreach (var label in _labels)
            label.RemoveFromContainer();
        
        _labels = Array.Empty<FLabel>();
        
        _hud.parts.Remove(this);
    }
}
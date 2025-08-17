using UnityEngine;

namespace TheAlchemist;

internal class AlchemistPingCircle : TemplarCircle
{
	private readonly Color _color;
	
	internal AlchemistPingCircle(PhysicalObject source, Vector2 pos, float rad, float radIncrement, float radAcceleration, int lifeTime, bool followSource, Color color)
		: base(source, pos, rad, radIncrement, radAcceleration, lifeTime, followSource)
	{
		_color = color;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		var a = Vector2.Lerp(lastPos, pos, timeStacker);
		
		var num = Mathf.Lerp(lastRad, rad, timeStacker);
		
		var num2 = ((lifeTime - frame) - timeStacker) / lifeTime;
		
		//var num3 = Mathf.Pow(num2, 0.5f) * maxThickness;
		
		sLeaser.sprites[0].SetPosition(a - camPos);
		
		sLeaser.sprites[0].scale = num / 8f;
		
		sLeaser.sprites[0].color = new Color(_color.r / num, _color.g, _color.b, num2 * maxAlpha);
		
		if (!sLeaser.deleteMeNextFrame && (slatedForDeletetion || room != rCam.room))
			sLeaser.CleanSpritesAndRemove();
	}
}

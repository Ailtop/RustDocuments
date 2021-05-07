using FX.SpriteEffects;
using UnityEngine;

namespace FX
{
	public interface ISpriteEffectStack
	{
		Renderer mainRenderer { get; }

		void Add(SpriteEffect effect);

		bool Contains(SpriteEffect effect);

		bool Remove(SpriteEffect effect);
	}
}

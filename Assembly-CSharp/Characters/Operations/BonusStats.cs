using System;
using FX;
using UnityEngine;

namespace Characters.Operations
{
	[Obsolete("이거 대신 AttachAbility 사용하세요")]
	public class BonusStats : CharacterOperation
	{
		[SerializeField]
		private Stat.Values _stat;

		[SerializeField]
		private PoolObject _effect;

		[SerializeField]
		private int _offset = 1;

		[SerializeField]
		private float _duration = 1f;

		private Character _character;

		public override void Run(Character owner)
		{
			_character = owner;
			Stat.ValuesWithEvent.OnDetachDelegate onDetach = null;
			if (_effect != null)
			{
				PoolObject spawnedEffect = _effect.Spawn(owner.transform.position);
				VisualEffect.PostProcess(spawnedEffect, owner, 1f, 0f, Vector3.zero, true, true, true);
				SpriteRenderer component = spawnedEffect.GetComponent<SpriteRenderer>();
				SpriteRenderer mainRenderer = (owner.spriteEffectStack as SpriteEffectStack).mainRenderer;
				component.sortingLayerID = mainRenderer.sortingLayerID;
				component.sortingOrder = mainRenderer.sortingOrder + _offset;
				onDetach = delegate
				{
					spawnedEffect.Despawn();
				};
			}
			if (_duration == 0f)
			{
				if (!owner.stat.Contains(_stat))
				{
					owner.stat.AttachValues(_stat, onDetach);
				}
			}
			else
			{
				owner.stat.AttachOrUpdateTimedValues(_stat, _duration, onDetach);
			}
		}

		public override void Stop()
		{
			_character?.stat.DetachValues(_stat);
		}
	}
}

using FX;
using UnityEngine;

namespace Characters.Operations
{
	public class BuffToTarget : CharacterOperation
	{
		[SerializeField]
		private Stat.Values _stat;

		[SerializeField]
		private PoolObject _effect;

		[SerializeField]
		private int _offset = 1;

		[SerializeField]
		private float _duration = 1f;

		[SerializeField]
		private Character _target;

		public override void Run(Character owner)
		{
			Stat.ValuesWithEvent.OnDetachDelegate onDetach = null;
			if (_effect != null)
			{
				PoolObject spawnedEffect = _effect.Spawn(_target.transform.position);
				VisualEffect.PostProcess(spawnedEffect, _target, 1f, 0f, Vector3.zero, true, true, true);
				SpriteRenderer component = spawnedEffect.GetComponent<SpriteRenderer>();
				SpriteRenderer mainRenderer = (_target.spriteEffectStack as SpriteEffectStack).mainRenderer;
				component.sortingLayerID = mainRenderer.sortingLayerID;
				component.sortingOrder = mainRenderer.sortingOrder + _offset;
				onDetach = delegate
				{
					spawnedEffect.Despawn();
				};
			}
			_target.stat.AttachOrUpdateTimedValues(_stat, _duration, onDetach);
		}
	}
}

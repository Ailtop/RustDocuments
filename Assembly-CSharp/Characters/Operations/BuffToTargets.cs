using System.Collections.Generic;
using FX;
using UnityEngine;

namespace Characters.Operations
{
	public class BuffToTargets : CharacterOperation
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
		private List<Character> _targets;

		private void Awake()
		{
			foreach (Character target in _targets)
			{
				target.health.onDied += delegate
				{
					_targets.Remove(target);
				};
			}
		}

		public override void Run(Character owner)
		{
			Stat.ValuesWithEvent.OnDetachDelegate onDetach = null;
			Character character = SelectTarget();
			if (_effect != null)
			{
				PoolObject spawnedEffect = _effect.Spawn(character.transform.position);
				VisualEffect.PostProcess(spawnedEffect, character, 1f, 0f, Vector3.zero, true, true, true);
				SpriteRenderer component = spawnedEffect.GetComponent<SpriteRenderer>();
				SpriteRenderer mainRenderer = (character.spriteEffectStack as SpriteEffectStack).mainRenderer;
				component.sortingLayerID = mainRenderer.sortingLayerID;
				component.sortingOrder = mainRenderer.sortingOrder + _offset;
				onDetach = delegate
				{
					spawnedEffect.Despawn();
				};
			}
			character.stat.AttachOrUpdateTimedValues(_stat, _duration, onDetach);
		}

		private Character SelectTarget()
		{
			return _targets.Random();
		}
	}
}

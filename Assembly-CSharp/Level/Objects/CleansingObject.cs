using Characters;
using Characters.Abilities;
using Characters.Abilities.Enemies;
using FX;
using Singletons;
using UnityEngine;

namespace Level.Objects
{
	public class CleansingObject : InteractiveObject
	{
		[SerializeField]
		[GetComponent]
		private Animator _animator;

		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private EffectInfo _effectinfo;

		private const string deactivateClipCode = "Deactivate";

		public override void InteractWith(Character character)
		{
			IAbilityInstance instance = character.ability.GetInstance<CurseOfLight>();
			PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
			_effectinfo.Spawn(_spawnPosition.position);
			_animator.Play("Deactivate");
			if (instance != null)
			{
				character.ability.Remove(instance.ability);
			}
			Deactivate();
		}
	}
}

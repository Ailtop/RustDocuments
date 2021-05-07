using Characters;
using Characters.Gear.Weapons;
using Singletons;
using UnityEngine;

namespace Level
{
	public class DeterminedGrave : InteractiveObject
	{
		[SerializeField]
		[GetComponent]
		private Animator _animator;

		[SerializeField]
		private Weapon _weapon;

		public Weapon droppedWeapon { get; private set; }

		public override void OnActivate()
		{
			_animator.Play(InteractiveObject._activateHash);
		}

		public override void OnDeactivate()
		{
			_animator.Play(InteractiveObject._deactivateHash);
		}

		public override void InteractWith(Character character)
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
			StartCoroutine(_003CInteractWith_003Eg__CDelayedDrop_007C8_0());
			Deactivate();
		}
	}
}

using Characters;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class BossGate : InteractiveObject
	{
		[SerializeField]
		[GetComponent]
		private Animator _animator;

		private bool _used;

		public override void OnActivate()
		{
			base.OnActivate();
			_animator?.Play(InteractiveObject._activateHash);
		}

		public override void OnDeactivate()
		{
			base.OnDeactivate();
			_animator?.Play(InteractiveObject._deactivateHash);
		}

		public override void InteractWith(Character character)
		{
			if (!_used)
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
				_used = true;
				Singleton<Service>.Instance.levelManager.LoadNextMap();
			}
		}
	}
}

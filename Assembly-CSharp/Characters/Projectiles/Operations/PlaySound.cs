using FX;
using Singletons;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public sealed class PlaySound : Operation
	{
		[SerializeField]
		private SoundInfo _soundInfo;

		[SerializeField]
		private Transform _position;

		public override void Run(Projectile projectile)
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_soundInfo, (_position == null) ? base.transform.position : _position.position);
		}
	}
}

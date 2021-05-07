using UnityEngine;

namespace Characters.Operations.Fx
{
	public class DropParts : CharacterOperation
	{
		[SerializeField]
		private Transform _spawnPoint;

		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private ParticleEffectInfo _particleEffectInfo;

		[SerializeField]
		private Vector2 _direction;

		[SerializeField]
		private float _power = 3f;

		[SerializeField]
		private bool _interpolation;

		public override void Run(Character owner)
		{
			_particleEffectInfo.Emit(_spawnPoint.position, _range.bounds, _direction * _power, _interpolation);
		}
	}
}

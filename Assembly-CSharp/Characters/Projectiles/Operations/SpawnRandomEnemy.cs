using Characters.AI;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class SpawnRandomEnemy : Operation
	{
		[SerializeField]
		private Character[] _characters;

		[SerializeField]
		private bool _setPlayerAsTarget;

		[SerializeField]
		[Range(0f, 10f)]
		private float _distribution;

		[SerializeField]
		[Range(1f, 10f)]
		private int _repeatCount;

		[SerializeField]
		private bool _containInWave = true;

		public override void Run(Projectile projectile)
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			Vector3 position = projectile.transform.position;
			for (int i = 0; i < _repeatCount; i++)
			{
				float num = Random.Range(0f - _distribution, _distribution);
				Random.Range(0f - _distribution, _distribution);
				Vector3 position2 = position;
				position2.x += Random.Range(0f - _distribution, _distribution);
				position2.y += Random.Range(0f - _distribution, _distribution);
				Character character = Object.Instantiate(_characters.Random(), position2, Quaternion.identity);
				character.ForceToLookAt((num < 0f) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
				if (_setPlayerAsTarget)
				{
					character.GetComponentInChildren<AIController>().target = player;
				}
				if (_containInWave)
				{
					Map.Instance.waveContainer.Attach(character);
				}
			}
		}
	}
}

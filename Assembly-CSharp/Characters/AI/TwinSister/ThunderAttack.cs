using System.Collections;
using Characters.Operations.Attack;
using Characters.Operations.Fx;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.TwinSister
{
	public class ThunderAttack : MonoBehaviour
	{
		[SerializeField]
		private Collider2D _attackRange;

		[SerializeField]
		[Subcomponent(typeof(SpawnEffect))]
		private SpawnEffect _spawnAttackSign;

		[SerializeField]
		[Subcomponent(typeof(SweepAttack2))]
		private SweepAttack2 _sweepAttack;

		[Subcomponent(typeof(SpawnEffect))]
		[SerializeField]
		private SpawnEffect _sweepAttackEffect;

		[Subcomponent(typeof(PlaySound))]
		[SerializeField]
		private PlaySound _playSignSound;

		[Subcomponent(typeof(PlaySound))]
		[SerializeField]
		private PlaySound _playAttackSound;

		[SerializeField]
		private float _signDelay;

		[SerializeField]
		private float _term = 0.15f;

		[SerializeField]
		private float _distance;

		private int _count;

		private bool _initialized;

		private void Initialize(Character character)
		{
			Bounds bounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			float x = bounds.min.x;
			float x2 = bounds.max.x;
			float x3 = _attackRange.bounds.size.x;
			float x4 = _attackRange.bounds.extents.x;
			for (float num = x + x4; num <= x2; num += x3 + _distance)
			{
				_count++;
			}
			_sweepAttack.Initialize();
			_initialized = true;
		}

		public IEnumerator CRun(AIController controller)
		{
			if (!_initialized)
			{
				Initialize(controller.character);
			}
			Character character = controller.character;
			yield return character.chronometer.master.WaitForSeconds(_signDelay);
			Bounds platformBounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			float startX = ((character.lookingDirection == Character.LookingDirection.Left) ? platformBounds.max.x : platformBounds.min.x);
			float sizeX = _attackRange.bounds.size.x;
			float extentsX = _attackRange.bounds.extents.x;
			int sign = ((character.lookingDirection == Character.LookingDirection.Right) ? 1 : (-1));
			for (int j = 0; j < _count; j++)
			{
				float x = startX + (sizeX * (float)j + extentsX) * (float)sign + (float)j * _distance * (float)sign;
				_attackRange.transform.position = new Vector3(x, platformBounds.max.y);
				_spawnAttackSign.Run(character);
				_playSignSound.Run(character);
			}
			yield return character.chronometer.master.WaitForSeconds(1f);
			for (int i = 0; i < _count; i++)
			{
				float x2 = startX + (sizeX * (float)i + extentsX) * (float)sign + (float)i * _distance * (float)sign;
				_attackRange.transform.position = new Vector3(x2, platformBounds.max.y);
				Physics2D.SyncTransforms();
				_playAttackSound.Run(character);
				_sweepAttack.Run(character);
				_sweepAttackEffect.Run(character);
				yield return character.chronometer.master.WaitForSeconds(_term);
			}
		}
	}
}

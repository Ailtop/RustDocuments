using System;
using System.Collections;
using Characters.Projectiles;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Necromancy : Keyword
	{
		[SerializeField]
		private double[] _projectileCountByLevel = new double[4] { 0.0, 1.0, 2.0, 3.0 };

		[Space]
		[SerializeField]
		private float _fireDelay;

		private WaitForSeconds _waitForFireDelay;

		[Space]
		[SerializeField]
		private float _baseDamage = 10f;

		[SerializeField]
		private Projectile _projectile;

		public override Key key => Key.Necromancy;

		protected override IList valuesByLevel => _projectileCountByLevel;

		private void Awake()
		{
			_waitForFireDelay = new WaitForSeconds(_fireDelay);
		}

		protected override void Initialize()
		{
		}

		protected override void UpdateBonus()
		{
		}

		protected override void OnAttach()
		{
			Character obj = base.character;
			obj.onKilled = (Character.OnKilledDelegate)Delegate.Combine(obj.onKilled, new Character.OnKilledDelegate(OnKilled));
		}

		protected override void OnDetach()
		{
			Character obj = base.character;
			obj.onKilled = (Character.OnKilledDelegate)Delegate.Remove(obj.onKilled, new Character.OnKilledDelegate(OnKilled));
		}

		private void OnKilled(ITarget target, ref Damage damage)
		{
			if (!(target.character == null))
			{
				StartCoroutine(CSpawnProjectiles(damage.hitPoint));
			}
		}

		private IEnumerator CSpawnProjectiles(Vector3 position)
		{
			yield return _waitForFireDelay;
			double num = _projectileCountByLevel[base.level];
			for (int i = 0; (double)i < num; i++)
			{
				_projectile.reusable.Spawn(position).GetComponent<Projectile>().Fire(base.character, _baseDamage, UnityEngine.Random.Range(0, 360));
			}
		}
	}
}

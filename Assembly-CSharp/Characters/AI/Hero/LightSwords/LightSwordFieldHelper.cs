using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI.Hero.LightSwords
{
	public class LightSwordFieldHelper : MonoBehaviour
	{
		private class Interval
		{
			internal float left;

			internal float right;

			internal Interval(float left, float right)
			{
				this.left = left;
				this.right = right;
			}
		}

		[SerializeField]
		private Character _owner;

		[SerializeField]
		[MinMaxSlider(0f, 180f)]
		private Vector2 _fireRange;

		[SerializeField]
		private float _fireDistance;

		[SerializeField]
		private int _intervalCount;

		[SerializeField]
		private LightSwordPool _pool;

		private List<LightSword> _swords;

		private List<Interval> _intervals;

		private Bounds _platform;

		public void Fire()
		{
			if (_intervals == null)
			{
				MakeInterval();
			}
			_swords = _pool.Get();
			_platform = _owner.movement.controller.collisionState.lastStandingCollider.bounds;
			StartCoroutine(CFire());
		}

		private IEnumerator CFire()
		{
			int count = _swords.Count - 1;
			int intervalIndex = 0;
			int num;
			do
			{
				Vector2 destination = new Vector2(UnityEngine.Random.Range(_intervals[intervalIndex].left, _intervals[intervalIndex].right), _platform.max.y);
				float degree = UnityEngine.Random.Range(_fireRange.x, _fireRange.y);
				Vector2 source = CalculateFirePosition(destination, degree);
				_swords[count].Fire(_owner, source, destination);
				intervalIndex = (intervalIndex + 1) % _intervalCount;
				yield return Chronometer.global.WaitForSeconds(0.1f);
				num = count - 1;
				count = num;
			}
			while (num >= 0);
		}

		public void Sign()
		{
			_swords.ForEach(delegate(LightSword sword)
			{
				if (sword.active)
				{
					sword.Sign();
				}
			});
		}

		public void Draw(Character owner)
		{
			_swords.ForEach(delegate(LightSword sword)
			{
				if (sword.active)
				{
					sword.Draw(owner);
				}
			});
		}

		public int GetActivatedSwordCount()
		{
			return _swords.Count((LightSword sword) => sword.active);
		}

		public LightSword GetClosestFromPlayer()
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			float num = float.PositiveInfinity;
			LightSword lightSword = null;
			foreach (LightSword sword in _swords)
			{
				if (!sword.active)
				{
					continue;
				}
				if (lightSword == null)
				{
					lightSword = sword;
					num = Mathf.Abs(player.transform.position.x - sword.GetStuckPosition().x);
					continue;
				}
				Vector3 stuckPosition = sword.GetStuckPosition();
				float num2 = Mathf.Abs(player.transform.position.x - stuckPosition.x);
				if (num2 < num)
				{
					lightSword = sword;
					num = num2;
				}
			}
			return lightSword;
		}

		public LightSword GetBehindPlayer()
		{
			float x = Singleton<Service>.Instance.levelManager.player.transform.position.x;
			float x2 = _owner.transform.position.x;
			LightSword lightSword = null;
			foreach (LightSword sword in _swords)
			{
				if (!sword.active)
				{
					continue;
				}
				float x3 = sword.GetStuckPosition().x;
				float num = x3 - x;
				float num2 = x3 - x2;
				if ((x >= x2 && (num2 < 0f || num < 0f)) || (x <= x2 && (num2 > 0f || num > 0f)))
				{
					continue;
				}
				if (lightSword == null)
				{
					lightSword = sword;
					continue;
				}
				float num3 = Mathf.Abs(lightSword.GetStuckPosition().x - x);
				if (Mathf.Abs(num) < num3)
				{
					lightSword = sword;
				}
			}
			return lightSword;
		}

		public LightSword GetFarthestFromHero()
		{
			float x = _owner.transform.position.x;
			float num = float.NegativeInfinity;
			LightSword lightSword = null;
			foreach (LightSword sword in _swords)
			{
				if (!sword.active)
				{
					continue;
				}
				if (lightSword == null)
				{
					lightSword = sword;
					continue;
				}
				float num2 = Mathf.Abs(sword.GetStuckPosition().x - x);
				if (num2 > num)
				{
					lightSword = sword;
					num = num2;
				}
			}
			return lightSword;
		}

		private void MakeInterval()
		{
			_intervals = new List<Interval>();
			Bounds bounds = _owner.movement.controller.collisionState.lastStandingCollider.bounds;
			float num = 1f;
			float num2 = (bounds.size.x - num) / (float)_intervalCount;
			float num3 = bounds.min.x + num;
			float num4 = num3 + num2;
			for (int i = 0; i < _intervalCount; i++)
			{
				_intervals.Add(new Interval(num3, num4));
				num3 = num4;
				num4 = num3 + num2;
			}
			_intervals.Shuffle();
		}

		private Vector2 CalculateFirePosition(Vector2 destination, float degree)
		{
			Vector2 vector = Vector2.right * _fireDistance;
			float f = degree * ((float)Math.PI / 180f);
			float x = vector.x * Mathf.Cos(f) - vector.y * Mathf.Sin(f);
			float y = vector.x * Mathf.Sin(f) + vector.y * Mathf.Cos(f);
			return new Vector2(x, y) + destination;
		}
	}
}

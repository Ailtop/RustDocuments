using System.Collections;
using Characters.Actions.Constraints.Customs;
using UnityEngine;

namespace Characters.Gear.Weapons.Gauges
{
	public class Magazine : MonoBehaviour
	{
		[SerializeField]
		private ValueGauge _gaugeWithValue;

		[SerializeField]
		private Color _reloadColor;

		private Bullet _bullet;

		public bool nonConsumptionState { get; set; }

		private void Awake()
		{
			_bullet = new Bullet((int)_gaugeWithValue.maxValue);
			_gaugeWithValue.FillUp();
		}

		public bool Has(int amount)
		{
			return _bullet.Has(amount);
		}

		public void Consume(int amount)
		{
			if (!nonConsumptionState && _bullet.Consume(amount))
			{
				_gaugeWithValue.Consume(amount);
			}
		}

		public void Reload()
		{
			CoroutineProxy.instance.StartCoroutine(CReload());
		}

		private IEnumerator CReload()
		{
			Color defaultColor = _gaugeWithValue.defaultBarColor;
			_gaugeWithValue.defaultBarColor = _reloadColor;
			yield return Chronometer.global.WaitForSeconds(2f);
			_gaugeWithValue.FillUp();
			_bullet.Reload();
			_gaugeWithValue.defaultBarColor = defaultColor;
		}
	}
}

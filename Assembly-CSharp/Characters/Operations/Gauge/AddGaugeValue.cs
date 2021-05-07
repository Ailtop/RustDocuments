using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Operations.Gauge
{
	public class AddGaugeValue : Operation
	{
		[SerializeField]
		private ValueGauge _gaugeWithValue;

		[SerializeField]
		private int _amount = 1;

		public override void Run()
		{
			_gaugeWithValue.Add(_amount);
		}
	}
}

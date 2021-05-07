using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Operations.Gauge
{
	public class SetGaugeValue : Operation
	{
		[SerializeField]
		private ValueGauge _gaugeWithValue;

		[SerializeField]
		private float _value;

		public override void Run()
		{
			_gaugeWithValue.Set(_value);
		}
	}
}

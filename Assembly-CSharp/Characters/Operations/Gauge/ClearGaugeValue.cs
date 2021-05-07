using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Operations.Gauge
{
	public class ClearGaugeValue : Operation
	{
		[SerializeField]
		private ValueGauge _gaugeWithValue;

		public override void Run()
		{
			_gaugeWithValue.Clear();
		}
	}
}

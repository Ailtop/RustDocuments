using UnityEngine;

namespace Characters.Gear.Weapons.Gauges
{
	public abstract class Gauge : MonoBehaviour
	{
		public abstract float gaugePercent { get; }

		public abstract string displayText { get; }

		public abstract Color barColor { get; }

		public abstract bool secondBar { get; }

		public abstract Color secondBarColor { get; }
	}
}

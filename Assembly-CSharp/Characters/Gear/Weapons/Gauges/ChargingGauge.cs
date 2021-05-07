using Characters.Actions;
using UnityEngine;

namespace Characters.Gear.Weapons.Gauges
{
	public class ChargingGauge : Gauge
	{
		[SerializeField]
		private Color _defaultBarColor = Color.black;

		[Space]
		[SerializeField]
		private bool _secondBar;

		[SerializeField]
		private Color _secondBarColor;

		private ChargeAction _currentChargeAction;

		[Space]
		[SerializeField]
		private ChargeAction[] _chargeActions;

		public override float gaugePercent
		{
			get
			{
				if (!(_currentChargeAction == null))
				{
					return _currentChargeAction.chargingPercent;
				}
				return 0f;
			}
		}

		public override string displayText => string.Empty;

		public override Color barColor => _defaultBarColor;

		public override bool secondBar => _secondBar;

		public override Color secondBarColor => _secondBarColor;

		private void Awake()
		{
			ChargeAction[] chargeActions = _chargeActions;
			foreach (ChargeAction chargeAction in chargeActions)
			{
				chargeAction.onStart += delegate
				{
					_currentChargeAction = chargeAction;
				};
			}
		}
	}
}

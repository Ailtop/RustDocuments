using FX;
using UnityEngine;

namespace Characters.Operations
{
	public class ActiveLineEffect : CharacterOperation
	{
		[SerializeField]
		private LineEffect _lineEffect;

		[SerializeField]
		private Transform _startPoint;

		[SerializeField]
		private Transform _endPoint;

		public override void Run(Character owner)
		{
			_lineEffect.startPoint = _startPoint.position;
			_lineEffect.endPoint = _endPoint.position;
			_lineEffect.Run();
		}

		public override void Stop()
		{
			_lineEffect.Hide();
		}
	}
}

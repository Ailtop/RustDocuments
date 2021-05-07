using Level;
using UnityEngine;

namespace Runnables
{
	public class OpenGate : Runnable
	{
		[SerializeField]
		[GetComponentInParent(false)]
		private Map _map;

		private Gate _gate;

		private void Start()
		{
			_gate = _map.GetComponentInChildren<Gate>();
		}

		public override void Run()
		{
			_gate.Activate();
		}
	}
}

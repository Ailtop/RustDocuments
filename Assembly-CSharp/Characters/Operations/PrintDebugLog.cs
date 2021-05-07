using UnityEngine;

namespace Characters.Operations
{
	public class PrintDebugLog : Operation
	{
		[SerializeField]
		private string _message = "Run";

		public override void Run()
		{
			Debug.Log(_message);
		}

		public override string ToString()
		{
			return "PrintDebugLog (" + _message + ")";
		}
	}
}

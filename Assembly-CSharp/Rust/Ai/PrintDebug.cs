using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public sealed class PrintDebug : BaseAction
	{
		[ApexSerialization]
		private string debugMessage;

		public override void DoExecute(BaseContext c)
		{
			Debug.Log(debugMessage);
		}
	}
}

using Characters.Abilities;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class AddYakshaStompStack : Operation
	{
		[SerializeField]
		private YakshaPassiveAttacher _yakshaPassive;

		public override void Run()
		{
			_yakshaPassive.AddStack();
		}
	}
}

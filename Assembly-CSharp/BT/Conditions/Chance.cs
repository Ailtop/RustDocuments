using UnityEngine;

namespace BT.Conditions
{
	public class Chance : Condition
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float _successChance;

		protected override bool Check(Context context)
		{
			return MMMaths.Chance(_successChance);
		}
	}
}

using Characters.AI.Hero.LightSwords;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public sealed class LightSwordFieldMove : LightMove
	{
		private enum Where
		{
			PlayerBehind,
			PlayerClosest
		}

		[SerializeField]
		private LightSwordFieldHelper _helper;

		[SerializeField]
		private Where _where;

		protected override LightSword GetDestination()
		{
			switch (_where)
			{
			case Where.PlayerBehind:
				return _helper.GetBehindPlayer();
			case Where.PlayerClosest:
				return _helper.GetClosestFromPlayer();
			default:
				return _helper.GetClosestFromPlayer();
			}
		}
	}
}

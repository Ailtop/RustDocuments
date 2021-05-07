using Characters;
using Services;
using Singletons;
using UnityEngine;

namespace BT.Conditions
{
	public class PlayerInRange : Condition
	{
		[SerializeField]
		private float _distance;

		protected override bool Check(Context context)
		{
			Transform transform = context.Get<Transform>(Key.OwnerTransform);
			Character player = Singleton<Service>.Instance.levelManager.player;
			if (transform == null || player == null)
			{
				return false;
			}
			return Vector2.Distance(player.transform.position, transform.position) < _distance;
		}
	}
}

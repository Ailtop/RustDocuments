using UnityEngine;

namespace Characters.Actions.Cooldowns
{
	public class Infinity : Cooldown
	{
		protected static Infinity _singleton;

		internal static Infinity singleton
		{
			get
			{
				if (_singleton == null)
				{
					_singleton = new GameObject("Infinity")
					{
						hideFlags = HideFlags.HideAndDontSave
					}.AddComponent<Infinity>();
				}
				return _singleton;
			}
		}

		public override float remainPercent => 0f;

		public override bool canUse => true;

		internal override bool Consume()
		{
			return true;
		}
	}
}

using Level;
using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public class ToSavedPosition : Policy
	{
		[SerializeField]
		private PositionCache _repo;

		public override Vector2 GetPosition()
		{
			return _repo.Load();
		}
	}
}

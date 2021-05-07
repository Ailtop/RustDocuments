using Level;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations.Customs.EntSkul
{
	public class SummonEntMinionAtEntSapling : CharacterOperation
	{
		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private float _lifeTime;

		private static readonly NonAllocOverlapper _overlapper;

		static SummonEntMinionAtEntSapling()
		{
			_overlapper = new NonAllocOverlapper(32);
			_overlapper.contactFilter.SetLayerMask(512);
		}

		public override void Run(Character owner)
		{
			if (owner.playerComponents == null)
			{
				return;
			}
			foreach (SaplingTarget component in _overlapper.OverlapCollider(_range).GetComponents<SaplingTarget>())
			{
				if (!component.spawnable)
				{
					break;
				}
				component.SummonEntMinion(owner, _lifeTime);
			}
		}
	}
}

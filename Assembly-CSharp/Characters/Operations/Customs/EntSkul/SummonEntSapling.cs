using Level;
using UnityEngine;

namespace Characters.Operations.Customs.EntSkul
{
	public class SummonEntSapling : CharacterOperation
	{
		[SerializeField]
		private bool _intro = true;

		[SerializeField]
		private EntSapling _ent;

		[SerializeField]
		private LayerMask _terrainLayer;

		[SerializeField]
		private int _preloadCount = 5;

		private const float _groundFindingRayDistance = 9f;

		private void Awake()
		{
			_ent.Preload(_preloadCount);
		}

		public override void Run(Character owner)
		{
			if (owner.playerComponents != null)
			{
				Vector3 position = owner.transform.position;
				_ent.Spawn(position, _intro);
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using Characters.Operations;
using PhysicsUtils;
using UnityEngine;

namespace Characters.AI.Behaviours.Chimera
{
	public class WreckDestroy : Behaviour
	{
		[Header("Ready")]
		[SerializeField]
		private OperationInfos _readyOperations;

		[Header("Attack")]
		[SerializeField]
		private OperationInfos _attackOperations;

		[Header("End")]
		[SerializeField]
		private OperationInfos _endOperations;

		[Header("Hit")]
		[SerializeField]
		private OperationInfos _hitOperations;

		[SerializeField]
		private Collider2D _wreckFindRange;

		private static readonly NonAllocOverlapper _wreckOverlapper;

		static WreckDestroy()
		{
			_wreckOverlapper = new NonAllocOverlapper(100);
			_wreckOverlapper.contactFilter.SetLayerMask(1024);
		}

		private void Awake()
		{
			_readyOperations.Initialize();
			_attackOperations.Initialize();
			_endOperations.Initialize();
			_hitOperations.Initialize();
		}

		public void Ready(Character character)
		{
			_readyOperations.gameObject.SetActive(true);
			_readyOperations.Run(character);
		}

		public void Attack(Character character)
		{
			_attackOperations.gameObject.SetActive(true);
			_attackOperations.Run(character);
		}

		public void End(Character character)
		{
			_endOperations.gameObject.SetActive(true);
			_endOperations.Run(character);
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			DestroyWreck(controller.character);
			_hitOperations.gameObject.SetActive(true);
			_hitOperations.Run(controller.character);
			base.result = Result.Done;
			yield break;
		}

		private List<ChimeraWreck> GetChimeraWrecks()
		{
			_wreckOverlapper.contactFilter.SetLayerMask(1024);
			return _wreckOverlapper.OverlapCollider(_wreckFindRange).GetComponents<ChimeraWreck>();
		}

		public void DestroyWreck(Character character)
		{
			List<ChimeraWreck> chimeraWrecks = GetChimeraWrecks();
			for (int i = 0; i < chimeraWrecks.Count; i++)
			{
				chimeraWrecks[i].DestroyProp(character);
			}
		}
	}
}

using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Chimera
{
	public class ChimeraWreck : MonoBehaviour
	{
		[SerializeField]
		private ParticleEffectInfo _particle;

		[SerializeField]
		private Transform _emitPosition;

		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _operationInfos;

		private void Awake()
		{
			_operationInfos.Initialize();
		}

		public void DestroyProp(Character chimera)
		{
			_particle.Emit(_emitPosition.position, _range.bounds, Vector2.up * 2f);
			_operationInfos.Run(chimera);
			Object.Destroy(base.gameObject);
		}
	}
}

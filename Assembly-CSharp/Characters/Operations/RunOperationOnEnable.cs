using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations
{
	public class RunOperationOnEnable : MonoBehaviour
	{
		[SerializeField]
		private OperationInfos _operationInfos;

		private void OnEnable()
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			_operationInfos.Run(player);
		}
	}
}

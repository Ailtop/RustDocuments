using Characters;
using Characters.Operations;
using UnityEngine;

namespace Level.Traps
{
	public class BlossomFog : Trap
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private OperationInfos _operationInfos;

		private void Awake()
		{
			_operationInfos.Initialize();
		}

		private void OnEnable()
		{
			_operationInfos.gameObject.SetActive(true);
			_operationInfos.Run(_character);
		}
	}
}

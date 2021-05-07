using Characters;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Level.Traps
{
	public class Grinder : Trap
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _operationInfos;

		private void Awake()
		{
			_operationInfos.Initialize();
			_operationInfos.Run(_character);
		}
	}
}

using Characters;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Level.Traps
{
	public class ThiefTrap : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operationsOnDie;

		private void Awake()
		{
			_operationsOnDie.Initialize();
		}

		private void OnEnable()
		{
			StartCoroutine(_operationsOnDie.CRun(_character));
		}
	}
}

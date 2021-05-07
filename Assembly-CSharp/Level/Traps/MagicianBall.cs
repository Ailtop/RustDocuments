using Characters;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Level.Traps
{
	public class MagicianBall : Trap
	{
		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _runOperations;

		private Character _character;

		private void Awake()
		{
			_character = GetComponentInParent<Character>();
			_runOperations.Initialize();
		}

		private void OnEnable()
		{
			_runOperations.Run(_character);
		}
	}
}

using System.Collections;
using Characters;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Level
{
	public class WaveAction : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		private Wave _target;

		[SerializeField]
		private bool _alsoClear;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _operations;

		private bool _run;

		private void Awake()
		{
			_operations.Initialize();
			_target.onClear += Run;
			if (_alsoClear)
			{
				StartCoroutine(CCheckAlsoClear());
			}
		}

		private void Run()
		{
			if (!_run)
			{
				_operations.Run(_character);
				_run = true;
			}
		}

		private IEnumerator CCheckAlsoClear()
		{
			while (_target != null && _target.state != Wave.State.Stopped)
			{
				yield return null;
			}
			if (_target != null)
			{
				Run();
			}
		}
	}
}

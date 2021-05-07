using System.Collections;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters
{
	public class RiderPolymorph : MonoBehaviour
	{
		[SerializeField]
		private PolymorphBody _polymorphBody;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		private bool _initialized;

		private void OnEnable()
		{
			if (!_initialized)
			{
				_operations.Initialize();
				_initialized = true;
			}
			StartCoroutine(CStartOperation());
		}

		private IEnumerator CStartOperation()
		{
			yield return null;
			yield return _operations.CRun(_polymorphBody.character);
		}

		private void OnDisable()
		{
			_operations.StopAll();
		}
	}
}

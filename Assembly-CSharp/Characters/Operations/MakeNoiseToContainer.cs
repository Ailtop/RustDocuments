using System.Collections;
using UnityEngine;

namespace Characters.Operations
{
	public class MakeNoiseToContainer : CharacterOperation
	{
		[SerializeField]
		private Transform _container;

		[SerializeField]
		private float _noise;

		[SerializeField]
		private float _restoreTime;

		private Vector2[] _origin;

		private void Awake()
		{
			_origin = new Vector2[_container.childCount];
		}

		public override void Run(Character owner)
		{
			for (int i = 0; i < _container.childCount; i++)
			{
				Transform child = _container.GetChild(i);
				_origin[i] = child.transform.position;
				child.Translate(Random.insideUnitSphere * _noise);
			}
			if (_restoreTime > 0f)
			{
				StartCoroutine(CRestore(owner.chronometer.master));
			}
		}

		private IEnumerator CRestore(Chronometer chronometer)
		{
			yield return chronometer.WaitForSeconds(_restoreTime);
			Restore();
		}

		private void Restore()
		{
			for (int i = 0; i < _container.childCount; i++)
			{
				_container.GetChild(i).transform.position = _origin[i];
			}
		}
	}
}

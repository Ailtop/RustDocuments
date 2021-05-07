using System.Collections;
using UnityEngine;

namespace Level.ReactiveProps
{
	public class FlyGroup : MonoBehaviour
	{
		private enum StartPositionType
		{
			Consistent,
			RandomInBounds
		}

		[SerializeField]
		private StartPositionType _startPositionType;

		[SerializeField]
		private Transform _startPoint;

		[SerializeField]
		private Collider2D _startBounds;

		[SerializeField]
		private Transform _group;

		private ReactiveProp[] _reactiveProps;

		private void Start()
		{
			ReactiveProp[] array = (_reactiveProps = _group.GetComponentsInChildren<AlwaysFly>());
			array = _reactiveProps;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(false);
			}
		}

		public void Activate()
		{
			switch (_startPositionType)
			{
			case StartPositionType.Consistent:
				StartCosistentPosition();
				break;
			case StartPositionType.RandomInBounds:
				StartRandomPosition();
				break;
			}
			StartCoroutine(Spawn());
		}

		private IEnumerator Spawn()
		{
			ReactiveProp[] reactiveProps = _reactiveProps;
			foreach (ReactiveProp fly in reactiveProps)
			{
				int waitForRandomAnimationLength = Random.Range(1, 3);
				for (int i = 0; i < waitForRandomAnimationLength; i++)
				{
					yield return null;
				}
				fly.ResetPosition();
				fly.gameObject.SetActive(true);
			}
		}

		private void StartRandomPosition()
		{
			_group.transform.position = MMMaths.RandomPointWithinBounds(_startBounds.bounds);
		}

		private void StartCosistentPosition()
		{
			_group.transform.position = _startPoint.position;
		}
	}
}

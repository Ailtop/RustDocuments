using BT.SharedValues;
using UnityEngine;

namespace BT
{
	public class SetOwnerTransform : MonoBehaviour
	{
		[SerializeField]
		private BehaviourTreeRunner _runner;

		[SerializeField]
		private Transform _transform;

		private void Awake()
		{
			_runner.context.Set(Key.OwnerTransform, new SharedValue<Transform>(_transform));
		}
	}
}

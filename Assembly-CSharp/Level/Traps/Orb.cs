using Characters;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Level.Traps
{
	public class Orb : MonoBehaviour
	{
		[SerializeField]
		private Character _owner;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _onEnable;

		private float _radian;

		private void Awake()
		{
			_onEnable.Initialize();
		}

		public void Initialize(float startAngle)
		{
			_radian = startAngle;
		}

		private void OnEnable()
		{
			StartCoroutine(_onEnable.CRun(_owner));
		}

		public void MoveCenteredOn(Vector3 pivot, float radious, float amount)
		{
			Vector3 vector = pivot - _owner.transform.position;
			_radian += amount;
			_owner.movement.Move((Vector2)vector + new Vector2(Mathf.Cos(_radian), Mathf.Sin(_radian)) * radious);
		}
	}
}

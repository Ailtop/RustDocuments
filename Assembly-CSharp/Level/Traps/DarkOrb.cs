using Characters;
using Characters.Operations.Attack;
using UnityEngine;

namespace Level.Traps
{
	public class DarkOrb : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private SweepAttack _sweepAttack;

		[SerializeField]
		private Transform _pivot;

		[SerializeField]
		private float _rotateSpeed = 1f;

		[SerializeField]
		private float _radius;

		private float _rotationTime;

		private void Start()
		{
			_sweepAttack.Initialize();
			_sweepAttack.Run(_character);
		}

		private void Update()
		{
			Vector3 vector = _pivot.transform.position - _character.transform.position;
			_rotationTime += _rotateSpeed * _character.chronometer.master.deltaTime;
			_character.movement.Move((Vector2)vector + new Vector2(Mathf.Cos(_rotationTime), Mathf.Sin(_rotationTime)) * _radius);
		}
	}
}

using UnityEngine;
using UserInput;

namespace EndingCredit
{
	public class Input : MonoBehaviour
	{
		[SerializeField]
		private float _speed;

		[SerializeField]
		private float _accelerationValue;

		private float _startSpeed;

		public float speed => _speed;

		private void Start()
		{
			_startSpeed = _speed;
		}

		private void Update()
		{
			if (KeyMapper.Map.Attack.IsPressed || KeyMapper.Map.Jump.IsPressed || KeyMapper.Map.Submit.IsPressed || KeyMapper.Map.Down.IsPressed)
			{
				_speed = _startSpeed * _accelerationValue;
			}
			if (KeyMapper.Map.Attack.WasReleased || KeyMapper.Map.Jump.WasReleased || KeyMapper.Map.Submit.WasReleased || KeyMapper.Map.Down.WasReleased)
			{
				_speed = _startSpeed;
			}
		}
	}
}

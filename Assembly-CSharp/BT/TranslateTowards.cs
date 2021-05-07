using UnityEngine;

namespace BT
{
	public class TranslateTowards : Node
	{
		[SerializeField]
		private CustomFloat _speedX;

		[SerializeField]
		private CustomFloat _speedY;

		[SerializeField]
		[Range(0f, 1f)]
		private float _rightChance;

		[SerializeField]
		[Range(0f, 1f)]
		private float _upChance;

		private float _speedXValue;

		private float _speedYValue;

		protected override void OnInitialize()
		{
			_speedXValue = _speedX.value;
			_speedYValue = _speedY.value;
			base.OnInitialize();
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			Transform transform = context.Get<Transform>(Key.OwnerTransform);
			if (transform == null)
			{
				Debug.LogError("OwnerTransform is null");
				return NodeState.Fail;
			}
			float deltaTime = context.deltaTime;
			Vector2 zero = Vector2.zero;
			zero += (MMMaths.Chance(_rightChance) ? Vector2.right : Vector2.left) * _speedXValue;
			zero += (MMMaths.Chance(_upChance) ? Vector2.up : Vector2.down) * _speedYValue;
			transform.Translate(zero * deltaTime);
			return NodeState.Success;
		}
	}
}

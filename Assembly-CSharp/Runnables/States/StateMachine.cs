using UnityEngine;

namespace Runnables.States
{
	public class StateMachine : MonoBehaviour
	{
		[Header("초기값 설정")]
		[SerializeField]
		private State _state;

		public State currentState => _state;

		public void TransitTo(State state)
		{
			_state = state;
		}
	}
}

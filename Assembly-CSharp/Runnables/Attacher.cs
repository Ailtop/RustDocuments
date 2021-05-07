using CutScenes;
using UnityEngine;

namespace Runnables
{
	public class Attacher : Runnable
	{
		private enum Type
		{
			Attach,
			Detach
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		[State.Subcomponent]
		private State _state;

		public override void Run()
		{
			if (_type == Type.Attach)
			{
				_state.Attach();
			}
			else
			{
				_state.Detach();
			}
		}
	}
}

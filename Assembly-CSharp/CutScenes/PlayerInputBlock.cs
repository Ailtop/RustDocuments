using Characters.Controllers;

namespace CutScenes
{
	public class PlayerInputBlock : State
	{
		public override void Attach()
		{
			PlayerInput.blocked.Attach(State.key);
		}

		public override void Detach()
		{
			PlayerInput.blocked.Detach(State.key);
		}

		private void OnDisable()
		{
			Detach();
		}

		private void OnDestroy()
		{
			Detach();
		}
	}
}

using Characters.Actions;
using FX;

namespace UI
{
	public class ActionIcon : IconWithCooldown
	{
		public Action action { get; set; }

		protected override void Update()
		{
			if (!(action == null))
			{
				base.Update();
				base.icon.material = (action.canUse ? null : Materials.ui_grayScale);
			}
		}
	}
}

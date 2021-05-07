using System;
using UnityEditor;

namespace Runnables
{
	public abstract class UICommands : Runnable
	{
		public new class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, UICommands.types)
			{
			}
		}

		public new static readonly Type[] types = new Type[4]
		{
			typeof(OpenUIHealthBar),
			typeof(CloseAllUIHealthBar),
			typeof(CompleteUIConversation),
			typeof(SetHeadUpDisplay)
		};
	}
}

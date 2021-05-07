using Services;
using Singletons;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class SetFadeColor : Event
	{
		[SerializeField]
		private Color _color;

		public override void Run()
		{
			Singleton<Service>.Instance.fadeInOut.SetFadeColor(_color);
		}
	}
}

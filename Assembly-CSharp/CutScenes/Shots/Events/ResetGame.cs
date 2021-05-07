using Level;
using Services;
using Singletons;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class ResetGame : Event
	{
		[SerializeField]
		private Chapter.Type _type;

		public override void Run()
		{
			Singleton<Service>.Instance.levelManager.ResetGame(_type);
		}
	}
}

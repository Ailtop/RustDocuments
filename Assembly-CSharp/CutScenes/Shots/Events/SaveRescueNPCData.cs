using Data;
using Level.Npc;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class SaveRescueNPCData : Event
	{
		[SerializeField]
		private NpcType _type;

		public override void Run()
		{
			GameData.Progress.SetRescued(_type, true);
		}
	}
}

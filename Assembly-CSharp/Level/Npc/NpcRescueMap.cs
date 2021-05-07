using UnityEngine;

namespace Level.Npc
{
	public class NpcRescueMap : MonoBehaviour
	{
		[SerializeField]
		private Map _map;

		[SerializeField]
		private NpcType _npcType;

		public Map map => _map;

		public NpcType npcType => _npcType;
	}
}

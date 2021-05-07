using Characters;
using FX.Connections;
using UnityEngine;

namespace Level.Objects
{
	public class DivineShieldEffect : MonoBehaviour
	{
		[SerializeField]
		private DivineShield _divineShield;

		[SerializeField]
		private Connection _connection;

		private void Awake()
		{
			Character target = _divineShield.target;
			Vector2 endOffset = new Vector2(0f, target.collider.size.y * 0.5f);
			_connection.Connect(base.transform, Vector2.zero, target.transform, endOffset);
		}
	}
}

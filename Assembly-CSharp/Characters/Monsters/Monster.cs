using UnityEngine;

namespace Characters.Monsters
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Character))]
	public class Monster : MonoBehaviour
	{
		private const float _groundFindingRayDistance = 9f;

		[SerializeField]
		[GetComponent]
		private PoolObject _poolObject;

		[SerializeField]
		[GetComponent]
		private Character _character;

		public PoolObject poolObject => _poolObject;

		public Character character => _character;

		public Monster Summon(Vector3 position)
		{
			Monster component = _poolObject.Spawn().GetComponent<Monster>();
			component.transform.position = position;
			return component;
		}

		public void Despawn()
		{
			_poolObject.Despawn();
		}
	}
}

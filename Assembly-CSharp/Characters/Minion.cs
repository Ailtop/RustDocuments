using System.Collections;
using Characters.Actions;
using Characters.Movements;
using Characters.Player;
using UnityEngine;

namespace Characters
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Character))]
	public class Minion : MonoBehaviour
	{
		private const float _groundFindingRayDistance = 9f;

		[SerializeField]
		[GetComponent]
		private PoolObject _poolObject;

		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		private Action _onDespawn;

		public PoolObject poolObject => _poolObject;

		public Character character => _character;

		public MinionLeader owner { get; private set; }

		private void Awake()
		{
			if (_onDespawn != null)
			{
				_onDespawn.Initialize(_character);
			}
		}

		public Minion Summon(MinionLeader minionOwner, Vector3 position)
		{
			owner = minionOwner;
			Minion component = _poolObject.Spawn().GetComponent<Minion>();
			Movement movement = component.character.movement;
			if (movement != null && (movement.config.type == Movement.Config.Type.Walking || movement.config.type == Movement.Config.Type.AcceleratingWalking))
			{
				component.character.movement.verticalVelocity = 0f;
				component.character.animationController.ForceUpdate();
			}
			component.transform.position = position;
			return component;
		}

		public Minion Summon(Vector3 position)
		{
			Minion component = _poolObject.Spawn().GetComponent<Minion>();
			Movement movement = component.character.movement;
			if (movement != null && (movement.config.type == Movement.Config.Type.Walking || movement.config.type == Movement.Config.Type.AcceleratingWalking))
			{
				Vector3 vector = position;
				vector.y += 2.97f;
				RaycastHit2D raycastHit2D = Physics2D.Raycast(position, Vector2.down, 9f, Layers.terrainMask);
				if ((bool)raycastHit2D)
				{
					position = raycastHit2D.point;
				}
				component.character.animationController.ForceUpdate();
			}
			component.transform.position = position;
			return component;
		}

		public void Despawn()
		{
			if (_onDespawn == null)
			{
				_poolObject.Despawn();
			}
			else
			{
				StartCoroutine(CDespawn());
			}
		}

		private IEnumerator CDespawn()
		{
			_onDespawn.TryStart();
			while (_onDespawn.running)
			{
				yield return null;
			}
			_poolObject.Despawn();
		}
	}
}

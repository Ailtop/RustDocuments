using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Attack
{
	public sealed class InstantAttackInContainer : CharacterOperation
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(InstantAttack))]
		private InstantAttack _instantAttack;

		[SerializeField]
		private Transform _container;

		private void Awake()
		{
			_instantAttack.Initialize();
		}

		public override void Run(Character owner)
		{
			Collider2D[] componentsInChildren = _container.GetComponentsInChildren<Collider2D>();
			foreach (Collider2D range in componentsInChildren)
			{
				_instantAttack.range = range;
				_instantAttack.Run(owner);
			}
		}

		public override void Stop()
		{
			base.Stop();
			_instantAttack.Stop();
		}
	}
}

using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.Gear.Quintessences
{
	public class RunOperation : UseQuintessence
	{
		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		[SerializeField]
		private Transform _flipObject;

		protected override void Awake()
		{
			base.Awake();
			_quintessence.onEquipped += _operations.Initialize;
		}

		protected override void OnUse()
		{
			if (_flipObject != null)
			{
				if (_quintessence.owner.lookingDirection == Character.LookingDirection.Right)
				{
					_flipObject.localScale = new Vector2(1f, 1f);
				}
				else
				{
					_flipObject.localScale = new Vector2(-1f, 1f);
				}
			}
			StartCoroutine(_operations.CRun(_quintessence.owner));
		}

		private void OnDisable()
		{
			_operations.StopAll();
		}
	}
}

using System;
using Characters.Operations.Attack;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public sealed class BigBang : CharacterOperation
	{
		[Header("Gain Energy")]
		[SerializeField]
		private Transform _fireTransformContainer;

		[SerializeField]
		private float _radius = 20f;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MultipleFireProjectile))]
		private MultipleFireProjectile _multipleFireProjectile;

		public override void Initialize()
		{
			_multipleFireProjectile.Initialize();
		}

		public override void Run(Character owner)
		{
			MakeFireTransfom();
			if (owner.lookingDirection == Character.LookingDirection.Left)
			{
				_fireTransformContainer.localScale = new Vector3(-1f, 1f, 1f);
			}
			else
			{
				_fireTransformContainer.localScale = new Vector3(1f, 1f, 1f);
			}
			_multipleFireProjectile.Run(owner);
		}

		private void MakeFireTransfom()
		{
			float num = 360 / _fireTransformContainer.childCount;
			float num2 = UnityEngine.Random.Range(0f, num);
			for (int i = 0; i < _fireTransformContainer.childCount; i++)
			{
				Transform child = _fireTransformContainer.GetChild(i);
				child.transform.localPosition = new Vector2(Mathf.Cos(num2 * ((float)Math.PI / 180f)), Mathf.Sin(num2 * ((float)Math.PI / 180f))) * _radius;
				child.transform.localRotation = Quaternion.Euler(0f, 0f, 180f + num2);
				num2 += num;
			}
		}
	}
}

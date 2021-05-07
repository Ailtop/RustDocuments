using System.Collections;
using Characters.Actions;
using Characters.Operations;
using Characters.Operations.Attack;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Blast : Keyword, IAttackDamage
	{
		[SerializeField]
		private float[] _damageAmountByLevel = new float[6] { 0f, 2f, 4f, 7f, 11f, 16f };

		[Space]
		[SerializeField]
		[Information("투사체가 공격하자마자 바로 발사되면 어색하기 때문에, 공격 모션의 중간에 투사체가 발사됩니다. 공격 모션중 몇퍼센트 구간에서 발사할지를 결정합니다.", InformationAttribute.InformationType.Info, false)]
		[Range(0f, 1f)]
		private float _firingTimingOnMotion = 0.3f;

		[SerializeField]
		[Information("투사체의 크기는 캐릭터 콜라이더의 높이에 비례합니다. 그 비율을 결정합니다.", InformationAttribute.InformationType.Info, false)]
		[Range(0f, 1f)]
		private float _projectileScaleRatio = 0.75f;

		[Space]
		[Information("투사체의 크기를 랜덤하게하기 위한 파라미터입니다.", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private float _minProjectileScale = 0.9f;

		[SerializeField]
		private float _maxProjectileScale = 1.1f;

		[Space]
		[SerializeField]
		private Transform _firePosition;

		[SerializeField]
		private FireProjectile _fireProjectileOpration;

		[Space]
		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operation;

		public override Key key => Key.Blast;

		protected override IList valuesByLevel => _damageAmountByLevel;

		public float amount => _damageAmountByLevel[base.level];

		protected override void Initialize()
		{
			_operation.Initialize();
		}

		protected override void UpdateBonus()
		{
		}

		private void OnStartAction(Action action)
		{
			if (action.type == Action.Type.JumpAttack || action.type == Action.Type.BasicAttack)
			{
				StartCoroutine(CFireProjectile(base.character.motion.length * 0.3f));
			}
		}

		private IEnumerator CFireProjectile(float delay)
		{
			yield return new WaitForSeconds(_firingTimingOnMotion);
			Bounds bounds = base.character.collider.bounds;
			Vector3 center = bounds.center;
			if (base.character.lookingDirection == Character.LookingDirection.Right)
			{
				center.x = bounds.max.x;
			}
			else
			{
				center.x = bounds.min.x;
			}
			float num = bounds.size.y * _projectileScaleRatio;
			_fireProjectileOpration.scale.Set(num * _minProjectileScale, num * _maxProjectileScale);
			_firePosition.position = center;
			_operation.Run(base.character);
		}

		protected override void OnAttach()
		{
			base.character.onStartAction += OnStartAction;
		}

		protected override void OnDetach()
		{
			base.character.onStartAction -= OnStartAction;
		}
	}
}

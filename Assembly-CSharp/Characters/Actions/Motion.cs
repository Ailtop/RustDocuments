using System;
using System.Collections;
using System.Linq;
using Characters.Actions.Constraints;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class Motion : MonoBehaviour
	{
		public enum SpeedMultiplierSource
		{
			Default,
			ForceBasic,
			ForceSkill,
			ForceMovement,
			ForceCharging,
			ForceBasicAndCharging,
			ForceSkillAndCharging
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<Motion>
		{
		}

		[SerializeField]
		[GetComponentInParent(true)]
		private Action _action;

		[SerializeField]
		private CharacterAnimationController.AnimationInfo _animationInfo;

		[SerializeField]
		[Constraint.Subcomponent]
		private Constraint.Subcomponents _constraints;

		[SerializeField]
		[Information("재생 중 이동불가 여부", InformationAttribute.InformationType.Info, false)]
		private bool _blockMovement = true;

		[SerializeField]
		[Information("재생 중 방향 변경 불가 여부", InformationAttribute.InformationType.Info, false)]
		private bool _blockLook = true;

		[Information("체크시 재생이 끝난 후 마지막 프레임 상태로 유지, 반복되는 애니메이션은 반복, 지속 시간은 Length를 따라감", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private bool _stay;

		[SerializeField]
		[Information("애니메이션 재생 시간, 0이면 CharacterBody 애니메이션의 길이만큼 지속", InformationAttribute.InformationType.Info, false)]
		private float _length;

		[SerializeField]
		[Information("애니메이션 재생 속도", InformationAttribute.InformationType.Info, false)]
		private float _speed = 1f;

		[SerializeField]
		[Information("어떤 속도 스탯을 사용할지", InformationAttribute.InformationType.Info, false)]
		private SpeedMultiplierSource _speedMultiplierSource;

		[SerializeField]
		[Information("속도 스탯에 영향받는 정도", InformationAttribute.InformationType.Info, false)]
		[Range(0f, 1f)]
		private float _speedMultiplierFactor = 1f;

		private float _runSpeed;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		private OperationInfo[] _operationInfos;

		public Character owner => action.owner;

		public Action action { get; set; }

		public CharacterAnimationController.AnimationInfo animationInfo
		{
			get
			{
				return _animationInfo;
			}
			set
			{
				_animationInfo = animationInfo;
			}
		}

		public bool blockMovement => _blockMovement;

		public bool blockLook => _blockLook;

		public bool stay => _stay;

		public float length { get; set; }

		public float speed => _speed;

		public float time { get; protected set; }

		public float normalizedTime => time / length;

		public SpeedMultiplierSource speedMultiplierSource => _speedMultiplierSource;

		public float speedMultiplierFactor => _speedMultiplierFactor;

		public bool running { get; set; }

		public event System.Action onStart;

		public event System.Action onApply;

		public event System.Action onEnd;

		public event System.Action onCancel;

		public static Motion AddComponent(GameObject gameObject, Action action, bool blockLook, bool blockMovement)
		{
			Motion motion = gameObject.AddComponent<Motion>();
			motion.action = action;
			motion._blockLook = blockLook;
			motion._blockMovement = blockMovement;
			motion._constraints = new Constraint.Subcomponents();
			motion.Initialize(action);
			return motion;
		}

		private void Awake()
		{
			if (_animationInfo == null)
			{
				_animationInfo = new CharacterAnimationController.AnimationInfo();
			}
			if (_operations == null)
			{
				_operationInfos = new OperationInfo[0];
			}
			else if (_action == null)
			{
				_operationInfos = _operations.components.OrderBy((OperationInfo operation) => operation.timeToTrigger).ToArray();
			}
			else
			{
				_operationInfos = (from operation in _action.operations.Concat(_operations.components)
					orderby operation.timeToTrigger
					select operation).ToArray();
			}
			length = CaculateRealLength();
			for (int i = 0; i < _operationInfos.Length; i++)
			{
				_operationInfos[i].operation.Initialize();
			}
		}

		private void OnDisable()
		{
			CancelBehaviour();
		}

		private void StopAllOperations()
		{
			for (int i = 0; i < _operationInfos.Length; i++)
			{
				if (!_operationInfos[i].stay)
				{
					_operationInfos[i].operation.Stop();
				}
			}
		}

		public void Initialize(Action action)
		{
			this.action = action;
			for (int i = 0; i < _constraints.components.Length; i++)
			{
				_constraints.components[i].Initilaize(action);
			}
		}

		public float CaculateRealLength()
		{
			if (_length > 0f)
			{
				return _length;
			}
			float num = 0f;
			for (int i = 0; i < _animationInfo.values.Length; i++)
			{
				if (_animationInfo.values[i].clip != null && _animationInfo.values[i].clip.length > num)
				{
					num = _animationInfo.values[i].clip.length;
				}
			}
			return num;
		}

		public void StartBehaviour(float speed)
		{
			if (!running)
			{
				_runSpeed = speed;
				running = true;
				this.onStart?.Invoke();
				StartCoroutine("CRunOperations");
			}
		}

		public void EndBehaviour()
		{
			if (running)
			{
				time = length;
				StopAllOperations();
				running = false;
				StopCoroutine("CRunOperations");
				this.onEnd?.Invoke();
			}
		}

		public void CancelBehaviour()
		{
			if (running)
			{
				StopAllOperations();
				running = false;
				StopCoroutine("CRunOperations");
				this.onCancel?.Invoke();
			}
		}

		public override string ToString()
		{
			if (_animationInfo == null || _animationInfo.values == null || _animationInfo.values.Length == 0 || _animationInfo.values[0].clip == null)
			{
				return base.ToString();
			}
			return "Motion (" + _animationInfo.values[0].clip.name + ")";
		}

		public IEnumerator CWaitForEndOfRunning()
		{
			while (running)
			{
				yield return null;
			}
		}

		private IEnumerator CRunOperations()
		{
			int operationIndex = 0;
			time = 0f;
			while (true)
			{
				if (operationIndex < _operationInfos.Length && time >= _operationInfos[operationIndex].timeToTrigger)
				{
					_operationInfos[operationIndex].operation.runSpeed = _runSpeed;
					_operationInfos[operationIndex].operation.Run(owner);
					operationIndex++;
				}
				else
				{
					yield return new WaitForEndOfFrame();
					time += owner.chronometer.animation.deltaTime * _runSpeed;
				}
			}
		}

		internal bool PassConstraints()
		{
			return _constraints.components.Pass();
		}

		internal void ConsumeConstraints()
		{
			action.ConsumeConstraints();
			_constraints.components.Consume();
		}
	}
}

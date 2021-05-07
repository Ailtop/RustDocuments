using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Blessing
{
	public class Blessing : MonoBehaviour, IAbility, IAbilityInstance
	{
		[Header("임시")]
		[SerializeField]
		private string _notificationText;

		[SerializeField]
		private string _floatingTextkey;

		[SerializeField]
		private string _activatedNameKey;

		[SerializeField]
		private string _activatedChatKey;

		[SerializeField]
		private AnimationClip _holyGrail;

		[SerializeField]
		private Sprite _icon;

		[SerializeField]
		private float _duration;

		[SerializeField]
		[AbilityAttacher.Subcomponent]
		private AbilityAttacher.Subcomponents _abilityAttacher;

		private float _remainTime;

		public float duration => _duration;

		public int iconPriority { get; set; }

		public bool removeOnSwapWeapon { get; set; }

		public Character owner { get; private set; }

		public IAbility ability => this;

		public float remainTime
		{
			get
			{
				return _remainTime;
			}
			set
			{
				_remainTime = value;
			}
		}

		public bool attached { get; private set; }

		public Sprite icon => _icon;

		public float iconFillAmount => 1f - _remainTime / _duration;

		public bool iconFillInversed => false;

		public bool iconFillFlipped => false;

		public int iconStacks => 0;

		public bool expired => remainTime <= 0f;

		public AnimationClip clip => _holyGrail;

		public string activatedNameKey => _activatedNameKey;

		public string activatedChatKey => _activatedChatKey;

		public void Apply(Character target)
		{
			Vector2 vector = new Vector2(target.collider.bounds.center.x, target.collider.bounds.max.y + 0.5f);
			Singleton<Service>.Instance.floatingTextSpawner.SpawnBuff(Lingua.GetLocalizedString(_floatingTextkey), vector);
			owner = target;
			base.transform.parent = owner.transform;
			base.transform.localPosition = Vector3.zero;
			_abilityAttacher.Initialize(owner);
			owner.ability.Add(this);
		}

		public void Attach()
		{
			attached = true;
			remainTime = duration;
			_abilityAttacher.StartAttach();
		}

		public IAbilityInstance CreateInstance(Character owner)
		{
			return this;
		}

		public void Initialize()
		{
		}

		public void Refresh()
		{
			remainTime = duration;
		}

		public void UpdateTime(float deltaTime)
		{
			remainTime -= deltaTime;
		}

		public void Detach()
		{
			attached = false;
			_abilityAttacher.StopAttach();
		}
	}
}

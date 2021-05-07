using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters
{
	public class CharacterHealthBar : MonoBehaviour
	{
		public enum ActionWhenCharacterNull
		{
			Deactivate,
			ShowZero
		}

		[SerializeField]
		protected RectTransform _container;

		[SerializeField]
		protected bool _alwaysVisible;

		[SerializeField]
		private ActionWhenCharacterNull _actionWhenCharacterNull;

		[SerializeField]
		protected RectTransform _healthBar;

		[SerializeField]
		protected RectTransform _shieldBar;

		[SerializeField]
		protected RectTransform _damageLerpBar;

		[SerializeField]
		protected RectTransform _healLerpBar;

		protected Character _character;

		protected CharacterHealth _health;

		protected float _percent;

		protected float _percentWithShield;

		protected Vector3 _defaultHealthScale;

		protected Vector3 _defaultShieldScale;

		protected Vector3 _defaultDamageLerpScale;

		protected Vector3 _defaultHealLerpScale;

		protected Vector3 _healthScale = Vector3.one;

		protected Vector3 _shieldScale = Vector3.one;

		protected Vector3 _damageLerpScale = Vector3.one;

		protected Vector3 _healLerpScale = Vector3.one;

		private const float _lifeTime = 3f;

		protected float _remainLifetime;

		public bool visible
		{
			get
			{
				return base.gameObject.activeSelf;
			}
			set
			{
				base.gameObject.SetActive(value);
			}
		}

		private void Awake()
		{
			_defaultHealthScale = _healthBar.localScale;
			_defaultShieldScale = _shieldBar.localScale;
			_defaultDamageLerpScale = _damageLerpBar.localScale;
			_defaultHealLerpScale = _healLerpBar.localScale;
		}

		private void OnEnable()
		{
			_healthScale.x = 0f;
			_shieldScale.x = 0f;
			_damageLerpScale.x = 0f;
			_healLerpScale.x = 0f;
			StartCoroutine(CLerp());
		}

		public void Initialize(Character character)
		{
			_character = character;
			_health = _character.health;
			if (!_alwaysVisible)
			{
				_container.gameObject.SetActive(false);
				_remainLifetime = 3f;
				_health.onTookDamage += OnTookDamage;
			}
			else
			{
				_container.gameObject.SetActive(true);
			}
		}

		public void SetWidth(float width)
		{
			RectTransform healthBar = _healthBar;
			RectTransform shieldBar = _shieldBar;
			RectTransform damageLerpBar = _damageLerpBar;
			Vector2 vector2 = (_healLerpBar.pivot = new Vector2(0.5f, 0.5f));
			Vector2 vector4 = (damageLerpBar.pivot = vector2);
			Vector2 vector7 = (healthBar.pivot = (shieldBar.pivot = vector4));
			_container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			RectTransform healthBar2 = _healthBar;
			RectTransform shieldBar2 = _shieldBar;
			RectTransform damageLerpBar2 = _damageLerpBar;
			vector2 = (_healLerpBar.pivot = new Vector2(0f, 0.5f));
			vector4 = (damageLerpBar2.pivot = vector2);
			vector7 = (healthBar2.pivot = (shieldBar2.pivot = vector4));
		}

		private void OnTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			_container.gameObject.SetActive(true);
			_remainLifetime = 3f;
		}

		private void Update()
		{
			if (_character == null && _actionWhenCharacterNull == ActionWhenCharacterNull.Deactivate)
			{
				_container.gameObject.SetActive(false);
				return;
			}
			double num;
			double num2;
			double num3;
			if (_character == null || _character.health == null || _character.health.dead)
			{
				num = 0.0;
				num2 = ((_health == null) ? 0.0 : _health.maximumHealth);
				num3 = 0.0;
			}
			else
			{
				num = _health.currentHealth;
				num2 = _health.maximumHealth;
				num3 = _health.shield.amount;
			}
			if (num2 == 0.0)
			{
				_percent = 0f;
				_percentWithShield = 0f;
			}
			else if (num + num3 <= num2)
			{
				_percent = (float)_health.percent;
				_percentWithShield = (float)((num + num3) / num2);
			}
			else
			{
				_percent = (float)(num / (num + num3));
				_percentWithShield = 1f;
			}
			_healLerpScale.x = _percentWithShield;
			_healLerpBar.localScale = Vector3.Scale(_healLerpScale, _defaultHealLerpScale);
		}

		private IEnumerator CLerp()
		{
			while (true)
			{
				if (_percentWithShield < _damageLerpScale.x)
				{
					_damageLerpScale.x = Mathf.Lerp(_damageLerpScale.x, _percentWithShield, 0.1f);
				}
				else
				{
					_damageLerpScale.x = _shieldScale.x;
				}
				if (_percentWithShield < _shieldScale.x)
				{
					_shieldScale.x = _percentWithShield;
				}
				else
				{
					_shieldScale.x = Mathf.Lerp(_shieldScale.x, _percentWithShield, 0.1f);
				}
				_healthScale.x = _shieldScale.x - (_percentWithShield - _percent);
				if (_healthScale.x < 0f)
				{
					_healthScale.x = 0f;
				}
				_damageLerpBar.localScale = Vector3.Scale(_damageLerpScale, _defaultDamageLerpScale);
				_healthBar.localScale = Vector3.Scale(_healthScale, _defaultHealthScale);
				_shieldBar.localScale = Vector3.Scale(_shieldScale, _defaultShieldScale);
				_remainLifetime -= Time.deltaTime;
				if (!_alwaysVisible && _remainLifetime <= 0f)
				{
					_damageLerpScale.x = _shieldScale.x;
					_shieldScale.x = _percentWithShield;
					_container.gameObject.SetActive(false);
				}
				yield return null;
			}
		}
	}
}

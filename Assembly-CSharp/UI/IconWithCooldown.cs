using System.Collections;
using Characters.Cooldowns;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class IconWithCooldown : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private Image _cooldownMask;

		[SerializeField]
		private Image _streakMask;

		[SerializeField]
		private TMP_Text _remainStreaks;

		[SerializeField]
		private Animator _effect;

		private float _effectLength;

		private CooldownSerializer _cooldown;

		private CoroutineReference _cPlayEffectReference;

		public Image icon => _icon;

		public CooldownSerializer cooldown
		{
			get
			{
				return _cooldown;
			}
			set
			{
				if (_effect != null && _cooldown != value)
				{
					if (_cooldown != null)
					{
						_cooldown.onReady -= SpawnEffect;
					}
					if (value != null)
					{
						value.onReady += SpawnEffect;
					}
				}
				_cooldown = value;
			}
		}

		private void Awake()
		{
			_effect.gameObject.SetActive(true);
			_effectLength = _effect.GetCurrentAnimatorStateInfo(0).length;
			_effect.gameObject.SetActive(false);
		}

		protected virtual void Update()
		{
			if (cooldown == null)
			{
				return;
			}
			_cooldownMask.fillAmount = cooldown.remainPercent;
			if (cooldown.type == CooldownSerializer.Type.Time)
			{
				if (_remainStreaks != null)
				{
					if (cooldown.streak.remains > 0)
					{
						_remainStreaks.text = cooldown.streak.remains.ToString();
						_streakMask.fillAmount = cooldown.time.streak.remainPercent;
					}
					else if (cooldown.stacks > 1)
					{
						_remainStreaks.text = cooldown.stacks.ToString();
						_streakMask.fillAmount = 0f;
					}
					else
					{
						_remainStreaks.text = string.Empty;
						_streakMask.fillAmount = 0f;
					}
				}
			}
			else
			{
				_remainStreaks.text = string.Empty;
				_streakMask.fillAmount = 0f;
			}
		}

		private void OnDisable()
		{
			_effect.gameObject.SetActive(false);
		}

		private void SpawnEffect()
		{
			if (base.isActiveAndEnabled)
			{
				_cPlayEffectReference.Stop();
				_cPlayEffectReference = this.StartCoroutineWithReference(CPlayEffect());
			}
		}

		private IEnumerator CPlayEffect()
		{
			_effect.gameObject.SetActive(true);
			_effect.Play(0, 0, 0f);
			yield return Chronometer.global.WaitForSeconds(_effectLength);
			_effect.gameObject.SetActive(false);
		}
	}
}

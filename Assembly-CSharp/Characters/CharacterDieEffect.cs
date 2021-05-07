using FX;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters
{
	public class CharacterDieEffect : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		protected Character _character;

		[SerializeField]
		private Transform _effectSpawnPoint;

		[SerializeField]
		private EffectInfo _effect;

		[SerializeField]
		private ParticleEffectInfo _particleInfo;

		[SerializeField]
		private float _vibrationAmount;

		[SerializeField]
		private float _vibrationDuration;

		[SerializeField]
		private SoundInfo _sound;

		[SerializeField]
		[FormerlySerializedAs("_destroyCharacter")]
		private bool _deactivateCharacter = true;

		protected virtual void Awake()
		{
			_character.health.onDiedTryCatch += Spawn;
		}

		protected void Spawn()
		{
			Vector3 position = ((_effectSpawnPoint == null) ? _character.transform.position : _effectSpawnPoint.position);
			_effect.Spawn(position);
			_particleInfo?.Emit(_character.transform.position, _character.collider.bounds, _character.movement.push);
			if (_vibrationDuration > 0f)
			{
				Singleton<Service>.Instance.controllerVibation.vibration.Attach(this, _vibrationAmount, _vibrationDuration);
			}
			if (_sound != null)
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_sound, base.transform.position);
			}
			if (_deactivateCharacter)
			{
				base.gameObject.SetActive(false);
				_character.collider.enabled = false;
			}
			Object.Destroy(this);
		}

		public void Detach()
		{
			_character.health.onDiedTryCatch -= Spawn;
		}

		private void OnDestroy()
		{
			_character.health.onDiedTryCatch -= Spawn;
		}
	}
}

using Characters;
using UnityEngine;

namespace Level.Traps
{
	public class MonsterSpawner : MonoBehaviour
	{
		private enum Target
		{
			LooseSubject,
			StrangeSubject
		}

		[SerializeField]
		private Prop _prop;

		[SerializeField]
		private GameObject _destroyedBody;

		[SerializeField]
		private Character _looseSubject;

		[SerializeField]
		private Character _strangeSubject;

		[SerializeField]
		private bool _containInWave = true;

		[SerializeField]
		private Target _target;

		private void Awake()
		{
			_prop.onDestroy += SpawnCharacter;
		}

		private void SpawnCharacter()
		{
			_destroyedBody.SetActive(true);
			Character character = ((_target == Target.LooseSubject) ? _looseSubject : _strangeSubject);
			character.gameObject.SetActive(true);
			if (_containInWave)
			{
				Map.Instance.waveContainer.Attach(character);
			}
			character.collider.enabled = true;
		}
	}
}

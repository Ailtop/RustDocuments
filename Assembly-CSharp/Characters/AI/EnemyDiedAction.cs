using System.Collections;
using Characters.Actions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.AI
{
	public class EnemyDiedAction : MonoBehaviour
	{
		[SerializeField]
		private AIController _aiController;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		[FormerlySerializedAs("_deadAction")]
		private Action _dieAction;

		[SerializeField]
		private Action _diedAction;

		private bool _run;

		public Action diedAction => _dieAction;

		private void Start()
		{
			if (!(_aiController == null))
			{
				_aiController.character.health.onDiedTryCatch += OnDied;
			}
		}

		private void OnDied()
		{
			if (!_run)
			{
				_run = true;
				ActiveCharacterSprite();
				_aiController.StopAllCoroutinesWithBehaviour();
				if (_dieAction != null)
				{
					StartCoroutine(PlayDieAction());
				}
				_aiController.character.health.onDiedTryCatch -= OnDied;
			}
		}

		private void ActiveCharacterSprite()
		{
			_aiController.character.collider.enabled = false;
			_aiController.character.gameObject.SetActive(true);
			_spriteRenderer.enabled = true;
		}

		private IEnumerator PlayDieAction()
		{
			bool flag = _dieAction.TryStart();
			while (!flag)
			{
				yield return null;
				flag = _dieAction.TryStart();
			}
			if (_diedAction != null)
			{
				StartCoroutine(PlayDiedAction());
			}
		}

		private IEnumerator PlayDiedAction()
		{
			while (_dieAction.running)
			{
				yield return null;
			}
			_diedAction.TryStart();
		}
	}
}

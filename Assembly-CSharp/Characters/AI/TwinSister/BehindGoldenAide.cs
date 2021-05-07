using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.TwinSister
{
	public class BehindGoldenAide : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[Header("Intro")]
		[Space]
		[SerializeField]
		private Action _introOut;

		[SerializeField]
		private Transform _introSource;

		[SerializeField]
		private Transform _introDest;

		[Header("InGame")]
		[Space]
		[SerializeField]
		private Action _in;

		[SerializeField]
		private Transform _inStart;

		[SerializeField]
		private Transform _inDest;

		[SerializeField]
		private float _inDuration;

		[SerializeField]
		private Action _out;

		private void Start()
		{
		}

		public IEnumerator CIntroOut()
		{
			Show(_introSource.position);
			_introOut.TryStart();
			while (_introOut.running)
			{
				yield return null;
			}
			Hide();
		}

		public IEnumerator CIn()
		{
			Show(_inStart.position);
			_in.TryStart();
			Vector2 vector = _character.transform.position;
			Vector2 vector2 = _inDest.position;
			yield return MoveToDestination(vector, vector2, _in, _inDuration);
		}

		public IEnumerator COut()
		{
			_out.TryStart();
			Vector2 vector = _character.transform.position;
			Vector2 vector2 = _inStart.position;
			yield return MoveToDestination(vector, vector2, _out, _inDuration);
			Hide();
		}

		public void Hide()
		{
			_character.@base.gameObject.SetActive(false);
		}

		private void Show(Vector3 startPoint)
		{
			_character.transform.position = startPoint;
			_character.@base.gameObject.SetActive(true);
		}

		private IEnumerator MoveToDestination(Vector3 source, Vector3 dest, Action action, float duration)
		{
			float elapsed = 0f;
			Character.LookingDirection direction = _character.lookingDirection;
			while (action.running)
			{
				yield return null;
				Vector2 vector = Vector2.Lerp(source, dest, elapsed / duration);
				_character.transform.position = vector;
				elapsed += _character.chronometer.master.deltaTime;
				if (elapsed > duration)
				{
					_character.CancelAction();
					break;
				}
				if ((source - dest).magnitude < 0.1f)
				{
					_character.CancelAction();
					break;
				}
			}
			_character.transform.position = dest;
			_character.lookingDirection = direction;
		}
	}
}

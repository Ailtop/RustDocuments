using System.Collections;
using Characters;
using FX;
using Scenes;
using Services;
using Singletons;
using UnityEngine;

namespace Level.Chapter2
{
	public class Elevator : InteractiveObject
	{
		private readonly int _introHash = Animator.StringToHash("Back_Intro");

		private readonly int _frontIdleHash = Animator.StringToHash("Front_Idle");

		private readonly int _backIdleHash = Animator.StringToHash("Back_Idle");

		private readonly int _frontCloseHash = Animator.StringToHash("Front_Close");

		private readonly int _backCloseHash = Animator.StringToHash("Back_Close");

		private readonly int _frontOutroHash = Animator.StringToHash("Front_Outro");

		[SerializeField]
		private float _introLength;

		[SerializeField]
		private float _closeLength;

		[SerializeField]
		private float _outroLength;

		[SerializeField]
		private Animator _front;

		[SerializeField]
		private Animator _behind;

		[SerializeField]
		private Transform _destination;

		[SerializeField]
		private Transform _downDestination;

		[SerializeField]
		private Transform _cameraPoint;

		[SerializeField]
		private GameObject _block;

		[SerializeField]
		private SoundInfo _elevatorUp;

		[SerializeField]
		private SoundInfo _elevatorDown;

		private bool _interactable;

		private bool _used;

		private void OnEnable()
		{
			Intro();
		}

		public void Intro()
		{
			_behind.Play(_introHash);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_elevatorUp, base.transform.position);
			StartCoroutine(WaitForIntro());
		}

		private IEnumerator WaitForIntro()
		{
			yield return Chronometer.global.WaitForSeconds(_introLength);
			_front.gameObject.SetActive(true);
			_front.Play(_frontIdleHash);
			_behind.Play(_backIdleHash);
			_interactable = true;
		}

		private IEnumerator WalkToIn()
		{
			_block.SetActive(false);
			yield return MoveTo(_destination.position);
			Close();
		}

		public void Close()
		{
			_front.Play(_frontCloseHash);
			_behind.Play(_backCloseHash);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_elevatorDown, base.transform.position);
			StartCoroutine(WaitForClose());
		}

		private IEnumerator WaitForClose()
		{
			yield return Chronometer.global.WaitForSeconds(_closeLength);
			_front.Play(_frontOutroHash);
			_behind.gameObject.SetActive(false);
			Scene<GameBase>.instance.cameraController.StartTrack(_cameraPoint);
			StartCoroutine(MovePlayer());
			yield return Chronometer.global.WaitForSeconds(_outroLength);
		}

		private void LoadNextMap()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
			Singleton<Service>.Instance.levelManager.LoadNextMap();
			Character player = Singleton<Service>.Instance.levelManager.player;
			Scene<GameBase>.instance.cameraController.StartTrack(player.transform);
		}

		private IEnumerator MovePlayer()
		{
			yield return Chronometer.global.WaitForSeconds(1f);
			Singleton<Service>.Instance.levelManager.player.transform.position = _downDestination.position;
		}

		public override void InteractWith(Character character)
		{
			if (_interactable && !_used)
			{
				_used = true;
				StartCoroutine(WalkToIn());
			}
		}

		private IEnumerator MoveTo(Vector3 destination)
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			while (true)
			{
				float num = destination.x - player.transform.position.x;
				if (!(Mathf.Abs(num) < 0.1f))
				{
					Vector2 move = ((num > 0f) ? Vector2.right : Vector2.left);
					player.movement.move = move;
					yield return null;
					continue;
				}
				break;
			}
		}

		public void Run()
		{
			StartCoroutine(CMove());
		}

		private IEnumerator CMove()
		{
			while (!_interactable)
			{
				yield return null;
			}
			StartCoroutine(WalkToIn());
		}
	}
}

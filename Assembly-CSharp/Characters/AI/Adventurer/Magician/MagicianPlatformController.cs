using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.AI.Adventurer.Magician
{
	public class MagicianPlatformController : MonoBehaviour
	{
		[SerializeField]
		private Character _owner;

		[SerializeField]
		private float _interval;

		[SerializeField]
		private MagicianPlatform[] _leftPlatforms;

		[SerializeField]
		private MagicianPlatform[] _rightPlatforms;

		private HashSet<MagicianPlatform> _left;

		private HashSet<MagicianPlatform> _right;

		private void OnEnable()
		{
			_left = new HashSet<MagicianPlatform>(_leftPlatforms);
			_right = new HashSet<MagicianPlatform>(_rightPlatforms);
			foreach (MagicianPlatform item in _left)
			{
				item.Initialize(this);
			}
			foreach (MagicianPlatform item2 in _right)
			{
				item2.Initialize(this);
			}
			StartCoroutine(CRun());
		}

		private IEnumerator CRun()
		{
			while (!_owner.health.dead)
			{
				NextSpawn();
				yield return Chronometer.global.WaitForSeconds(_interval);
			}
		}

		private void NextSpawn()
		{
			MagicianPlatform magicianPlatform = _left.Random();
			magicianPlatform.Show();
			_left.Remove(magicianPlatform);
			magicianPlatform = _right.Random();
			magicianPlatform.Show();
			_right.Remove(magicianPlatform);
		}

		public void AddPlatform(MagicianPlatform platform, bool left)
		{
			if (left)
			{
				_left.Add(platform);
			}
			else
			{
				_right.Add(platform);
			}
		}
	}
}

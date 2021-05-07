using System.Collections;
using Characters;
using Characters.Operations.Attack;
using Characters.Operations.Fx;
using PhysicsUtils;
using UnityEngine;

namespace Level.Traps
{
	public class ArcherStatue : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private float _signLength = 1f;

		[SerializeField]
		private float _interval = 3f;

		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private readonly NonAllocOverlapper _overlapper = new NonAllocOverlapper(1);

		[SerializeField]
		private FireProjectile _fireProjectile;

		[SerializeField]
		private PlaySound _signSound;

		[SerializeField]
		private PlaySound _fireSound;

		[SerializeField]
		private GameObject _sign;

		private void Awake()
		{
			_fireProjectile.Initialize();
			_signSound.Initialize();
			_fireSound.Initialize();
			StartCoroutine(CAttack());
		}

		private IEnumerator CAttack()
		{
			while (true)
			{
				yield return Chronometer.global.WaitForSeconds(0.1f);
				FindPlayer();
				if (_overlapper.results.Count != 0)
				{
					_signSound.Run(_character);
					_sign.SetActive(true);
					yield return Chronometer.global.WaitForSeconds(_signLength);
					_sign.SetActive(false);
					_fireProjectile.Run(_character);
					_fireSound.Run(_character);
					yield return Chronometer.global.WaitForSeconds(_interval);
				}
			}
		}

		private void FindPlayer()
		{
			_range.enabled = true;
			_overlapper.contactFilter.SetLayerMask(512);
			_overlapper.OverlapCollider(_range);
			_range.enabled = false;
		}
	}
}

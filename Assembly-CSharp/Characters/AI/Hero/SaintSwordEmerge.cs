using System.Collections;
using UnityEngine;

namespace Characters.AI.Hero
{
	public class SaintSwordEmerge : MonoBehaviour
	{
		[SerializeField]
		private Transform _body;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private Transform _sourceTransfom;

		[SerializeField]
		private Transform _destTransfom;

		public void Emerge(Character owner)
		{
			StartCoroutine(CMove(owner));
		}

		private IEnumerator CMove(Character owner)
		{
			float elapsed = 0f;
			Vector3 source = _sourceTransfom.position;
			Vector3 dest = _destTransfom.position;
			base.transform.position = source;
			Show();
			while (elapsed < _duration)
			{
				yield return null;
				elapsed += owner.chronometer.master.deltaTime;
				base.transform.position = Vector2.Lerp(source, dest, elapsed / _duration);
			}
			base.transform.position = dest;
		}

		private void Show()
		{
			_body.gameObject.SetActive(true);
		}

		public void Hide()
		{
			_body.gameObject.SetActive(false);
		}
	}
}

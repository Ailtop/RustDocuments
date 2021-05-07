using System.Collections;
using UnityEngine;

namespace Characters.Operations
{
	public class LerpCollider : CharacterOperation
	{
		[SerializeField]
		private BoxCollider2D _source;

		[SerializeField]
		private BoxCollider2D _dest;

		[SerializeField]
		private Curve _sourceToDestCurve;

		[SerializeField]
		[FrameTime]
		private float _term;

		[SerializeField]
		private Curve _destToSourceCurve;

		[SerializeField]
		private bool _bounce;

		private CoroutineReference _coroutineReference;

		private Vector2 _originSize;

		private Vector2 _originOffset;

		private void Awake()
		{
			_originSize = _source.size;
			_originOffset = _source.offset;
		}

		public override void Run(Character owner)
		{
			_coroutineReference = this.StartCoroutineWithReference(CRun(owner));
		}

		private IEnumerator CRun(Character owner)
		{
			Vector2 source = _source.size;
			Vector2 sourceOffset = _source.offset;
			Vector2 dest = _dest.size;
			Vector2 destOffset = _dest.offset;
			yield return CLerp(owner.chronometer.master, _sourceToDestCurve, source, sourceOffset, dest, destOffset);
			if (_bounce)
			{
				yield return owner.chronometer.master.WaitForSeconds(_term);
				yield return CLerp(owner.chronometer.master, _destToSourceCurve, dest, destOffset, source, sourceOffset);
			}
		}

		private IEnumerator CLerp(Chronometer chronometer, Curve curve, Vector2 source, Vector2 sourceOffset, Vector2 dest, Vector2 destOffset)
		{
			for (float elapsed = 0f; elapsed < curve.duration; elapsed += chronometer.deltaTime)
			{
				yield return null;
				_source.size = Vector2.Lerp(source, dest, curve.Evaluate(elapsed));
				_source.offset = Vector2.Lerp(sourceOffset, destOffset, curve.Evaluate(elapsed));
			}
			_source.size = dest;
			_source.offset = destOffset;
		}

		public override void Stop()
		{
			_source.size = _originSize;
			_source.offset = _originOffset;
			_coroutineReference.Stop();
		}
	}
}

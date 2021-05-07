using System.Collections;
using FX;
using UnityEngine;

public sealed class ReusableChronoSpriteEffect : MonoBehaviour, IUseChronometer, IDelayable, IPoolObjectCopiable<ReusableChronoSpriteEffect>
{
	[SerializeField]
	private PoolObject _reusable;

	[SerializeField]
	[GetComponent]
	private SpriteRenderer _renderer;

	[GetComponent]
	[SerializeField]
	private Animator _animator;

	private MaterialPropertyBlock _propertyBlock;

	private CoroutineReference _cPlayReference;

	public PoolObject reusable => _reusable;

	public SpriteRenderer renderer => _renderer;

	public Animator animator => _animator;

	public ChronometerBase chronometer { get; set; }

	public float delay { get; set; }

	public int hue
	{
		set
		{
			renderer.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetInt(EffectInfo.huePropertyID, value);
			renderer.SetPropertyBlock(_propertyBlock);
		}
	}

	private void Awake()
	{
		_propertyBlock = new MaterialPropertyBlock();
		_reusable.onDespawn += OnDespawn;
	}

	private void OnDespawn()
	{
		_cPlayReference.Stop();
	}

	public void Copy(ReusableChronoSpriteEffect to)
	{
		to.animator.runtimeAnimatorController = animator.runtimeAnimatorController;
		to.animator.speed = animator.speed;
		to.animator.updateMode = animator.updateMode;
		to.animator.cullingMode = animator.cullingMode;
		to.renderer.sprite = renderer.sprite;
		to.renderer.color = renderer.color;
		to.renderer.flipX = renderer.flipX;
		to.renderer.flipY = renderer.flipY;
		to.renderer.drawMode = renderer.drawMode;
		to.renderer.sortingLayerID = renderer.sortingLayerID;
		to.renderer.sortingOrder = renderer.sortingOrder;
		to.renderer.maskInteraction = renderer.maskInteraction;
		to.renderer.spriteSortPoint = renderer.spriteSortPoint;
		to.renderer.renderingLayerMask = renderer.renderingLayerMask;
	}

	public void Play(float delay, float duration, bool loop, AnimationCurve fadeOutCurve, float fadeOutDuration)
	{
		_cPlayReference = CoroutineProxy.instance.StartCoroutineWithReference(CPlay(delay, duration, loop, fadeOutCurve, fadeOutDuration));
	}

	private IEnumerator CPlay(float delay, float duration, bool loop, AnimationCurve fadeOutCurve, float fadeOutDuration)
	{
		float remain2 = delay;
		while (remain2 > float.Epsilon)
		{
			_renderer.enabled = false;
			yield return null;
			float num = chronometer.DeltaTime();
			remain2 -= num;
		}
		_renderer.enabled = true;
		if (_animator.runtimeAnimatorController != null)
		{
			if (!_animator.enabled)
			{
				_animator.enabled = true;
			}
			_animator.Play(0, 0, 0f);
		}
		if (loop)
		{
			yield break;
		}
		if (duration == 0f)
		{
			duration = _animator.GetCurrentAnimatorStateInfo(0).length;
		}
		duration -= fadeOutDuration;
		if (duration <= 0f)
		{
			Debug.LogError("Duration - Fade out duration이 0이하입니다.");
			_cPlayReference.Clear();
			_reusable.Despawn();
			yield break;
		}
		remain2 += duration;
		_animator.enabled = false;
		while (remain2 > float.Epsilon)
		{
			yield return null;
			float num2 = chronometer.DeltaTime();
			_animator.Update(num2);
			remain2 -= num2;
		}
		if (fadeOutDuration > 0f)
		{
			yield return CFadeOut(0f - remain2, fadeOutDuration, fadeOutCurve);
		}
		_animator.enabled = true;
		_cPlayReference.Clear();
		_reusable.Despawn();
	}

	private IEnumerator CFadeOut(float time, float duration, AnimationCurve fadeOutCurve)
	{
		Color color = _renderer.color;
		float alpha = color.a;
		while (time < duration)
		{
			yield return null;
			float num = chronometer.DeltaTime();
			time += num;
			_animator.Update(num);
			color.a = alpha * (1f - fadeOutCurve.Evaluate(time / duration));
			_renderer.color = color;
		}
	}
}

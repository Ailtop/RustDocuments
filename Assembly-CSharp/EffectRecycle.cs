using UnityEngine.Serialization;

public class EffectRecycle : BaseMonoBehaviour, IClientComponent, IRagdollInhert, IEffectRecycle
{
	public enum PlayMode
	{
		Once,
		Looped
	}

	public enum ParentDestroyBehaviour
	{
		Detach,
		Destroy,
		DetachWaitDestroy
	}

	[ReadOnly]
	[FormerlySerializedAs("lifeTime")]
	public float detachTime;

	[FormerlySerializedAs("lifeTime")]
	[ReadOnly]
	public float recycleTime;

	public PlayMode playMode;

	public ParentDestroyBehaviour onParentDestroyed;
}

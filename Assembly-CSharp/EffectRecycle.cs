using UnityEngine.Serialization;

public class EffectRecycle : BaseMonoBehaviour, IClientComponent, IRagdollInhert, IEffectRecycle
{
	public enum PlayMode
	{
		Once = 0,
		Looped = 1
	}

	public enum ParentDestroyBehaviour
	{
		Detach = 0,
		Destroy = 1,
		DetachWaitDestroy = 2
	}

	[FormerlySerializedAs("lifeTime")]
	[ReadOnly]
	public float detachTime;

	[FormerlySerializedAs("lifeTime")]
	[ReadOnly]
	public float recycleTime;

	public PlayMode playMode;

	public ParentDestroyBehaviour onParentDestroyed;
}

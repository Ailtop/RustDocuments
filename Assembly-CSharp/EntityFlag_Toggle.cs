using UnityEngine;
using UnityEngine.Events;

public class EntityFlag_Toggle : EntityComponent<BaseEntity>, IOnPostNetworkUpdate, IOnSendNetworkUpdate, IPrefabPreProcess
{
	private enum FlagCheck
	{
		All = 0,
		Any = 1
	}

	public bool runClientside = true;

	public bool runServerside = true;

	public BaseEntity.Flags flag;

	[SerializeField]
	[Tooltip("If multiple flags are defined in 'flag', should they all be set, or any?")]
	private FlagCheck flagCheck;

	[Tooltip("Specify any flags that must NOT be on for this toggle to be on")]
	[SerializeField]
	private BaseEntity.Flags notFlag;

	[SerializeField]
	private UnityEvent onFlagEnabled = new UnityEvent();

	[SerializeField]
	private UnityEvent onFlagDisabled = new UnityEvent();

	internal bool hasRunOnce;

	internal bool lastToggleOn;

	protected void OnDisable()
	{
		hasRunOnce = false;
		lastToggleOn = false;
	}

	public void DoUpdate(BaseEntity entity)
	{
		bool flag = ((flagCheck == FlagCheck.All) ? entity.HasFlag(this.flag) : entity.HasAny(this.flag));
		if (entity.HasAny(notFlag))
		{
			flag = false;
		}
		if (!hasRunOnce || flag != lastToggleOn)
		{
			hasRunOnce = true;
			lastToggleOn = flag;
			if (flag)
			{
				onFlagEnabled.Invoke();
			}
			else
			{
				onFlagDisabled.Invoke();
			}
			OnStateToggled(flag);
		}
	}

	protected virtual void OnStateToggled(bool state)
	{
	}

	public void OnPostNetworkUpdate(BaseEntity entity)
	{
		if (!(base.baseEntity != entity) && runClientside)
		{
			DoUpdate(entity);
		}
	}

	public void OnSendNetworkUpdate(BaseEntity entity)
	{
		if (runServerside)
		{
			DoUpdate(entity);
		}
	}

	public void PreProcess(IPrefabProcessor process, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if ((!clientside || !runClientside) && (!serverside || !runServerside))
		{
			process.RemoveComponent(this);
		}
	}
}

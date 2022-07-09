using Rust;
using UnityEngine;

public class TriggerParent : TriggerBase, IServerComponent
{
	[Tooltip("Deparent if the parented entity clips into an obstacle")]
	[SerializeField]
	public bool doClippingCheck;

	[Tooltip("If deparenting via clipping, this will be used (if assigned) to also move the entity to a valid dismount position")]
	public BaseMountable associatedMountable;

	[Tooltip("Needed if the player might dismount inside the trigger and the trigger might be moving. Being mounting inside the trigger lets them dismount in local trigger-space, which means client and server will sync up.Otherwise the client/server delay can have them dismounting into invalid space.")]
	public bool parentMountedPlayers;

	[Tooltip("Sleepers don't have all the checks (e.g. clipping) that awake players get. If that might be a problem,sleeper parenting can be disabled. You'll need an associatedMountable though so that the sleeper can be dismounted.")]
	public bool parentSleepers = true;

	public bool ParentNPCPlayers;

	[Tooltip("If the player is already parented to something else, they'll switch over to another parent only if this is true")]
	public bool overrideOtherTriggers;

	public const int CLIP_CHECK_MASK = 1218511105;

	private BasePlayer killPlayerTemp;

	public override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		return baseEntity.gameObject;
	}

	public override void OnEntityEnter(BaseEntity ent)
	{
		if (!(ent is NPCPlayer) || ParentNPCPlayers)
		{
			if (ShouldParent(ent))
			{
				Parent(ent);
			}
			base.OnEntityEnter(ent);
			if (entityContents != null && entityContents.Count == 1)
			{
				InvokeRepeating(OnTick, 0f, 0f);
			}
		}
	}

	public override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if (entityContents == null || entityContents.Count == 0)
		{
			CancelInvoke(OnTick);
		}
		BasePlayer basePlayer = ent.ToPlayer();
		if (!parentSleepers || !(basePlayer != null) || !basePlayer.IsSleeping())
		{
			Unparent(ent);
		}
	}

	public virtual bool ShouldParent(BaseEntity ent, bool bypassOtherTriggerCheck = false)
	{
		if (!ent.canTriggerParent)
		{
			return false;
		}
		if (!bypassOtherTriggerCheck)
		{
			BaseEntity parentEntity = ent.GetParentEntity();
			if (!overrideOtherTriggers && BaseNetworkableEx.IsValid(parentEntity) && parentEntity != GameObjectEx.ToBaseEntity(base.gameObject))
			{
				return false;
			}
		}
		if (ent.FindTrigger<TriggerParentExclusion>() != null)
		{
			return false;
		}
		if (doClippingCheck && IsClipping(ent))
		{
			return false;
		}
		if (!parentMountedPlayers || !parentSleepers)
		{
			BasePlayer basePlayer = ent.ToPlayer();
			if (basePlayer != null)
			{
				if (!parentMountedPlayers && basePlayer.isMounted)
				{
					return false;
				}
				if (!parentSleepers && basePlayer.IsSleeping())
				{
					return false;
				}
			}
		}
		return true;
	}

	protected void Parent(BaseEntity ent)
	{
		if (!(ent.GetParentEntity() == GameObjectEx.ToBaseEntity(base.gameObject)))
		{
			ent.SetParent(GameObjectEx.ToBaseEntity(base.gameObject), worldPositionStays: true, sendImmediate: true);
		}
	}

	protected void Unparent(BaseEntity ent)
	{
		if (ent.GetParentEntity() != GameObjectEx.ToBaseEntity(base.gameObject))
		{
			return;
		}
		if (BaseNetworkableEx.IsValid(ent) && !ent.IsDestroyed)
		{
			TriggerParent triggerParent = ent.FindSuitableParent();
			if (triggerParent != null && BaseNetworkableEx.IsValid(GameObjectEx.ToBaseEntity(triggerParent.gameObject)))
			{
				triggerParent.Parent(ent);
				return;
			}
		}
		ent.SetParent(null, worldPositionStays: true, sendImmediate: true);
		BasePlayer basePlayer = ent.ToPlayer();
		if (!(basePlayer != null))
		{
			return;
		}
		basePlayer.PauseFlyHackDetection(5f);
		basePlayer.PauseSpeedHackDetection(5f);
		basePlayer.PauseVehicleNoClipDetection(5f);
		if (associatedMountable != null && ((doClippingCheck && IsClipping(ent)) || basePlayer.IsSleeping()))
		{
			if (associatedMountable.GetDismountPosition(basePlayer, out var res))
			{
				basePlayer.MovePosition(res);
				basePlayer.SendNetworkUpdateImmediate();
				basePlayer.ClientRPCPlayer(null, basePlayer, "ForcePositionTo", res);
			}
			else
			{
				killPlayerTemp = basePlayer;
				Invoke(KillPlayerDelayed, 0f);
			}
		}
	}

	private void KillPlayerDelayed()
	{
		if (BaseNetworkableEx.IsValid(killPlayerTemp) && !killPlayerTemp.IsDead())
		{
			killPlayerTemp.Hurt(1000f, DamageType.Suicide, killPlayerTemp, useProtection: false);
		}
		killPlayerTemp = null;
	}

	private void OnTick()
	{
		if (entityContents == null)
		{
			return;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(base.gameObject);
		if (!BaseNetworkableEx.IsValid(baseEntity) || baseEntity.IsDestroyed)
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if (BaseNetworkableEx.IsValid(entityContent) && !entityContent.IsDestroyed)
			{
				if (ShouldParent(entityContent))
				{
					Parent(entityContent);
				}
				else
				{
					Unparent(entityContent);
				}
			}
		}
	}

	protected virtual bool IsClipping(BaseEntity ent)
	{
		return GamePhysics.CheckOBB(ent.WorldSpaceBounds(), 1218511105, QueryTriggerInteraction.Ignore);
	}
}

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

	public bool ParentNPCPlayers;

	[Tooltip("If the player is already parented to something else, they'll switch over to another parent only if this is true")]
	public bool overrideOtherTriggers;

	public const int CLIP_CHECK_MASK = 1218511105;

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
		if (!(basePlayer != null) || !basePlayer.IsSleeping())
		{
			Unparent(ent);
		}
	}

	protected virtual bool ShouldParent(BaseEntity ent)
	{
		BaseEntity parentEntity = ent.GetParentEntity();
		if (!overrideOtherTriggers && BaseEntityEx.IsValid(parentEntity) && parentEntity != GameObjectEx.ToBaseEntity(base.gameObject))
		{
			return false;
		}
		if (ent.FindTrigger<TriggerParentExclusion>() != null)
		{
			return false;
		}
		if (doClippingCheck && IsClipping(ent))
		{
			return false;
		}
		if (!parentMountedPlayers)
		{
			BasePlayer basePlayer = ent.ToPlayer();
			if (basePlayer != null && basePlayer.isMounted)
			{
				return false;
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
		if (!(ent.GetParentEntity() != GameObjectEx.ToBaseEntity(base.gameObject)))
		{
			ent.SetParent(null, worldPositionStays: true, sendImmediate: true);
			BasePlayer basePlayer = ent.ToPlayer();
			if (basePlayer != null)
			{
				basePlayer.PauseFlyHackDetection(5f);
				basePlayer.PauseSpeedHackDetection(5f);
				basePlayer.PauseVehicleNoClipDetection(5f);
			}
			if (associatedMountable != null && doClippingCheck && IsClipping(ent) && ent is BasePlayer basePlayer2 && associatedMountable.GetDismountPosition(basePlayer2, out var res))
			{
				basePlayer2.MovePosition(res);
				basePlayer2.SendNetworkUpdateImmediate();
				basePlayer2.ClientRPCPlayer(null, basePlayer2, "ForcePositionTo", res);
			}
		}
	}

	private void OnTick()
	{
		if (entityContents == null)
		{
			return;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(base.gameObject);
		if (!BaseEntityEx.IsValid(baseEntity) || baseEntity.IsDestroyed)
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if (BaseEntityEx.IsValid(entityContent) && !entityContent.IsDestroyed)
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

	private bool IsClipping(BaseEntity ent)
	{
		return GamePhysics.CheckOBB(ent.WorldSpaceBounds(), 1218511105, QueryTriggerInteraction.Ignore);
	}
}

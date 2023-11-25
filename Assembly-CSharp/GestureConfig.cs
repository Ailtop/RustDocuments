using Oxide.Core;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Gestures/Gesture Config")]
public class GestureConfig : ScriptableObject
{
	public enum GestureType
	{
		Player = 0,
		NPC = 1,
		Cinematic = 2
	}

	public enum PlayerModelLayer
	{
		UpperBody = 3,
		FullBody = 4
	}

	public enum MovementCapabilities
	{
		FullMovement = 0,
		NoMovement = 1
	}

	public enum AnimationType
	{
		OneShot = 0,
		Loop = 1
	}

	public enum ViewMode
	{
		FirstPerson = 0,
		ThirdPerson = 1
	}

	public enum GestureActionType
	{
		None = 0,
		ShowNameTag = 1,
		DanceAchievement = 2
	}

	[ReadOnly]
	public uint gestureId;

	public string gestureCommand;

	public string convarName;

	public Translate.Phrase gestureName;

	public Sprite icon;

	public int order = 1;

	public float duration = 1.5f;

	public bool canCancel = true;

	[Header("Player model setup")]
	public PlayerModelLayer playerModelLayer = PlayerModelLayer.UpperBody;

	public GestureType gestureType;

	public bool hideHeldEntity = true;

	public bool canDuckDuringGesture;

	public bool hideInWheel;

	public bool hasViewmodelAnimation = true;

	public MovementCapabilities movementMode;

	public AnimationType animationType;

	public BasePlayer.CameraMode viewMode;

	public bool useRootMotion;

	[Header("Ownership")]
	public GestureActionType actionType;

	public bool forceUnlock;

	public SteamDLCItem dlcItem;

	public SteamInventoryItem inventoryItem;

	public bool IsOwnedBy(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseGesture", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (forceUnlock)
		{
			return true;
		}
		if (gestureType == GestureType.NPC)
		{
			return player.IsNpc;
		}
		if (gestureType == GestureType.Cinematic)
		{
			return player.IsAdmin;
		}
		if (dlcItem != null && dlcItem.CanUse(player))
		{
			return true;
		}
		if (inventoryItem != null && player.blueprints.steamInventory.HasItem(inventoryItem.id))
		{
			return true;
		}
		return false;
	}

	public bool CanBeUsedBy(BasePlayer player)
	{
		if (player.isMounted)
		{
			if (playerModelLayer == PlayerModelLayer.FullBody)
			{
				return false;
			}
			if (player.GetMounted().allowedGestures == BaseMountable.MountGestureType.None)
			{
				return false;
			}
		}
		if (player.IsSwimming() && playerModelLayer == PlayerModelLayer.FullBody)
		{
			return false;
		}
		if (playerModelLayer == PlayerModelLayer.FullBody && player.modelState.ducked)
		{
			return false;
		}
		return true;
	}
}

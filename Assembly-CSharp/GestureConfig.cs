using Oxide.Core;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Gestures/Gesture Config")]
public class GestureConfig : ScriptableObject
{
	public enum PlayerModelLayer
	{
		UpperBody = 3,
		FullBody
	}

	public enum MovementCapabilities
	{
		FullMovement,
		NoMovement
	}

	public enum AnimationType
	{
		OneShot,
		Loop
	}

	public enum ViewMode
	{
		FirstPerson,
		ThirdPerson
	}

	public enum GestureActionType
	{
		None,
		ShowNameTag,
		DanceAchievement
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

	public MovementCapabilities movementMode;

	public AnimationType animationType;

	public BasePlayer.CameraMode viewMode;

	public bool useRootMotion;

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
		return true;
	}
}

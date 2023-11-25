using System;
using System.Collections.Generic;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class PhoneController : EntityComponent<BaseEntity>
{
	public int PhoneNumber;

	public string PhoneName;

	public bool CanModifyPhoneName = true;

	public bool CanSaveNumbers = true;

	public bool RequirePower = true;

	public bool RequireParent;

	public float CallWaitingTime = 12f;

	public bool AppendGridToName;

	public bool IsMobile;

	public bool CanSaveVoicemail;

	public GameObjectRef PhoneDialog;

	public VoiceProcessor VProcessor;

	public PreloadedCassetteContent PreloadedContent;

	public SoundDefinition DialToneSfx;

	public SoundDefinition RingingSfx;

	public SoundDefinition ErrorSfx;

	public SoundDefinition CallIncomingWhileBusySfx;

	public SoundDefinition PickupHandsetSfx;

	public SoundDefinition PutDownHandsetSfx;

	public SoundDefinition FailedWrongNumber;

	public SoundDefinition FailedNoAnswer;

	public SoundDefinition FailedNetworkBusy;

	public SoundDefinition FailedEngaged;

	public SoundDefinition FailedRemoteHangUp;

	public SoundDefinition FailedSelfHangUp;

	public Light RingingLight;

	public float RingingLightFrequency = 0.4f;

	public AudioSource answeringMachineSound;

	public EntityRef currentPlayerRef;

	public List<ProtoBuf.VoicemailEntry> savedVoicemail;

	public PhoneController activeCallTo;

	public int MaxVoicemailSlots
	{
		get
		{
			if (!(cachedCassette != null))
			{
				return 0;
			}
			return cachedCassette.MaximumVoicemailSlots;
		}
	}

	public BasePlayer currentPlayer
	{
		get
		{
			if (currentPlayerRef.IsValid(isServer))
			{
				return currentPlayerRef.Get(isServer).ToPlayer();
			}
			return null;
		}
		set
		{
			currentPlayerRef.Set(value);
		}
	}

	private bool isServer
	{
		get
		{
			if (base.baseEntity != null)
			{
				return base.baseEntity.isServer;
			}
			return false;
		}
	}

	public int lastDialedNumber { get; set; }

	public PhoneDirectory savedNumbers { get; set; }

	public BaseEntity ParentEntity => base.baseEntity;

	private Cassette cachedCassette
	{
		get
		{
			if (!(base.baseEntity != null) || !(base.baseEntity is Telephone telephone))
			{
				return null;
			}
			return telephone.cachedCassette;
		}
	}

	public Telephone.CallState serverState { get; set; }

	public uint AnsweringMessageId
	{
		get
		{
			if (!(base.baseEntity is Telephone telephone))
			{
				return 0u;
			}
			return telephone.AnsweringMessageId;
		}
	}

	private bool IsPowered()
	{
		if (base.baseEntity != null && base.baseEntity is IOEntity iOEntity)
		{
			return iOEntity.IsPowered();
		}
		return false;
	}

	public bool IsSavedContactValid(string contactName, int contactNumber)
	{
		if (contactName.Length <= 0 || contactName.Length > 20)
		{
			return false;
		}
		if (contactNumber < 10000000 || contactNumber >= 100000000)
		{
			return false;
		}
		return true;
	}

	public void OnFlagsChanged(BaseEntity.Flags old, BaseEntity.Flags next)
	{
	}

	public void ServerInit()
	{
		if (PhoneNumber == 0 && !Rust.Application.isLoadingSave)
		{
			PhoneNumber = TelephoneManager.GetUnusedTelephoneNumber();
			if (AppendGridToName & !string.IsNullOrEmpty(PhoneName))
			{
				PhoneName = PhoneName + " " + PositionToGridCoord(base.transform.position);
			}
			TelephoneManager.RegisterTelephone(this);
		}
	}

	public void PostServerLoad()
	{
		currentPlayer = null;
		base.baseEntity.SetFlag(BaseEntity.Flags.Busy, b: false);
		TelephoneManager.RegisterTelephone(this);
	}

	public void DoServerDestroy()
	{
		TelephoneManager.DeregisterTelephone(this);
	}

	public void ClearCurrentUser(BaseEntity.RPCMessage msg)
	{
		ClearCurrentUser();
	}

	public void ClearCurrentUser()
	{
		if (currentPlayer != null)
		{
			currentPlayer.SetActiveTelephone(null);
			currentPlayer = null;
		}
		base.baseEntity.SetFlag(BaseEntity.Flags.Busy, b: false);
	}

	public void SetCurrentUser(BaseEntity.RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(currentPlayer == player))
		{
			UpdateServerPlayer(player);
			if (serverState == Telephone.CallState.Dialing || serverState == Telephone.CallState.Ringing || serverState == Telephone.CallState.InProcess)
			{
				ServerHangUp(default(BaseEntity.RPCMessage));
			}
		}
	}

	private void UpdateServerPlayer(BasePlayer newPlayer)
	{
		if (!(currentPlayer == newPlayer))
		{
			if (currentPlayer != null)
			{
				currentPlayer.SetActiveTelephone(null);
			}
			currentPlayer = newPlayer;
			base.baseEntity.SetFlag(BaseEntity.Flags.Busy, currentPlayer != null);
			if (currentPlayer != null)
			{
				currentPlayer.SetActiveTelephone(this);
			}
		}
	}

	public void InitiateCall(BaseEntity.RPCMessage msg)
	{
		if (!(msg.player != currentPlayer))
		{
			int number = msg.read.Int32();
			CallPhone(number);
		}
	}

	public void CallPhone(int number)
	{
		if (number == PhoneNumber)
		{
			OnDialFailed(Telephone.DialFailReason.CallSelf);
			return;
		}
		if (TelephoneManager.GetCurrentActiveCalls() + 1 > TelephoneManager.MaxConcurrentCalls)
		{
			OnDialFailed(Telephone.DialFailReason.NetworkBusy);
			return;
		}
		PhoneController telephone = TelephoneManager.GetTelephone(number);
		if (telephone != null)
		{
			if (Interface.CallHook("OnPhoneDial", this, telephone, currentPlayer) == null)
			{
				if (telephone.serverState == Telephone.CallState.Idle && telephone.CanReceiveCall())
				{
					SetPhoneState(Telephone.CallState.Dialing);
					lastDialedNumber = number;
					activeCallTo = telephone;
					activeCallTo.ReceiveCallFrom(this);
				}
				else
				{
					OnDialFailed(Telephone.DialFailReason.Engaged);
					telephone.OnIncomingCallWhileBusy();
				}
			}
		}
		else
		{
			OnDialFailed(Telephone.DialFailReason.WrongNumber);
		}
	}

	private bool CanReceiveCall()
	{
		object obj = Interface.CallHook("CanReceiveCall", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (RequirePower && !IsPowered())
		{
			return false;
		}
		if (RequireParent && !base.baseEntity.HasParent())
		{
			return false;
		}
		return true;
	}

	public void AnswerPhone(BaseEntity.RPCMessage msg)
	{
		if (IsInvoking(TimeOutDialing))
		{
			CancelInvoke(TimeOutDialing);
		}
		if (!(activeCallTo == null))
		{
			BasePlayer player = msg.player;
			if (Interface.CallHook("OnPhoneAnswer", this, activeCallTo) == null)
			{
				UpdateServerPlayer(player);
				BeginCall();
				activeCallTo.BeginCall();
				Interface.CallHook("OnPhoneAnswered", this, activeCallTo);
			}
		}
	}

	public void ReceiveCallFrom(PhoneController t)
	{
		activeCallTo = t;
		SetPhoneState(Telephone.CallState.Ringing);
		Invoke(TimeOutDialing, CallWaitingTime);
	}

	private void TimeOutDialing()
	{
		if (Interface.CallHook("OnPhoneDialTimeout", activeCallTo, this, activeCallTo.currentPlayer) == null)
		{
			if (activeCallTo != null)
			{
				activeCallTo.ServerPlayAnsweringMessage(this);
			}
			SetPhoneState(Telephone.CallState.Idle);
			Interface.CallHook("OnPhoneDialTimedOut", activeCallTo, this, activeCallTo.currentPlayer);
		}
	}

	public void OnDialFailed(Telephone.DialFailReason reason)
	{
		if (Interface.CallHook("OnPhoneDialFail", this, reason, currentPlayer) == null)
		{
			SetPhoneState(Telephone.CallState.Idle);
			base.baseEntity.ClientRPC(null, "ClientOnDialFailed", (int)reason);
			activeCallTo = null;
			if (IsInvoking(TimeOutCall))
			{
				CancelInvoke(TimeOutCall);
			}
			if (IsInvoking(TriggerTimeOut))
			{
				CancelInvoke(TriggerTimeOut);
			}
			if (IsInvoking(TimeOutDialing))
			{
				CancelInvoke(TimeOutDialing);
			}
			Interface.CallHook("OnPhoneDialFailed", this, reason, currentPlayer);
		}
	}

	public void ServerPlayAnsweringMessage(PhoneController fromPhone)
	{
		NetworkableId arg = default(NetworkableId);
		uint num = 0u;
		uint arg2 = 0u;
		if (activeCallTo != null && activeCallTo.cachedCassette != null)
		{
			arg = activeCallTo.cachedCassette.net.ID;
			num = activeCallTo.cachedCassette.AudioId;
			if (num == 0)
			{
				arg2 = activeCallTo.cachedCassette.PreloadContent.GetSoundContentId(activeCallTo.cachedCassette.PreloadedAudio);
			}
		}
		if (arg.IsValid)
		{
			base.baseEntity.ClientRPC(null, "ClientPlayAnsweringMessage", arg, num, arg2, fromPhone.HasVoicemailSlot() ? 1 : 0, activeCallTo.PhoneNumber);
			Invoke(TriggerTimeOut, activeCallTo.cachedCassette.MaxCassetteLength);
		}
		else
		{
			OnDialFailed(Telephone.DialFailReason.TimedOut);
		}
	}

	private void TriggerTimeOut()
	{
		OnDialFailed(Telephone.DialFailReason.TimedOut);
	}

	public void SetPhoneStateWithPlayer(Telephone.CallState state)
	{
		serverState = state;
		base.baseEntity.ClientRPC(null, "SetClientState", (int)serverState, (activeCallTo != null) ? activeCallTo.PhoneNumber : 0);
		if (base.baseEntity is MobilePhone mobilePhone)
		{
			mobilePhone.ToggleRinging(state == Telephone.CallState.Ringing);
		}
	}

	private void SetPhoneState(Telephone.CallState state)
	{
		if (state == Telephone.CallState.Idle && currentPlayer == null)
		{
			base.baseEntity.SetFlag(BaseEntity.Flags.Busy, b: false);
		}
		serverState = state;
		base.baseEntity.ClientRPC(null, "SetClientState", (int)serverState, (activeCallTo != null) ? activeCallTo.PhoneNumber : 0);
		if (base.baseEntity is Telephone telephone)
		{
			telephone.MarkDirtyForceUpdateOutputs();
		}
		if (base.baseEntity is MobilePhone mobilePhone)
		{
			mobilePhone.ToggleRinging(state == Telephone.CallState.Ringing);
		}
	}

	public void BeginCall()
	{
		if (Interface.CallHook("OnPhoneCallStart", this, activeCallTo, currentPlayer) == null)
		{
			if (IsMobile && activeCallTo != null && !activeCallTo.RequirePower)
			{
				_ = currentPlayer != null;
			}
			SetPhoneStateWithPlayer(Telephone.CallState.InProcess);
			Invoke(TimeOutCall, TelephoneManager.MaxCallLength);
			Interface.CallHook("OnPhoneCallStarted", this, activeCallTo, currentPlayer);
		}
	}

	public void ServerHangUp(BaseEntity.RPCMessage msg)
	{
		if (!(msg.player != currentPlayer))
		{
			ServerHangUp();
		}
	}

	public void ServerHangUp()
	{
		if (activeCallTo != null)
		{
			activeCallTo.RemoteHangUp();
		}
		SelfHangUp();
	}

	private void SelfHangUp()
	{
		OnDialFailed(Telephone.DialFailReason.SelfHangUp);
	}

	private void RemoteHangUp()
	{
		OnDialFailed(Telephone.DialFailReason.RemoteHangUp);
	}

	private void TimeOutCall()
	{
		OnDialFailed(Telephone.DialFailReason.TimeOutDuringCall);
	}

	public void OnReceivedVoiceFromUser(byte[] data)
	{
		if (activeCallTo != null)
		{
			activeCallTo.OnReceivedDataFromConnectedPhone(data);
		}
	}

	public void OnReceivedDataFromConnectedPhone(byte[] data)
	{
		base.baseEntity.ClientRPCEx(new SendInfo(BaseNetworkable.GetConnectionsWithin(base.transform.position, 15f))
		{
			priority = Priority.Immediate
		}, null, "OnReceivedVoice", data.Length, data);
	}

	public void OnIncomingCallWhileBusy()
	{
		base.baseEntity.ClientRPC(null, "OnIncomingCallDuringCall");
	}

	public void DestroyShared()
	{
		if (isServer && serverState != 0 && activeCallTo != null)
		{
			activeCallTo.RemoteHangUp();
		}
	}

	public void UpdatePhoneName(BaseEntity.RPCMessage msg)
	{
		if (!(msg.player != currentPlayer))
		{
			string text = msg.read.String();
			if (text.Length > 20)
			{
				text = text.Substring(0, 20);
			}
			if (Interface.CallHook("OnPhoneNameUpdate", this, text, msg.player) == null)
			{
				PhoneName = text;
				base.baseEntity.SendNetworkUpdate();
				Interface.CallHook("OnPhoneNameUpdated", this, PhoneName, msg.player);
			}
		}
	}

	public void Server_RequestPhoneDirectory(BaseEntity.RPCMessage msg)
	{
		if (msg.player != currentPlayer)
		{
			return;
		}
		int page = msg.read.Int32();
		using PhoneDirectory phoneDirectory = Pool.Get<PhoneDirectory>();
		TelephoneManager.GetPhoneDirectory(PhoneNumber, page, 12, phoneDirectory);
		base.baseEntity.ClientRPC(null, "ReceivePhoneDirectory", phoneDirectory);
	}

	public void Server_AddSavedNumber(BaseEntity.RPCMessage msg)
	{
		if (!(msg.player != currentPlayer))
		{
			if (savedNumbers == null)
			{
				savedNumbers = Pool.Get<PhoneDirectory>();
			}
			if (savedNumbers.entries == null)
			{
				savedNumbers.entries = Pool.GetList<PhoneDirectory.DirectoryEntry>();
			}
			int num = msg.read.Int32();
			string text = msg.read.String();
			if (IsSavedContactValid(text, num) && savedNumbers.entries.Count < 10)
			{
				PhoneDirectory.DirectoryEntry directoryEntry = Pool.Get<PhoneDirectory.DirectoryEntry>();
				directoryEntry.phoneName = text;
				directoryEntry.phoneNumber = num;
				directoryEntry.ShouldPool = false;
				savedNumbers.ShouldPool = false;
				savedNumbers.entries.Add(directoryEntry);
				base.baseEntity.SendNetworkUpdate();
			}
		}
	}

	public void Server_RemoveSavedNumber(BaseEntity.RPCMessage msg)
	{
		if (!(msg.player != currentPlayer))
		{
			uint number = msg.read.UInt32();
			if (savedNumbers.entries.RemoveAll((PhoneDirectory.DirectoryEntry p) => p.phoneNumber == number) > 0)
			{
				base.baseEntity.SendNetworkUpdate();
			}
		}
	}

	public string GetDirectoryName()
	{
		return PhoneName;
	}

	public static string PositionToGridCoord(Vector3 position)
	{
		Vector2 vector = new Vector2(TerrainMeta.NormalizeX(position.x), TerrainMeta.NormalizeZ(position.z));
		float num = TerrainMeta.Size.x / 1024f;
		int num2 = 7;
		Vector2 vector2 = vector * num * num2;
		float num3 = Mathf.Floor(vector2.x) + 1f;
		float num4 = Mathf.Floor(num * (float)num2 - vector2.y);
		string text = string.Empty;
		float num5 = num3 / 26f;
		float num6 = num3 % 26f;
		if (num6 == 0f)
		{
			num6 = 26f;
		}
		if (num5 > 1f)
		{
			text += Convert.ToChar(64 + (int)num5);
		}
		text += Convert.ToChar(64 + (int)num6);
		return $"{text}{num4}";
	}

	public void WatchForDisconnects()
	{
		bool flag = false;
		if (currentPlayer != null)
		{
			if (currentPlayer.IsSleeping())
			{
				flag = true;
			}
			if (currentPlayer.IsDead())
			{
				flag = true;
			}
			if (Vector3.Distance(base.transform.position, currentPlayer.transform.position) > 5f)
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			ServerHangUp();
			ClearCurrentUser();
		}
	}

	public void OnParentChanged(BaseEntity newParent)
	{
		if (newParent != null && newParent is BasePlayer)
		{
			TelephoneManager.RegisterTelephone(this, checkPhoneNumber: true);
		}
		else
		{
			TelephoneManager.DeregisterTelephone(this);
		}
	}

	private bool HasVoicemailSlot()
	{
		return MaxVoicemailSlots > 0;
	}

	public void ServerSendVoicemail(BaseEntity.RPCMessage msg)
	{
		if (!(msg.player == null))
		{
			byte[] data = msg.read.BytesWithSize();
			PhoneController telephone = TelephoneManager.GetTelephone(msg.read.Int32());
			if (!(telephone == null) && Cassette.IsOggValid(data, telephone.cachedCassette))
			{
				telephone.SaveVoicemail(data, msg.player.displayName);
			}
		}
	}

	public void SaveVoicemail(byte[] data, string playerName)
	{
		uint audioId = FileStorage.server.Store(data, FileStorage.Type.ogg, base.baseEntity.net.ID);
		if (savedVoicemail == null)
		{
			savedVoicemail = Pool.GetList<ProtoBuf.VoicemailEntry>();
		}
		ProtoBuf.VoicemailEntry voicemailEntry = Pool.Get<ProtoBuf.VoicemailEntry>();
		voicemailEntry.audioId = audioId;
		voicemailEntry.timestamp = DateTime.Now.ToBinary();
		voicemailEntry.userName = playerName;
		voicemailEntry.ShouldPool = false;
		savedVoicemail.Add(voicemailEntry);
		while (savedVoicemail.Count > MaxVoicemailSlots)
		{
			FileStorage.server.Remove(savedVoicemail[0].audioId, FileStorage.Type.ogg, base.baseEntity.net.ID);
			savedVoicemail.RemoveAt(0);
		}
		base.baseEntity.SendNetworkUpdate();
	}

	public void ServerPlayVoicemail(BaseEntity.RPCMessage msg)
	{
		base.baseEntity.ClientRPC(null, "ClientToggleVoicemail", 1, msg.read.UInt32());
	}

	public void ServerStopVoicemail(BaseEntity.RPCMessage msg)
	{
		base.baseEntity.ClientRPC(null, "ClientToggleVoicemail", 0, msg.read.UInt32());
	}

	public void ServerDeleteVoicemail(BaseEntity.RPCMessage msg)
	{
		uint num = msg.read.UInt32();
		for (int i = 0; i < savedVoicemail.Count; i++)
		{
			if (savedVoicemail[i].audioId == num)
			{
				ProtoBuf.VoicemailEntry obj = savedVoicemail[i];
				FileStorage.server.Remove(obj.audioId, FileStorage.Type.ogg, base.baseEntity.net.ID);
				obj.ShouldPool = true;
				Pool.Free(ref obj);
				savedVoicemail.RemoveAt(i);
				base.baseEntity.SendNetworkUpdate();
				break;
			}
		}
	}

	public void DeleteAllVoicemail()
	{
		if (savedVoicemail == null)
		{
			return;
		}
		foreach (ProtoBuf.VoicemailEntry item in savedVoicemail)
		{
			item.ShouldPool = true;
			FileStorage.server.Remove(item.audioId, FileStorage.Type.ogg, base.baseEntity.net.ID);
		}
		Pool.FreeList(ref savedVoicemail);
	}
}

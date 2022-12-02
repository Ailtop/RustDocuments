using System;
using System.Collections.Generic;
using System.Linq;
using Network;
using ProtoBuf;
using UnityEngine;

namespace Facepunch.CardGames;

public abstract class CardGameController : IDisposable
{
	public enum CardGameState
	{
		NotPlaying = 0,
		InGameBetweenRounds = 1,
		InGameRound = 2,
		InGameRoundEnding = 3
	}

	public enum Playability
	{
		OK = 0,
		NoPlayer = 1,
		NotEnoughBuyIn = 2,
		TooMuchBuyIn = 3,
		RanOutOfScrap = 4,
		Idle = 5
	}

	public const int IDLE_KICK_SECONDS = 240;

	public CardGame.CardList localPlayerCards;

	protected int activePlayerIndex;

	public const int STD_RAISE_INCREMENTS = 5;

	protected bool isWaitingBetweenTurns;

	public CardGameState State { get; set; }

	public bool HasGameInProgress => State >= CardGameState.InGameBetweenRounds;

	public bool HasRoundInProgressOrEnding
	{
		get
		{
			if (State != CardGameState.InGameRound)
			{
				return State == CardGameState.InGameRoundEnding;
			}
			return true;
		}
	}

	public bool HasActiveRound => State == CardGameState.InGameRound;

	public CardPlayerData[] PlayerData { get; private set; }

	public abstract int MinPlayers { get; }

	public abstract int MinBuyIn { get; }

	public abstract int MaxBuyIn { get; }

	public abstract int MinToPlay { get; }

	public virtual float MaxTurnTime => 30f;

	public virtual int EndRoundDelay => 0;

	public virtual int TimeBetweenRounds => 8;

	protected virtual float TimeBetweenTurns => 1f;

	public BaseCardGameEntity Owner { get; set; }

	public int ScrapItemID => Owner.ScrapItemID;

	protected bool IsServer => Owner.isServer;

	protected bool IsClient => Owner.isClient;

	public CardGame.RoundResults resultInfo { get; private set; }

	public CardGameController(BaseCardGameEntity owner)
	{
		Owner = owner;
		PlayerData = new CardPlayerData[MaxPlayersAtTable()];
		resultInfo = Pool.Get<CardGame.RoundResults>();
		resultInfo.results = Pool.GetList<CardGame.RoundResults.Result>();
		localPlayerCards = Pool.Get<CardGame.CardList>();
		localPlayerCards.cards = Pool.GetList<int>();
		for (int i = 0; i < PlayerData.Length; i++)
		{
			PlayerData[i] = GetNewCardPlayerData(i);
		}
	}

	public IEnumerable<CardPlayerData> PlayersInRound()
	{
		CardPlayerData[] playerData = PlayerData;
		foreach (CardPlayerData cardPlayerData in playerData)
		{
			if (cardPlayerData.HasUserInCurrentRound)
			{
				yield return cardPlayerData;
			}
		}
	}

	protected abstract int GetFirstPlayerRelIndex(bool startOfRound);

	public void Dispose()
	{
		for (int i = 0; i < PlayerData.Length; i++)
		{
			PlayerData[i].Dispose();
		}
		localPlayerCards.Dispose();
		resultInfo.Dispose();
	}

	public int NumPlayersAllowedToPlay(CardPlayerData ignore = null)
	{
		int num = 0;
		CardPlayerData[] playerData = PlayerData;
		foreach (CardPlayerData cardPlayerData in playerData)
		{
			if (cardPlayerData != ignore && IsAllowedToPlay(cardPlayerData))
			{
				num++;
			}
		}
		return num;
	}

	public Playability GetPlayabilityStatus(CardPlayerData cpd)
	{
		if (!cpd.HasUser)
		{
			return Playability.NoPlayer;
		}
		int scrapAmount = cpd.GetScrapAmount();
		if (cpd.HasUserInGame)
		{
			if (scrapAmount < MinToPlay)
			{
				return Playability.RanOutOfScrap;
			}
		}
		else
		{
			if (scrapAmount < MinBuyIn)
			{
				return Playability.NotEnoughBuyIn;
			}
			if (scrapAmount > MaxBuyIn)
			{
				return Playability.TooMuchBuyIn;
			}
		}
		return Playability.OK;
	}

	public bool TryGetActivePlayer(out CardPlayerData activePlayer)
	{
		return ToCardPlayerData(activePlayerIndex, includeOutOfRound: false, out activePlayer);
	}

	protected bool ToCardPlayerData(int relIndex, bool includeOutOfRound, out CardPlayerData result)
	{
		if (!HasRoundInProgressOrEnding)
		{
			Debug.LogWarning(GetType().Name + ": Tried to call ToCardPlayerData while no round was in progress. Returning null.");
			result = null;
			return false;
		}
		int num = (includeOutOfRound ? NumPlayersInGame() : NumPlayersInCurrentRound());
		int index = RelToAbsIndex(relIndex % num, includeOutOfRound);
		return TryGetCardPlayerData(index, out result);
	}

	public int RelToAbsIndex(int relIndex, bool includeFolded)
	{
		if (!HasRoundInProgressOrEnding)
		{
			Debug.LogError(GetType().Name + ": Called RelToAbsIndex outside of a round. No-one is playing. Returning -1.");
			return -1;
		}
		int num = 0;
		for (int i = 0; i < PlayerData.Length; i++)
		{
			if (includeFolded ? PlayerData[i].HasUserInGame : PlayerData[i].HasUserInCurrentRound)
			{
				if (num == relIndex)
				{
					return i;
				}
				num++;
			}
		}
		Debug.LogError($"{GetType().Name}: No absolute index found for relative index {relIndex}. Only {NumPlayersInCurrentRound()} total players are in the round. Returning -1.");
		return -1;
	}

	public int GameToRoundIndex(int gameRelIndex)
	{
		if (!HasRoundInProgressOrEnding)
		{
			Debug.LogError(GetType().Name + ": Called GameToRoundIndex outside of a round. No-one is playing. Returning 0.");
			return 0;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < PlayerData.Length; i++)
		{
			if (PlayerData[i].HasUserInCurrentRound)
			{
				if (num == gameRelIndex)
				{
					return num2;
				}
				num++;
				num2++;
			}
			else if (PlayerData[i].HasUserInGame)
			{
				if (num == gameRelIndex)
				{
					return num2;
				}
				num++;
			}
		}
		Debug.LogError($"{GetType().Name}: No round index found for game index {gameRelIndex}. Only {NumPlayersInCurrentRound()} total players are in the round. Returning 0.");
		return 0;
	}

	public int NumPlayersInGame()
	{
		int num = 0;
		CardPlayerData[] playerData = PlayerData;
		for (int i = 0; i < playerData.Length; i++)
		{
			if (playerData[i].HasUserInGame)
			{
				num++;
			}
		}
		return num;
	}

	public int NumPlayersInCurrentRound()
	{
		int num = 0;
		CardPlayerData[] playerData = PlayerData;
		for (int i = 0; i < playerData.Length; i++)
		{
			if (playerData[i].HasUserInCurrentRound)
			{
				num++;
			}
		}
		return num;
	}

	public int MaxPlayersAtTable()
	{
		return Owner.mountPoints.Count;
	}

	public bool PlayerIsInGame(BasePlayer player)
	{
		return PlayerData.Any((CardPlayerData data) => data.HasUserInGame && data.UserID == player.userID);
	}

	public bool IsAtTable(BasePlayer player)
	{
		return IsAtTable(player.userID);
	}

	public virtual List<PlayingCard> GetTableCards()
	{
		return null;
	}

	public void StartTurnTimer(CardPlayerData pData, float turnTime)
	{
		if (IsServer)
		{
			pData.StartTurnTimer(OnTurnTimeout, turnTime);
			Owner.ClientRPC(null, "ClientStartTurnTimer", pData.mountIndex, turnTime);
		}
	}

	public bool IsAtTable(ulong userID)
	{
		return PlayerData.Any((CardPlayerData data) => data.UserID == userID);
	}

	public int GetScrapInPot()
	{
		if (IsServer)
		{
			StorageContainer pot = Owner.GetPot();
			if (pot != null)
			{
				return pot.inventory.GetAmount(ScrapItemID, onlyUsableAmounts: true);
			}
			return 0;
		}
		return 0;
	}

	public bool TryGetCardPlayerData(int index, out CardPlayerData cardPlayer)
	{
		if (index >= 0 && index < PlayerData.Length)
		{
			cardPlayer = PlayerData[index];
			return true;
		}
		cardPlayer = null;
		return false;
	}

	public bool TryGetCardPlayerData(ulong forPlayer, out CardPlayerData cardPlayer)
	{
		CardPlayerData[] playerData = PlayerData;
		foreach (CardPlayerData cardPlayerData in playerData)
		{
			if (cardPlayerData.UserID == forPlayer)
			{
				cardPlayer = cardPlayerData;
				return true;
			}
		}
		cardPlayer = null;
		return false;
	}

	public bool TryGetCardPlayerData(BasePlayer forPlayer, out CardPlayerData cardPlayer)
	{
		for (int i = 0; i < PlayerData.Length; i++)
		{
			if (PlayerData[i].UserID == forPlayer.userID)
			{
				cardPlayer = PlayerData[i];
				return true;
			}
		}
		cardPlayer = null;
		return false;
	}

	public bool IsAllowedToPlay(CardPlayerData cpd)
	{
		return GetPlayabilityStatus(cpd) == Playability.OK;
	}

	protected void ClearResultsInfo()
	{
		if (resultInfo.results == null)
		{
			return;
		}
		foreach (CardGame.RoundResults.Result result in resultInfo.results)
		{
			result?.Dispose();
		}
		resultInfo.results.Clear();
	}

	protected abstract CardPlayerData GetNewCardPlayerData(int mountIndex);

	protected abstract void OnTurnTimeout(CardPlayerData playerData);

	protected abstract void SubStartRound();

	protected abstract void SubReceivedInputFromPlayer(CardPlayerData playerData, int input, int value, bool countAsAction);

	protected abstract int GetAvailableInputsForPlayer(CardPlayerData playerData);

	protected abstract void HandlePlayerLeavingDuringTheirTurn(CardPlayerData pData);

	protected abstract void SubEndRound();

	protected abstract void SubEndGameplay();

	protected abstract void EndCycle();

	protected abstract bool ShouldEndCycle();

	public void EditorMakeRandomMove()
	{
	}

	public void JoinTable(BasePlayer player)
	{
		JoinTable(player.userID);
	}

	protected void SyncAllLocalPlayerCards()
	{
		CardPlayerData[] playerData = PlayerData;
		foreach (CardPlayerData pData in playerData)
		{
			SyncLocalPlayerCards(pData);
		}
	}

	protected void SyncLocalPlayerCards(CardPlayerData pData)
	{
		BasePlayer basePlayer = BasePlayer.FindByID(pData.UserID);
		if (basePlayer == null)
		{
			return;
		}
		localPlayerCards.cards.Clear();
		foreach (PlayingCard card in pData.Cards)
		{
			localPlayerCards.cards.Add(card.GetIndex());
		}
		Owner.ClientRPCPlayer(null, basePlayer, "ReceiveCardsForPlayer", localPlayerCards);
	}

	public void JoinTable(ulong userID)
	{
		if (IsAtTable(userID) || NumPlayersAllowedToPlay() >= MaxPlayersAtTable())
		{
			return;
		}
		int mountPointIndex = Owner.GetMountPointIndex(userID);
		if (mountPointIndex < 0)
		{
			return;
		}
		PlayerData[mountPointIndex].AddUser(userID);
		if (!HasGameInProgress)
		{
			if (!TryStartNewRound())
			{
				Owner.SendNetworkUpdate();
			}
		}
		else
		{
			Owner.SendNetworkUpdate();
		}
	}

	public void LeaveTable(ulong userID)
	{
		if (TryGetCardPlayerData(userID, out var cardPlayer))
		{
			LeaveTable(cardPlayer);
		}
	}

	public void LeaveTable(CardPlayerData pData)
	{
		if (HasActiveRound && TryGetActivePlayer(out var activePlayer))
		{
			if (pData == activePlayer)
			{
				HandlePlayerLeavingDuringTheirTurn(activePlayer);
			}
			else if (pData.HasUserInCurrentRound && pData.mountIndex < activePlayer.mountIndex && activePlayerIndex > 0)
			{
				activePlayerIndex--;
			}
		}
		pData.ClearAllData();
		if (HasActiveRound && NumPlayersInCurrentRound() < MinPlayers)
		{
			EndRoundWithDelay();
		}
		if (pData.HasUserInGame)
		{
			Owner.ClientRPC(null, "ClientOnPlayerLeft", pData.UserID);
		}
		Owner.SendNetworkUpdate();
	}

	protected int TryAddBet(CardPlayerData playerData, int maxAmount)
	{
		int num = TryMoveToPotStorage(playerData, maxAmount);
		playerData.betThisRound += num;
		playerData.betThisTurn += num;
		return num;
	}

	protected int GoAllIn(CardPlayerData playerData)
	{
		int num = TryMoveToPotStorage(playerData, 999999);
		playerData.betThisRound += num;
		playerData.betThisTurn += num;
		return num;
	}

	protected int TryMoveToPotStorage(CardPlayerData playerData, int maxAmount)
	{
		int num = 0;
		StorageContainer storage = playerData.GetStorage();
		StorageContainer pot = Owner.GetPot();
		if (storage != null && pot != null)
		{
			List<Item> obj = Pool.GetList<Item>();
			num = storage.inventory.Take(obj, ScrapItemID, maxAmount);
			if (num > 0)
			{
				foreach (Item item in obj)
				{
					if (!item.MoveToContainer(pot.inventory, -1, allowStack: true, ignoreStackLimit: true))
					{
						item.MoveToContainer(storage.inventory);
					}
				}
			}
			Pool.FreeList(ref obj);
		}
		else
		{
			Debug.LogError(GetType().Name + ": TryAddToPot: Null storage.");
		}
		return num;
	}

	protected int PayOutFromPot(CardPlayerData playerData, int maxAmount)
	{
		int num = 0;
		StorageContainer storage = playerData.GetStorage();
		StorageContainer pot = Owner.GetPot();
		if (storage != null && pot != null)
		{
			List<Item> obj = Pool.GetList<Item>();
			num = pot.inventory.Take(obj, ScrapItemID, maxAmount);
			if (num > 0)
			{
				foreach (Item item in obj)
				{
					if (!item.MoveToContainer(storage.inventory, -1, allowStack: true, ignoreStackLimit: true))
					{
						item.MoveToContainer(pot.inventory);
					}
				}
			}
			Pool.FreeList(ref obj);
		}
		else
		{
			Debug.LogError(GetType().Name + ": PayOut: Null storage.");
		}
		return num;
	}

	protected int PayOutAllFromPot(CardPlayerData playerData)
	{
		return PayOutFromPot(playerData, int.MaxValue);
	}

	protected void ClearPot()
	{
		StorageContainer pot = Owner.GetPot();
		if (pot != null)
		{
			pot.inventory.Clear();
		}
	}

	public int RemoveScrapFromStorage(CardPlayerData data)
	{
		StorageContainer storage = data.GetStorage();
		BasePlayer basePlayer = BasePlayer.FindByID(data.UserID);
		int num = 0;
		if (basePlayer != null)
		{
			List<Item> obj = Pool.GetList<Item>();
			num = storage.inventory.Take(obj, ScrapItemID, int.MaxValue);
			if (num > 0)
			{
				foreach (Item item in obj)
				{
					if (!item.MoveToContainer(basePlayer.inventory.containerMain, -1, allowStack: true, ignoreStackLimit: true))
					{
						item.MoveToContainer(storage.inventory);
					}
				}
			}
			Pool.FreeList(ref obj);
		}
		return num;
	}

	public virtual void Save(CardGame syncData)
	{
		syncData.players = Pool.GetList<CardGame.CardPlayer>();
		syncData.state = (int)State;
		syncData.activePlayerIndex = activePlayerIndex;
		CardPlayerData[] playerData = PlayerData;
		for (int i = 0; i < playerData.Length; i++)
		{
			playerData[i].Save(syncData);
		}
		syncData.pot = GetScrapInPot();
	}

	public void InvokeStartNewRound()
	{
		TryStartNewRound();
	}

	public bool TryStartNewRound()
	{
		if (HasRoundInProgressOrEnding)
		{
			return false;
		}
		CardPlayerData[] playerData = PlayerData;
		foreach (CardPlayerData cardPlayerData in playerData)
		{
			BasePlayer basePlayer;
			if (State == CardGameState.NotPlaying)
			{
				cardPlayerData.lastActionTime = Time.unscaledTime;
			}
			else if (cardPlayerData.HasBeenIdleFor(240) && BasePlayer.TryFindByID(cardPlayerData.UserID, out basePlayer))
			{
				basePlayer.GetMounted().DismountPlayer(basePlayer);
			}
		}
		if (NumPlayersAllowedToPlay() < MinPlayers)
		{
			EndGameplay();
			return false;
		}
		playerData = PlayerData;
		foreach (CardPlayerData cardPlayerData2 in playerData)
		{
			if (IsAllowedToPlay(cardPlayerData2))
			{
				cardPlayerData2.JoinRound();
			}
			else
			{
				cardPlayerData2.LeaveGame();
			}
		}
		State = CardGameState.InGameRound;
		SubStartRound();
		Owner.SendNetworkUpdate();
		return true;
	}

	protected void BeginRoundEnd()
	{
		State = CardGameState.InGameRoundEnding;
		CancelNextCycleInvoke();
		Owner.SendNetworkUpdate();
	}

	protected void EndRoundWithDelay()
	{
		State = CardGameState.InGameRoundEnding;
		CancelNextCycleInvoke();
		Owner.SendNetworkUpdate();
		Owner.Invoke(EndRound, EndRoundDelay);
	}

	public void EndRound()
	{
		State = CardGameState.InGameBetweenRounds;
		CancelNextCycleInvoke();
		ClearResultsInfo();
		SubEndRound();
		foreach (CardPlayerData item in PlayersInRound())
		{
			BasePlayer basePlayer = BasePlayer.FindByID(item.UserID);
			if (basePlayer != null && basePlayer.metabolism.CanConsume())
			{
				basePlayer.metabolism.MarkConsumption();
				basePlayer.metabolism.ApplyChange(MetabolismAttribute.Type.Calories, 2f, 0f);
				basePlayer.metabolism.ApplyChange(MetabolismAttribute.Type.Hydration, 2f, 0f);
			}
			item.LeaveCurrentRound(clearBets: true, leftRoundEarly: false);
		}
		Owner.SendNetworkUpdate();
		Owner.Invoke(InvokeStartNewRound, TimeBetweenRounds);
	}

	protected virtual void AddRoundResult(CardPlayerData pData, int winnings, int resultCode)
	{
		foreach (CardGame.RoundResults.Result result2 in resultInfo.results)
		{
			if (result2.ID == pData.UserID)
			{
				result2.winnings += winnings;
				return;
			}
		}
		CardGame.RoundResults.Result result = Pool.Get<CardGame.RoundResults.Result>();
		result.ID = pData.UserID;
		result.winnings = winnings;
		result.resultCode = resultCode;
		resultInfo.results.Add(result);
	}

	protected void EndGameplay()
	{
		if (HasGameInProgress)
		{
			CancelNextCycleInvoke();
			SubEndGameplay();
			State = CardGameState.NotPlaying;
			CardPlayerData[] playerData = PlayerData;
			for (int i = 0; i < playerData.Length; i++)
			{
				playerData[i].LeaveGame();
			}
			SyncAllLocalPlayerCards();
			Owner.SendNetworkUpdate();
		}
	}

	public void ReceivedInputFromPlayer(BasePlayer player, int input, bool countAsAction, int value = 0)
	{
		if (!(player == null))
		{
			player.ResetInputIdleTime();
			if (TryGetCardPlayerData(player, out var cardPlayer))
			{
				ReceivedInputFromPlayer(cardPlayer, input, countAsAction, value);
			}
		}
	}

	public void ReceivedInputFromPlayer(CardPlayerData pData, int input, bool countAsAction, int value = 0, bool playerInitiated = true)
	{
		if (HasGameInProgress && pData != null)
		{
			if (playerInitiated)
			{
				pData.lastActionTime = Time.unscaledTime;
			}
			SubReceivedInputFromPlayer(pData, input, value, countAsAction);
			if (HasActiveRound)
			{
				UpdateAllAvailableInputs();
				Owner.SendNetworkUpdate();
			}
		}
	}

	public void UpdateAllAvailableInputs()
	{
		for (int i = 0; i < PlayerData.Length; i++)
		{
			PlayerData[i].availableInputs = GetAvailableInputsForPlayer(PlayerData[i]);
		}
	}

	public void PlayerStorageChanged()
	{
		if (!HasGameInProgress)
		{
			TryStartNewRound();
		}
	}

	public void ServerPlaySound(CardGameSounds.SoundType type)
	{
		Owner.ClientRPC(null, "ClientPlaySound", (int)type);
	}

	public void GetConnectionsInGame(List<Connection> connections)
	{
		CardPlayerData[] playerData = PlayerData;
		foreach (CardPlayerData cardPlayerData in playerData)
		{
			if (cardPlayerData.HasUserInGame && BasePlayer.TryFindByID(cardPlayerData.UserID, out var basePlayer))
			{
				connections.Add(basePlayer.net.connection);
			}
		}
	}

	public virtual void OnTableDestroyed()
	{
		CardPlayerData[] playerData;
		if (HasGameInProgress)
		{
			playerData = PlayerData;
			foreach (CardPlayerData cardPlayerData in playerData)
			{
				if (cardPlayerData.HasUserInGame)
				{
					PayOutFromPot(cardPlayerData, cardPlayerData.GetTotalBetThisRound());
				}
			}
			if (GetScrapInPot() > 0)
			{
				int maxAmount = GetScrapInPot() / NumPlayersInGame();
				playerData = PlayerData;
				foreach (CardPlayerData cardPlayerData2 in playerData)
				{
					if (cardPlayerData2.HasUserInGame)
					{
						PayOutFromPot(cardPlayerData2, maxAmount);
					}
				}
			}
		}
		playerData = PlayerData;
		foreach (CardPlayerData cardPlayerData3 in playerData)
		{
			if (cardPlayerData3.HasUser)
			{
				RemoveScrapFromStorage(cardPlayerData3);
			}
		}
	}

	protected bool TryMoveToNextPlayerWithInputs(int startIndex, out CardPlayerData newActivePlayer)
	{
		activePlayerIndex = startIndex;
		TryGetActivePlayer(out newActivePlayer);
		int num = 0;
		bool flag = false;
		while (GetAvailableInputsForPlayer(newActivePlayer) == 0)
		{
			if (num == NumPlayersInCurrentRound())
			{
				flag = true;
				break;
			}
			activePlayerIndex = (activePlayerIndex + 1) % NumPlayersInCurrentRound();
			TryGetActivePlayer(out newActivePlayer);
			num++;
		}
		return !flag;
	}

	protected virtual void StartNextCycle()
	{
		isWaitingBetweenTurns = false;
	}

	protected void QueueNextCycleInvoke()
	{
		SingletonComponent<InvokeHandler>.Instance.Invoke(StartNextCycle, TimeBetweenTurns);
		isWaitingBetweenTurns = true;
		Owner.SendNetworkUpdate();
	}

	private void CancelNextCycleInvoke()
	{
		SingletonComponent<InvokeHandler>.Instance.CancelInvoke(StartNextCycle);
		isWaitingBetweenTurns = false;
	}
}

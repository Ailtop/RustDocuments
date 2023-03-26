using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Facepunch.CardGames;

public class CardPlayerData : IDisposable
{
	public enum CardPlayerState
	{
		None = 0,
		WantsToPlay = 1,
		InGame = 2,
		InCurrentRound = 3
	}

	public List<PlayingCard> Cards;

	public readonly int mountIndex;

	private readonly bool isServer;

	public int availableInputs;

	public int betThisRound;

	public int betThisTurn;

	public int finalScore;

	public float lastActionTime;

	public int remainingToPayOut;

	private Func<int, StorageContainer> getStorage;

	public readonly int scrapItemID;

	private Action<CardPlayerData> turnTimerCallback;

	public ulong UserID { get; set; }

	public CardPlayerState State { get; set; }

	public bool HasUser => State >= CardPlayerState.WantsToPlay;

	public bool HasUserInGame => State >= CardPlayerState.InGame;

	public bool HasUserInCurrentRound => State == CardPlayerState.InCurrentRound;

	public bool HasAvailableInputs => availableInputs > 0;

	public bool AllCardsAreKnown
	{
		get
		{
			if (Cards.Count == 0)
			{
				return false;
			}
			foreach (PlayingCard card in Cards)
			{
				if (card.IsUnknownCard)
				{
					return false;
				}
			}
			return true;
		}
	}

	private bool IsClient => !isServer;

	public bool LeftRoundEarly { get; set; }

	public bool SendCardDetails { get; set; }

	public bool hasCompletedTurn { get; private set; }

	public CardPlayerData(int mountIndex, bool isServer)
	{
		this.isServer = isServer;
		this.mountIndex = mountIndex;
		Cards = Pool.GetList<PlayingCard>();
	}

	public CardPlayerData(int scrapItemID, Func<int, StorageContainer> getStorage, int mountIndex, bool isServer)
		: this(mountIndex, isServer)
	{
		this.scrapItemID = scrapItemID;
		this.getStorage = getStorage;
	}

	public virtual void Dispose()
	{
		Pool.FreeList(ref Cards);
		if (isServer)
		{
			CancelTurnTimer();
		}
	}

	public int GetScrapAmount()
	{
		if (isServer)
		{
			StorageContainer storage = GetStorage();
			if (storage != null)
			{
				return storage.inventory.GetAmount(scrapItemID, onlyUsableAmounts: true);
			}
			Debug.LogError(GetType().Name + ": Couldn't get player storage.");
		}
		return 0;
	}

	public virtual int GetTotalBetThisRound()
	{
		return betThisRound;
	}

	public virtual List<PlayingCard> GetMainCards()
	{
		return Cards;
	}

	public virtual List<PlayingCard> GetSecondaryCards()
	{
		return null;
	}

	public void SetHasCompletedTurn(bool hasActed)
	{
		hasCompletedTurn = hasActed;
		if (!hasActed)
		{
			betThisTurn = 0;
		}
	}

	public bool HasBeenIdleFor(int seconds)
	{
		if (HasUserInGame)
		{
			return Time.unscaledTime > lastActionTime + (float)seconds;
		}
		return false;
	}

	public StorageContainer GetStorage()
	{
		return getStorage(mountIndex);
	}

	public void AddUser(ulong userID)
	{
		ClearAllData();
		UserID = userID;
		State = CardPlayerState.WantsToPlay;
		lastActionTime = Time.unscaledTime;
	}

	public void ClearAllData()
	{
		UserID = 0uL;
		availableInputs = 0;
		State = CardPlayerState.None;
		ClearPerRoundData();
	}

	public void JoinRound()
	{
		if (HasUser)
		{
			State = CardPlayerState.InCurrentRound;
			ClearPerRoundData();
		}
	}

	protected virtual void ClearPerRoundData()
	{
		Cards.Clear();
		betThisRound = 0;
		betThisTurn = 0;
		finalScore = 0;
		LeftRoundEarly = false;
		hasCompletedTurn = false;
		SendCardDetails = false;
	}

	public virtual void LeaveCurrentRound(bool clearBets, bool leftRoundEarly)
	{
		if (HasUserInCurrentRound)
		{
			availableInputs = 0;
			finalScore = 0;
			hasCompletedTurn = false;
			if (clearBets)
			{
				betThisRound = 0;
				betThisTurn = 0;
			}
			State = CardPlayerState.InGame;
			LeftRoundEarly = leftRoundEarly;
			CancelTurnTimer();
		}
	}

	public virtual void LeaveGame()
	{
		if (HasUserInGame)
		{
			Cards.Clear();
			availableInputs = 0;
			finalScore = 0;
			SendCardDetails = false;
			LeftRoundEarly = false;
			State = CardPlayerState.WantsToPlay;
		}
	}

	public void EnableSendingCards()
	{
		SendCardDetails = true;
	}

	public string HandToString()
	{
		return HandToString(Cards);
	}

	public static string HandToString(List<PlayingCard> cards)
	{
		string text = string.Empty;
		foreach (PlayingCard card in cards)
		{
			text = text + "23456789TJQKA"[(int)card.Rank] + "♠♥♦♣"[(int)card.Suit] + " ";
		}
		return text;
	}

	public virtual void Save(CardGame syncData)
	{
		CardGame.CardPlayer cardPlayer = Pool.Get<CardGame.CardPlayer>();
		cardPlayer.userid = UserID;
		cardPlayer.cards = Pool.GetList<int>();
		foreach (PlayingCard card in Cards)
		{
			cardPlayer.cards.Add(SendCardDetails ? card.GetIndex() : (-1));
		}
		cardPlayer.scrap = GetScrapAmount();
		cardPlayer.state = (int)State;
		cardPlayer.availableInputs = availableInputs;
		cardPlayer.betThisRound = betThisRound;
		cardPlayer.betThisTurn = betThisTurn;
		cardPlayer.leftRoundEarly = LeftRoundEarly;
		cardPlayer.sendCardDetails = SendCardDetails;
		syncData.players.Add(cardPlayer);
	}

	public void StartTurnTimer(Action<CardPlayerData> callback, float maxTurnTime)
	{
		turnTimerCallback = callback;
		SingletonComponent<InvokeHandler>.Instance.Invoke(TimeoutTurn, maxTurnTime);
	}

	public void CancelTurnTimer()
	{
		SingletonComponent<InvokeHandler>.Instance.CancelInvoke(TimeoutTurn);
	}

	public void TimeoutTurn()
	{
		if (turnTimerCallback != null)
		{
			turnTimerCallback(this);
		}
	}
}

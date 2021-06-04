using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Facepunch.CardGames
{
	public class CardPlayerData : IDisposable
	{
		public enum CardPlayerState
		{
			None,
			WantsToPlay,
			InGame,
			InCurrentRound
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

		private readonly int scrapItemID;

		public ulong UserID { get; private set; }

		public CardPlayerState State { get; private set; }

		public bool HasUser => State >= CardPlayerState.WantsToPlay;

		public bool HasUserInGame => State >= CardPlayerState.InGame;

		public bool HasUserInCurrentRound => State == CardPlayerState.InCurrentRound;

		private bool IsClient => !isServer;

		public bool LeftRoundEarly { get; private set; }

		public bool SendCardDetails { get; private set; }

		public bool hasActedThisTurn { get; private set; }

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

		public void Dispose()
		{
			Pool.FreeList(ref Cards);
		}

		public int GetScrapAmount()
		{
			if (!HasUser)
			{
				return 0;
			}
			if (isServer)
			{
				StorageContainer storage = GetStorage();
				if (storage != null)
				{
					return storage.inventory.GetAmount(scrapItemID, true);
				}
				Debug.LogError(GetType().Name + ": Couldn't get player storage.");
			}
			return 0;
		}

		public void SetHasActedThisTurn(bool hasActed)
		{
			hasActedThisTurn = hasActed;
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
			Cards.Clear();
			availableInputs = 0;
			betThisRound = 0;
			betThisTurn = 0;
			finalScore = 0;
			LeftRoundEarly = false;
			hasActedThisTurn = false;
			SendCardDetails = false;
			State = CardPlayerState.None;
		}

		public void JoinRound()
		{
			if (HasUser)
			{
				State = CardPlayerState.InCurrentRound;
				Cards.Clear();
				betThisRound = 0;
				betThisTurn = 0;
				finalScore = 0;
				LeftRoundEarly = false;
				hasActedThisTurn = false;
				SendCardDetails = false;
			}
		}

		public void LeaveCurrentRound(bool clearBets, bool leftRoundEarly)
		{
			if (HasUserInCurrentRound)
			{
				availableInputs = 0;
				finalScore = 0;
				hasActedThisTurn = false;
				if (clearBets)
				{
					betThisRound = 0;
					betThisTurn = 0;
				}
				State = CardPlayerState.InGame;
				LeftRoundEarly = leftRoundEarly;
			}
		}

		public void LeaveGame()
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

		public void Save(List<ProtoBuf.CardTable.CardPlayer> playersMsg)
		{
			ProtoBuf.CardTable.CardPlayer cardPlayer = Pool.Get<ProtoBuf.CardTable.CardPlayer>();
			cardPlayer.userid = UserID;
			cardPlayer.cards = Pool.GetList<int>();
			if (SendCardDetails)
			{
				foreach (PlayingCard card in Cards)
				{
					cardPlayer.cards.Add(card.GetIndex());
				}
			}
			cardPlayer.scrap = GetScrapAmount();
			cardPlayer.state = (int)State;
			cardPlayer.availableInputs = availableInputs;
			cardPlayer.betThisRound = betThisRound;
			cardPlayer.betThisTurn = betThisTurn;
			cardPlayer.trueCardCount = Cards.Count;
			cardPlayer.leftRoundEarly = LeftRoundEarly;
			cardPlayer.sendCardDetails = SendCardDetails;
			playersMsg.Add(cardPlayer);
		}
	}
}

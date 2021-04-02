using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Facepunch.CardGames
{
	public class CardPlayerData : IDisposable, IComparable<CardPlayerData>
	{
		public enum CardPlayerState
		{
			None,
			WantsToPlay,
			InGame,
			InCurrentRound
		}

		public List<PlayingCard> Cards;

		private readonly bool isServer;

		private readonly int mountIndex;

		public int availableInputs;

		public int finalScore;

		public float lastActionTime;

		private Func<int, StorageContainer> getStorage;

		private readonly int scrapItemID;

		public ulong UserID
		{
			get;
			private set;
		}

		public CardPlayerState State
		{
			get;
			private set;
		}

		public bool HasUser => State >= CardPlayerState.WantsToPlay;

		public bool HasUserInGame => State >= CardPlayerState.InGame;

		public bool HasUserInCurrentRound => State == CardPlayerState.InCurrentRound;

		private bool IsClient => !isServer;

		public int BetThisRound
		{
			get;
			private set;
		}

		public int BetThisTurn
		{
			get;
			private set;
		}

		public bool hasActedThisTurn
		{
			get;
			private set;
		}

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

		public int CompareTo(CardPlayerData other)
		{
			if (other == null)
			{
				return -1;
			}
			int num = finalScore.CompareTo(other.finalScore);
			if (num == 0)
			{
				return BetThisRound.CompareTo(other.BetThisRound);
			}
			return num;
		}

		public void AddBetAmount(int amount)
		{
			BetThisRound += amount;
			BetThisTurn += amount;
		}

		public void SetHasActedThisTurn(bool hasActed)
		{
			hasActedThisTurn = hasActed;
			if (!hasActed)
			{
				BetThisTurn = 0;
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
			BetThisRound = 0;
			BetThisTurn = 0;
			finalScore = 0;
			hasActedThisTurn = false;
			State = CardPlayerState.None;
		}

		public void JoinRound()
		{
			if (HasUser)
			{
				State = CardPlayerState.InCurrentRound;
				Cards.Clear();
				BetThisRound = 0;
				BetThisTurn = 0;
				finalScore = 0;
				hasActedThisTurn = false;
			}
		}

		public void LeaveCurrentRound(bool clearCards)
		{
			if (HasUserInCurrentRound)
			{
				if (clearCards)
				{
					Cards.Clear();
				}
				availableInputs = 0;
				finalScore = 0;
				hasActedThisTurn = false;
				State = CardPlayerState.InGame;
			}
		}

		public void LeaveGame()
		{
			if (HasUserInGame)
			{
				Cards.Clear();
				availableInputs = 0;
				finalScore = 0;
				State = CardPlayerState.WantsToPlay;
			}
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

		public void Save(List<ProtoBuf.CardTable.CardPlayer> playersMsg, bool sendCards)
		{
			ProtoBuf.CardTable.CardPlayer cardPlayer = Pool.Get<ProtoBuf.CardTable.CardPlayer>();
			cardPlayer.userid = UserID;
			if (sendCards)
			{
				cardPlayer.cards = Pool.GetList<int>();
				foreach (PlayingCard card in Cards)
				{
					cardPlayer.cards.Add(card.GetIndex());
				}
			}
			cardPlayer.scrap = GetScrapAmount();
			cardPlayer.state = (int)State;
			cardPlayer.availableInputs = availableInputs;
			cardPlayer.betThisRound = BetThisRound;
			cardPlayer.betThisTurn = BetThisTurn;
			playersMsg.Add(cardPlayer);
		}
	}
}

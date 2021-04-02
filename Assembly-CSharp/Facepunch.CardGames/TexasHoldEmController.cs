using System;
using System.Collections.Generic;
using PokerEvaluator;
using ProtoBuf;
using UnityEngine;

namespace Facepunch.CardGames
{
	public class TexasHoldEmController : CardGameController
	{
		[Flags]
		public enum PokerInputOption
		{
			None = 0x1,
			Fold = 0x2,
			Call = 0x4,
			AllIn = 0x8,
			Check = 0x10,
			Raise = 0x20,
			Bet = 0x40
		}

		public enum Playability
		{
			OK,
			NoPlayer,
			NotEnoughBuyIn,
			TooMuchBuyIn,
			RanOutOfScrap,
			Idle
		}

		public List<PlayingCard> flopCards = new List<PlayingCard>();

		public const int SMALL_BLIND = 5;

		public const int BIG_BLIND = 10;

		public const int RAISE_INCREMENTS = 5;

		private int dealerIndex;

		private int activePlayerIndex;

		private DeckOfCards deck = new DeckOfCards();

		public override int MinBuyIn => 100;

		public override int MaxBuyIn => 1000;

		public override int MinPlayers => 2;

		public PokerInputOption LastAction
		{
			get;
			private set;
		}

		public ulong LastActionTarget
		{
			get;
			private set;
		}

		public int LastActionValue
		{
			get;
			private set;
		}

		public int biggestRaiseThisTurn
		{
			get;
			private set;
		}

		protected override bool SendAllCards => !base.HasRoundInProgress;

		public TexasHoldEmController(CardTable owner)
			: base(owner)
		{
		}

		public Playability GetPlayabilityStatus(BasePlayer bp)
		{
			CardPlayerData cardPlayer;
			if (TryGetCardPlayerData(bp, out cardPlayer))
			{
				return GetPlayabilityStatus(cardPlayer);
			}
			return Playability.NoPlayer;
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
				if (scrapAmount <= 0)
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

		public int GetCurrentBet()
		{
			int num = 0;
			CardPlayerData[] playerData = base.playerData;
			foreach (CardPlayerData cardPlayerData in playerData)
			{
				if (cardPlayerData.HasUserInCurrentRound)
				{
					num = Mathf.Max(num, cardPlayerData.BetThisTurn);
				}
			}
			return num;
		}

		public override bool IsAllowedToPlay(CardPlayerData cpd)
		{
			return GetPlayabilityStatus(cpd) == Playability.OK;
		}

		public bool TryGetActivePlayer(out CardPlayerData activePlayer)
		{
			return ToCardPlayerData(activePlayerIndex, NumPlayersInCurrentRound(), out activePlayer);
		}

		public bool TryGetActivePlayer(int numPlayers, out CardPlayerData activePlayer)
		{
			return ToCardPlayerData(activePlayerIndex, numPlayers, out activePlayer);
		}

		public bool TryGetDealer(int numPlayers, out CardPlayerData dealer)
		{
			return ToCardPlayerData(dealerIndex, numPlayers, out dealer);
		}

		public bool TryGetSmallBlind(int numPlayers, out CardPlayerData smallBlind)
		{
			int relIndex = ((numPlayers < 3) ? dealerIndex : (dealerIndex + 1));
			return ToCardPlayerData(relIndex, numPlayers, out smallBlind);
		}

		public bool TryGetBigBlind(int numPlayers, out CardPlayerData bigBlind)
		{
			int relIndex = ((numPlayers < 3) ? (dealerIndex + 1) : (dealerIndex + 2));
			return ToCardPlayerData(relIndex, numPlayers, out bigBlind);
		}

		public int GetFirstPlayerRelIndex(bool startOfRound, int numPlayers)
		{
			if (startOfRound && numPlayers == 2)
			{
				return dealerIndex;
			}
			return dealerIndex + 1 % numPlayers;
		}

		private bool ToCardPlayerData(int relIndex, int numPlayers, out CardPlayerData result)
		{
			if (!base.HasRoundInProgress)
			{
				Debug.LogWarning(GetType().Name + ": Tried to call GetTruePlayerIndex while no round was in progress. Returning null.");
				result = null;
				return false;
			}
			int index = RelToAbsIndex(relIndex % numPlayers);
			return TryGetCardPlayerData(index, out result);
		}

		public static ushort EvaluatePokerHand(List<PlayingCard> cards)
		{
			ushort result = 0;
			int[] array = new int[cards.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = cards[i].GetEvaluationValue();
			}
			if (cards.Count == 5)
			{
				result = PokerLib.Eval5Hand(array);
			}
			else if (cards.Count == 7)
			{
				result = PokerLib.Eval7Hand(array);
			}
			else
			{
				Debug.LogError("Currently we can only evaluate five or seven card hands.");
			}
			return result;
		}

		public int GetCurrentMinRaise(CardPlayerData playerData)
		{
			return Mathf.Max(10, GetCurrentBet() - playerData.BetThisTurn + biggestRaiseThisTurn);
		}

		public override void Save(ProtoBuf.CardTable syncData)
		{
			base.Save(syncData);
			syncData.texasHoldEm = Pool.Get<ProtoBuf.CardTable.TexasHoldEm>();
			syncData.texasHoldEm.dealerIndex = dealerIndex;
			syncData.texasHoldEm.flopCards = Pool.GetList<int>();
			syncData.texasHoldEm.activePlayerIndex = activePlayerIndex;
			syncData.texasHoldEm.biggestRaiseThisTurn = biggestRaiseThisTurn;
			syncData.lastActionId = (int)LastAction;
			syncData.lastActionTarget = LastActionTarget;
			syncData.lastActionValue = LastActionValue;
			foreach (PlayingCard flopCard in flopCards)
			{
				syncData.texasHoldEm.flopCards.Add(flopCard.GetIndex());
			}
		}

		protected override void SubStartRound()
		{
			flopCards.Clear();
			deck = new DeckOfCards();
			biggestRaiseThisTurn = 0;
			LastAction = (PokerInputOption)0;
			LastActionTarget = 0uL;
			LastActionValue = 0;
			IncrementDealer();
			DealHoleCards();
			int numPlayers = NumPlayersInCurrentRound();
			activePlayerIndex = GetFirstPlayerRelIndex(true, numPlayers);
			ServerPlaySound(CardGameSounds.SoundType.Shuffle);
			CardPlayerData activePlayer;
			TryGetActivePlayer(numPlayers, out activePlayer);
			activePlayer.availableInputs = SubGetAvailableInputsForPlayer(activePlayer);
			if ((activePlayer.availableInputs & 0x40) == 64)
			{
				ReceivedInputFromPlayer(activePlayer, 64, 5, false);
			}
			else
			{
				ReceivedInputFromPlayer(activePlayer, 8, 5, false);
			}
			TryGetActivePlayer(numPlayers, out activePlayer);
			activePlayer.availableInputs = SubGetAvailableInputsForPlayer(activePlayer);
			if ((activePlayer.availableInputs & 0x20) == 32)
			{
				ReceivedInputFromPlayer(activePlayer, 32, 10, false);
			}
			else
			{
				ReceivedInputFromPlayer(activePlayer, 8, 10, false);
			}
		}

		protected override void SubEndRound()
		{
			List<CardPlayerData> list = Pool.GetList<CardPlayerData>();
			GetPlayersInRound(list);
			if (list.Count == 0)
			{
				return;
			}
			list.Sort();
			ClearWinnerInfo();
			SingletonComponent<InvokeHandler>.Instance.CancelInvoke(TimeoutTurn);
			int num = GetScrapInPot();
			foreach (CardPlayerData item in list)
			{
				num -= item.BetThisRound;
			}
			bool primaryWinner = true;
			while (GetScrapInPot() > 0 && list.Count > 0)
			{
				int num2 = 1;
				if (list.Count > 1)
				{
					for (int i = 1; i < list.Count && list[i].finalScore == list[0].finalScore; i++)
					{
						num2++;
					}
				}
				int num3 = Mathf.CeilToInt(num / num2);
				for (int j = 0; j < num2; j++)
				{
					int b = Mathf.CeilToInt(GetScrapInPot() / (num2 - j));
					CardPlayerData cardPlayerData = list[j];
					int betThisRound = cardPlayerData.BetThisRound;
					int num4 = 0;
					CardPlayerData[] playerData = base.playerData;
					foreach (CardPlayerData cardPlayerData2 in playerData)
					{
						num4 += Mathf.Min(cardPlayerData2.BetThisRound, betThisRound);
					}
					int num5 = Mathf.Min(num4, b);
					num5 += num3;
					num -= num3;
					PayOut(cardPlayerData, num5);
					_003CSubEndRound_003Eg__AddWinner_007C47_0(cardPlayerData, num5, primaryWinner);
				}
				list.RemoveRange(0, num2);
				primaryWinner = false;
			}
			base.Owner.ClientRPC(null, "OnWinnersDeclared", base.winnerInfo);
			StorageContainer pot = base.Owner.GetPot();
			if (pot != null)
			{
				int amount = pot.inventory.GetAmount(base.ScrapItemID, true);
				if (amount > 0)
				{
					Debug.LogWarning($"{GetType().Name}: Something went wrong in the winner calculation. Pot still has {amount} scrap left over after payouts. Expected 0.");
				}
			}
		}

		protected override void SubEndGameplay()
		{
		}

		private void IncrementDealer()
		{
			int num = NumPlayersInGame();
			dealerIndex = Mathf.Clamp(dealerIndex, 0, num - 1);
			if (num != 0)
			{
				dealerIndex = ++dealerIndex % num;
			}
		}

		private void IncrementActivePlayer()
		{
			if (!base.HasRoundInProgress)
			{
				Debug.LogError(GetType().Name + ": Can't increment active player when no round is in progress.");
			}
			int num = NumPlayersInCurrentRound();
			if (num != 0)
			{
				if (ShouldEndTurn())
				{
					EndTurn();
				}
				else
				{
					activePlayerIndex = ++activePlayerIndex % num;
				}
			}
		}

		private void DealHoleCards()
		{
			CardPlayerData[] playerData;
			for (int i = 0; i < 2; i++)
			{
				playerData = base.playerData;
				foreach (CardPlayerData cardPlayerData in playerData)
				{
					if (cardPlayerData.HasUserInCurrentRound)
					{
						PlayingCard card;
						if (deck.TryTakeCard(out card))
						{
							cardPlayerData.Cards.Add(card);
						}
						else
						{
							Debug.LogError(GetType().Name + ": No more cards in the deck to deal!");
						}
					}
				}
			}
			playerData = base.playerData;
			foreach (CardPlayerData cardPlayerData2 in playerData)
			{
				if (!cardPlayerData2.HasUserInCurrentRound)
				{
					continue;
				}
				BasePlayer basePlayer = BasePlayer.FindByID(cardPlayerData2.UserID);
				if (!(basePlayer != null))
				{
					continue;
				}
				localPlayerCards.cards.Clear();
				foreach (PlayingCard card2 in cardPlayerData2.Cards)
				{
					localPlayerCards.cards.Add(card2.GetIndex());
				}
				base.Owner.ClientRPCPlayer(null, basePlayer, "ReceiveCardsForPlayer", localPlayerCards);
			}
		}

		private bool DealFlop()
		{
			if (flopCards.Count == 0)
			{
				for (int i = 0; i < 3; i++)
				{
					PlayingCard card;
					if (deck.TryTakeCard(out card))
					{
						flopCards.Add(card);
					}
				}
				ServerPlaySound(CardGameSounds.SoundType.Draw);
				return true;
			}
			if (flopCards.Count == 3 || flopCards.Count == 4)
			{
				PlayingCard card2;
				if (deck.TryTakeCard(out card2))
				{
					flopCards.Add(card2);
				}
				ServerPlaySound(CardGameSounds.SoundType.Draw);
				return true;
			}
			return false;
		}

		protected override void TimeoutTurn()
		{
			CardPlayerData activePlayer;
			if (TryGetActivePlayer(out activePlayer))
			{
				ReceivedInputFromPlayer(activePlayer, 2, 0, false);
			}
		}

		protected override void SubReceivedInputFromPlayer(CardPlayerData playerData, int input, int value)
		{
			CardPlayerData activePlayer;
			if (!base.HasRoundInProgress || !TryGetActivePlayer(out activePlayer) || activePlayer != playerData)
			{
				return;
			}
			bool flag = false;
			if ((playerData.availableInputs & input) != input)
			{
				return;
			}
			switch (input)
			{
			case 2:
				playerData.LeaveCurrentRound(true);
				flag = true;
				LastActionValue = 0;
				playerData.SetHasActedThisTurn(true);
				break;
			case 4:
			{
				int currentBet = GetCurrentBet();
				int num = (LastActionValue = AddToPot(playerData, currentBet - playerData.BetThisTurn));
				playerData.SetHasActedThisTurn(true);
				break;
			}
			case 32:
			case 64:
			{
				int currentBet = GetCurrentBet();
				int biggestRaiseThisTurn = this.biggestRaiseThisTurn;
				if (playerData.BetThisTurn + value < currentBet + biggestRaiseThisTurn)
				{
					value = currentBet + biggestRaiseThisTurn - playerData.BetThisTurn;
				}
				int num = AddToPot(playerData, value);
				this.biggestRaiseThisTurn = Mathf.Max(this.biggestRaiseThisTurn, num - currentBet);
				LastActionValue = num;
				playerData.SetHasActedThisTurn(true);
				break;
			}
			case 8:
			{
				int currentBet = GetCurrentBet();
				int num = AddAllToPot(playerData);
				this.biggestRaiseThisTurn = Mathf.Max(this.biggestRaiseThisTurn, num - currentBet);
				LastActionValue = num;
				playerData.SetHasActedThisTurn(true);
				break;
			}
			case 16:
				LastActionValue = 0;
				playerData.SetHasActedThisTurn(true);
				break;
			}
			LastActionTarget = playerData.UserID;
			LastAction = (PokerInputOption)input;
			if (flag && NumPlayersInCurrentRound() == 1)
			{
				EndRound();
				return;
			}
			IncrementActivePlayer();
			if (!base.HasRoundInProgress)
			{
				return;
			}
			int num3 = 0;
			int num4 = 0;
			CardPlayerData activePlayer2;
			TryGetActivePlayer(out activePlayer2);
			while (SubGetAvailableInputsForPlayer(activePlayer2) == 1 && num3 <= NumPlayersInGame() && num4 < 200)
			{
				num4++;
				int count = flopCards.Count;
				IncrementActivePlayer();
				if (!base.HasRoundInProgress)
				{
					return;
				}
				TryGetActivePlayer(out activePlayer2);
				num3++;
				if (flopCards.Count != count)
				{
					num3 = 0;
				}
			}
			StartTurnTimer(MaxTurnTime);
		}

		private bool ShouldEndTurn()
		{
			CardPlayerData[] playerData = base.playerData;
			foreach (CardPlayerData cardPlayerData in playerData)
			{
				if (cardPlayerData.HasUserInCurrentRound && cardPlayerData.GetScrapAmount() > 0 && (cardPlayerData.BetThisTurn != GetCurrentBet() || !cardPlayerData.hasActedThisTurn))
				{
					return false;
				}
			}
			return true;
		}

		private void EndTurn()
		{
			CardPlayerData[] playerData = base.playerData;
			for (int i = 0; i < playerData.Length; i++)
			{
				playerData[i].SetHasActedThisTurn(false);
			}
			biggestRaiseThisTurn = 0;
			activePlayerIndex = GetFirstPlayerRelIndex(false, NumPlayersInCurrentRound());
			if (DealFlop())
			{
				int num = 0;
				playerData = base.playerData;
				foreach (CardPlayerData cardPlayerData in playerData)
				{
					if (cardPlayerData.HasUserInCurrentRound && cardPlayerData.GetScrapAmount() > 0)
					{
						num++;
					}
				}
				if (num == 1)
				{
					EndTurn();
				}
				return;
			}
			playerData = base.playerData;
			foreach (CardPlayerData cardPlayerData2 in playerData)
			{
				if (cardPlayerData2.HasUserInCurrentRound)
				{
					List<PlayingCard> obj = Pool.GetList<PlayingCard>();
					obj.AddRange(cardPlayerData2.Cards);
					obj.AddRange(flopCards);
					ushort finalScore = EvaluatePokerHand(obj);
					Pool.FreeList(ref obj);
					cardPlayerData2.finalScore = finalScore;
				}
			}
			EndRound();
		}

		protected override int SubGetAvailableInputsForPlayer(CardPlayerData playerData)
		{
			CardPlayerData activePlayer;
			if (!base.HasRoundInProgress || playerData == null || !TryGetActivePlayer(out activePlayer) || playerData != activePlayer)
			{
				return 1;
			}
			PokerInputOption pokerInputOption = PokerInputOption.None;
			int scrapAmount = playerData.GetScrapAmount();
			if (scrapAmount > 0)
			{
				pokerInputOption |= PokerInputOption.AllIn;
				pokerInputOption |= PokerInputOption.Fold;
				int currentBet = GetCurrentBet();
				if (playerData.BetThisTurn >= currentBet)
				{
					pokerInputOption |= PokerInputOption.Check;
				}
				if (currentBet > playerData.BetThisTurn && scrapAmount >= currentBet - playerData.BetThisTurn)
				{
					pokerInputOption |= PokerInputOption.Call;
				}
				if (scrapAmount >= GetCurrentMinRaise(playerData))
				{
					pokerInputOption = ((biggestRaiseThisTurn != 0) ? (pokerInputOption | PokerInputOption.Raise) : (pokerInputOption | PokerInputOption.Bet));
				}
			}
			return (int)pokerInputOption;
		}

		public override void OnTableDestroyed()
		{
			base.OnTableDestroyed();
			if (!base.HasGameInProgress)
			{
				return;
			}
			int maxAmount = GetScrapInPot() / NumPlayersInGame();
			CardPlayerData[] playerData = base.playerData;
			foreach (CardPlayerData cardPlayerData in playerData)
			{
				if (cardPlayerData.HasUserInGame)
				{
					PayOut(cardPlayerData, maxAmount);
				}
				if (cardPlayerData.HasUser)
				{
					RemoveScrapFromStorage(cardPlayerData);
				}
			}
		}

		protected override void SubOnPlayerLeaving(CardPlayerData playerData)
		{
			if (base.HasRoundInProgress)
			{
				CardPlayerData activePlayer;
				if (TryGetActivePlayer(out activePlayer) && playerData == activePlayer)
				{
					ReceivedInputFromPlayer(activePlayer, 2, 0, false);
				}
				playerData.LeaveCurrentRound(true);
				if (NumPlayersInCurrentRound() == 1)
				{
					EndRound();
				}
			}
		}
	}
}

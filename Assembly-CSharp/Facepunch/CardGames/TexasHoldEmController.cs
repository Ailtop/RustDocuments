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
			Bet = 0x40,
			RevealHand = 0x80
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

		public PokerInputOption LastAction { get; private set; }

		public ulong LastActionTarget { get; private set; }

		public int LastActionValue { get; private set; }

		public int biggestRaiseThisTurn { get; private set; }

		public TexasHoldEmController(CardTable owner)
			: base(owner)
		{
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
			CardPlayerData[] array = playerData;
			foreach (CardPlayerData cardPlayerData in array)
			{
				if (cardPlayerData.HasUserInCurrentRound)
				{
					num = Mathf.Max(num, cardPlayerData.betThisTurn);
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
			return ToCardPlayerData(activePlayerIndex, false, out activePlayer);
		}

		public bool TryGetDealer(out CardPlayerData dealer)
		{
			return ToCardPlayerData(dealerIndex, true, out dealer);
		}

		public bool TryGetSmallBlind(out CardPlayerData smallBlind)
		{
			int relIndex = ((NumPlayersInGame() < 3) ? dealerIndex : (dealerIndex + 1));
			return ToCardPlayerData(relIndex, true, out smallBlind);
		}

		public bool TryGetBigBlind(out CardPlayerData bigBlind)
		{
			int relIndex = ((NumPlayersInGame() < 3) ? (dealerIndex + 1) : (dealerIndex + 2));
			return ToCardPlayerData(relIndex, true, out bigBlind);
		}

		public int GetFirstPlayerRelIndex(bool startOfRound)
		{
			int num = NumPlayersInGame();
			if (startOfRound && num == 2)
			{
				return dealerIndex;
			}
			return (dealerIndex + 1) % num;
		}

		private void SetActivePlayerIndex(int index)
		{
			activePlayerIndex = index;
			if (base.IsServer)
			{
				base.Owner.SendNetworkUpdate();
			}
		}

		private bool ToCardPlayerData(int relIndex, bool includeFolded, out CardPlayerData result)
		{
			if (!base.HasRoundInProgress)
			{
				Debug.LogWarning(GetType().Name + ": Tried to call ToCardPlayerData while no round was in progress. Returning null.");
				result = null;
				return false;
			}
			int num = (includeFolded ? NumPlayersInGame() : NumPlayersInCurrentRound());
			int index = RelToAbsIndex(relIndex % num, includeFolded);
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
			return Mathf.Max(10, GetCurrentBet() - playerData.betThisTurn + biggestRaiseThisTurn);
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
			SetActivePlayerIndex(GetFirstPlayerRelIndex(true));
			ServerPlaySound(CardGameSounds.SoundType.Shuffle);
			CardPlayerData activePlayer;
			TryGetActivePlayer(out activePlayer);
			activePlayer.availableInputs = SubGetAvailableInputsForPlayer(activePlayer);
			if ((activePlayer.availableInputs & 0x40) == 64)
			{
				ReceivedInputFromPlayer(activePlayer, 64, false, 5, false);
			}
			else
			{
				ReceivedInputFromPlayer(activePlayer, 8, false, 5, false);
			}
			TryGetActivePlayer(out activePlayer);
			activePlayer.availableInputs = SubGetAvailableInputsForPlayer(activePlayer);
			if ((activePlayer.availableInputs & 0x20) == 32)
			{
				ReceivedInputFromPlayer(activePlayer, 32, false, 10, false);
			}
			else
			{
				ReceivedInputFromPlayer(activePlayer, 8, false, 10, false);
			}
		}

		protected override void SubEndRound()
		{
			int num = 0;
			List<CardPlayerData> obj = Pool.GetList<CardPlayerData>();
			CardPlayerData[] array = playerData;
			foreach (CardPlayerData cardPlayerData in array)
			{
				if (cardPlayerData.betThisRound > 0)
				{
					obj.Add(cardPlayerData);
				}
				if (cardPlayerData.HasUserInCurrentRound)
				{
					num++;
				}
			}
			if (obj.Count == 0)
			{
				return;
			}
			ClearWinnerInfo();
			bool flag = num > 1;
			SingletonComponent<InvokeHandler>.Instance.CancelInvoke(TimeoutTurn);
			int num2 = GetScrapInPot();
			array = playerData;
			foreach (CardPlayerData cardPlayerData2 in array)
			{
				if (cardPlayerData2.HasUserInGame)
				{
					num2 -= cardPlayerData2.betThisRound;
				}
			}
			bool flag2 = true;
			array = playerData;
			foreach (CardPlayerData obj2 in array)
			{
				obj2.remainingToPayOut = obj2.betThisRound;
			}
			while (obj.Count > 1)
			{
				int num3 = int.MaxValue;
				int num4 = 0;
				array = playerData;
				foreach (CardPlayerData cardPlayerData3 in array)
				{
					if (cardPlayerData3.betThisRound > 0)
					{
						if (cardPlayerData3.betThisRound < num3)
						{
							num3 = cardPlayerData3.betThisRound;
						}
						num4++;
					}
				}
				int num5 = num3 * num4;
				foreach (CardPlayerData item in obj)
				{
					item.betThisRound -= num3;
				}
				int num6 = int.MaxValue;
				foreach (CardPlayerData item2 in obj)
				{
					if (item2.HasUserInCurrentRound && item2.finalScore < num6)
					{
						num6 = item2.finalScore;
					}
				}
				if (flag2)
				{
					base.winnerInfo.winningScore = num6;
				}
				int num7 = 0;
				foreach (CardPlayerData item3 in obj)
				{
					if (item3.HasUserInCurrentRound && item3.finalScore == num6)
					{
						num7++;
					}
				}
				int num8 = Mathf.CeilToInt((float)(num5 + num2) / (float)num7);
				num2 = 0;
				foreach (CardPlayerData item4 in obj)
				{
					if (item4.HasUserInCurrentRound && item4.finalScore == num6)
					{
						if (flag)
						{
							item4.EnableSendingCards();
						}
						PayOut(item4, num8);
						_003CSubEndRound_003Eg__AddWinner_007C46_0(item4, num8, flag2);
					}
				}
				for (int num9 = obj.Count - 1; num9 >= 0; num9--)
				{
					if (obj[num9].betThisRound == 0)
					{
						obj.RemoveAt(num9);
					}
				}
				flag2 = false;
			}
			if (obj.Count == 1)
			{
				int num10 = obj[0].betThisRound + num2;
				num2 = 0;
				PayOut(obj[0], num10);
				bool primaryWinner = base.winnerInfo.winners.Count == 0;
				_003CSubEndRound_003Eg__AddWinner_007C46_0(obj[0], num10, primaryWinner);
			}
			base.Owner.ClientRPC(null, "OnWinnersDeclared", base.winnerInfo);
			StorageContainer pot = base.Owner.GetPot();
			if (pot != null)
			{
				int amount = pot.inventory.GetAmount(base.ScrapItemID, true);
				if (amount > 0)
				{
					Debug.LogError($"{GetType().Name}: Something went wrong in the winner calculation. Pot still has {amount} scrap left over after payouts. Expected 0. Clearing it.");
					pot.inventory.Clear();
				}
			}
			Pool.FreeList(ref obj);
		}

		protected override void SubEndGameplay()
		{
		}

		private void IncrementDealer()
		{
			int num = NumPlayersInGame();
			if (num == 0)
			{
				dealerIndex = 0;
				return;
			}
			dealerIndex = Mathf.Clamp(dealerIndex, 0, num - 1);
			dealerIndex = ++dealerIndex % num;
		}

		private void DealHoleCards()
		{
			for (int i = 0; i < 2; i++)
			{
				CardPlayerData[] array = playerData;
				foreach (CardPlayerData cardPlayerData in array)
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
			SyncAllLocalPlayerCards();
		}

		private bool DealFlop()
		{
			if (!base.HasRoundInProgress)
			{
				return false;
			}
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
				ReceivedInputFromPlayer(activePlayer, 2, true, 0, false);
			}
		}

		protected override void SubReceivedInputFromPlayer(CardPlayerData playerData, int input, int value, bool countAsAction)
		{
			if (!Enum.IsDefined(typeof(PokerInputOption), input))
			{
				return;
			}
			if (!base.HasRoundInProgress)
			{
				if (input == 128)
				{
					playerData.EnableSendingCards();
				}
				LastActionTarget = playerData.UserID;
				LastAction = (PokerInputOption)input;
				LastActionValue = 0;
			}
			else
			{
				CardPlayerData activePlayer;
				if (!TryGetActivePlayer(out activePlayer) || activePlayer != playerData)
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
					playerData.LeaveCurrentRound(false, true);
					flag = true;
					LastActionValue = 0;
					break;
				case 4:
				{
					int currentBet = GetCurrentBet();
					int num = (LastActionValue = AddToPot(playerData, currentBet - playerData.betThisTurn));
					break;
				}
				case 32:
				case 64:
				{
					int currentBet = GetCurrentBet();
					int num2 = biggestRaiseThisTurn;
					if (playerData.betThisTurn + value < currentBet + num2)
					{
						value = currentBet + num2 - playerData.betThisTurn;
					}
					int num = AddToPot(playerData, value);
					biggestRaiseThisTurn = Mathf.Max(biggestRaiseThisTurn, num - currentBet);
					LastActionValue = num;
					break;
				}
				case 8:
				{
					int currentBet = GetCurrentBet();
					int num = AddAllToPot(playerData);
					biggestRaiseThisTurn = Mathf.Max(biggestRaiseThisTurn, num - currentBet);
					LastActionValue = num;
					break;
				}
				case 16:
					LastActionValue = 0;
					break;
				}
				if (countAsAction && input != 1)
				{
					playerData.SetHasActedThisTurn(true);
				}
				LastActionTarget = playerData.UserID;
				LastAction = (PokerInputOption)input;
				if (flag && NumPlayersInCurrentRound() == 1)
				{
					EndRound();
					return;
				}
				int num4 = activePlayerIndex;
				if (flag)
				{
					if (activePlayerIndex > NumPlayersInCurrentRound() - 1)
					{
						num4 = 0;
					}
				}
				else
				{
					num4 = (activePlayerIndex + 1) % NumPlayersInCurrentRound();
				}
				if (ShouldEndTurn())
				{
					EndTurn();
				}
				else
				{
					SetActivePlayerIndex(num4);
				}
				if (!base.HasRoundInProgress)
				{
					return;
				}
				int num5 = 0;
				int num6 = 0;
				CardPlayerData activePlayer2;
				TryGetActivePlayer(out activePlayer2);
				while (SubGetAvailableInputsForPlayer(activePlayer2) == 1 && num5 <= NumPlayersInGame() && num6 < 200)
				{
					num6++;
					int count = flopCards.Count;
					if (ShouldEndTurn())
					{
						EndTurn();
					}
					else
					{
						SetActivePlayerIndex((activePlayerIndex + 1) % NumPlayersInCurrentRound());
					}
					if (!base.HasRoundInProgress)
					{
						return;
					}
					TryGetActivePlayer(out activePlayer2);
					num5++;
					if (flopCards.Count != count)
					{
						num5 = 0;
					}
				}
				StartTurnTimer(MaxTurnTime);
			}
		}

		private bool ShouldEndTurn()
		{
			CardPlayerData[] array = playerData;
			foreach (CardPlayerData cardPlayerData in array)
			{
				if (cardPlayerData.HasUserInCurrentRound && cardPlayerData.GetScrapAmount() > 0 && (cardPlayerData.betThisTurn != GetCurrentBet() || !cardPlayerData.hasActedThisTurn))
				{
					return false;
				}
			}
			return true;
		}

		private void EndTurn()
		{
			CardPlayerData[] array = playerData;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetHasActedThisTurn(false);
			}
			biggestRaiseThisTurn = 0;
			int num = GetFirstPlayerRelIndex(false);
			int num2 = NumPlayersInGame();
			int num3 = 0;
			CardPlayerData result;
			while (!ToCardPlayerData(num, true, out result) || !result.HasUserInCurrentRound)
			{
				num = ++num % num2;
				num3++;
				if (num3 > num2)
				{
					Debug.LogError(GetType().Name + ": This should never happen. Ended turn with no players in game?.");
					EndRound();
					return;
				}
			}
			int num4 = GameToRoundIndex(num);
			if (num4 < 0 || num4 > NumPlayersInCurrentRound())
			{
				Debug.LogError($"EndTurn NewActiveIndex is out of range: {num4}. Clamping it to between 0 and {NumPlayersInCurrentRound()}.");
				num4 = Mathf.Clamp(num4, 0, NumPlayersInCurrentRound());
			}
			SetActivePlayerIndex(num4);
			if (DealFlop())
			{
				int num5 = 0;
				array = playerData;
				foreach (CardPlayerData cardPlayerData in array)
				{
					if (cardPlayerData.HasUserInCurrentRound && cardPlayerData.GetScrapAmount() > 0)
					{
						num5++;
					}
				}
				if (num5 == 1)
				{
					EndTurn();
				}
				return;
			}
			array = playerData;
			foreach (CardPlayerData cardPlayerData2 in array)
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
			PokerInputOption pokerInputOption = PokerInputOption.None;
			if (playerData == null)
			{
				return (int)pokerInputOption;
			}
			if (!base.HasRoundInProgress)
			{
				if (!playerData.LeftRoundEarly && playerData.Cards.Count > 0 && !playerData.SendCardDetails)
				{
					pokerInputOption |= PokerInputOption.RevealHand;
				}
				return (int)pokerInputOption;
			}
			CardPlayerData activePlayer;
			if (!TryGetActivePlayer(out activePlayer) || playerData != activePlayer)
			{
				return (int)pokerInputOption;
			}
			int scrapAmount = playerData.GetScrapAmount();
			if (scrapAmount > 0)
			{
				pokerInputOption |= PokerInputOption.AllIn;
				pokerInputOption |= PokerInputOption.Fold;
				int currentBet = GetCurrentBet();
				if (playerData.betThisTurn >= currentBet)
				{
					pokerInputOption |= PokerInputOption.Check;
				}
				if (currentBet > playerData.betThisTurn && scrapAmount >= currentBet - playerData.betThisTurn)
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
			CardPlayerData[] array = playerData;
			foreach (CardPlayerData cardPlayerData in array)
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
			CardPlayerData activePlayer;
			if (base.HasRoundInProgress && TryGetActivePlayer(out activePlayer))
			{
				if (playerData == activePlayer)
				{
					ReceivedInputFromPlayer(activePlayer, 2, true, 0, false);
				}
				else if (playerData.HasUserInCurrentRound && playerData.mountIndex < activePlayer.mountIndex && activePlayerIndex > 0)
				{
					SetActivePlayerIndex(activePlayerIndex - 1);
				}
			}
		}
	}
}

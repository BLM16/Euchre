using System.Diagnostics;

namespace BLM16.Games.Euchre.Engine;

public abstract class ComputerPlayer : IComputerPlayer
{
	private const int _minHandScoreToCallTrump = 48;

	/// <inheritdoc/>
	public static Card PickCard(List<Card> hand, Suit trump, Card?[] playedCards, int leader, bool madeTrump)
	{
		// Computer leads as high as possible
		// Plays trump if they made it, otherwise the highest offsuit.
		if (playedCards[leader] is null)
		{
			return madeTrump ? PickHighestCardPreferringTrump(hand, trump) : PickHighestCardPreferringOffsuit(hand, trump);
		}

		var ledCard = playedCards[leader];
		var ledSuit = ledCard!.IsTrump(trump) ? trump : ledCard.Suit; // Account for left bower

		var winningCard = ledCard;
		foreach (var c in playedCards)
		{
			if (c is null) continue;
			if (c.IsHigherThan(winningCard, trump))
			{
				winningCard = c;
			}
		}

		var isPartnerWinning = false;
		switch (playedCards.Where(c => c is not null).Count())
		{
			case 2 when Array.IndexOf(playedCards, winningCard) == 0:
			case 3 when Array.IndexOf(playedCards, winningCard) == 1:
				isPartnerWinning = true;
				break;
		}

		var cardsFollowingSuit = hand.Where(c => c.Suit == ledSuit).ToList();
		if (cardsFollowingSuit.Count == 0)
		{
			if (playedCards.Where(c => c is not null).Count() == 3)
			{
				// We are the last player and our partner is winning so pick the lowest possible card
				if (isPartnerWinning)
				{
					return PickThrowAwayCard(hand, trump);
				}

				// We are the last player but our partner is losing so pick the lowest possible card that still wins
				Card? lowestWinningCardToPlay = null;
				foreach (var c in hand)
				{
					if (c.IsHigherThan(winningCard, trump))
					{
						// We can play a winning card
						lowestWinningCardToPlay ??= c;

						// Lower card that is still winning
						if (!c.IsHigherThan(lowestWinningCardToPlay, trump))
						{
							lowestWinningCardToPlay = c;
						}
					}
				}

				// We cannot win so play the lowest possible card
				lowestWinningCardToPlay ??= PickThrowAwayCard(hand, trump);
				return lowestWinningCardToPlay;
			}

			// Play our highest possible card to help win
			return PickHighestCardPreferringTrump(hand, trump);
		}

		// We are the last player and our partner is winning so pick the lowest possible card still following suit
		if (playedCards.Where(c => c is not null).Count() == 3 && isPartnerWinning)
		{
			return PickThrowAwayCard(cardsFollowingSuit, trump);
		}

		Card? cardToPlay = null;
		foreach (var c in cardsFollowingSuit)
		{
			// Ensure we are playing a winning card if possible
			if (c.IsHigherThan(winningCard, trump))
			{
				// We can play a winning card
				cardToPlay ??= c;

				// We are the last player so pick the lowest possible winning card
				if (playedCards.Where(c => c is not null).Count() == 3 && !c.IsHigherThan(cardToPlay, trump))
				{
					cardToPlay = c;
				}

				// Not the last player so pick the highest card to help win
				else if (c.IsHigherThan(cardToPlay, trump))
				{
					cardToPlay = c;
				}
			}
		}

		// If we can't win, play the lowest possible card
		cardToPlay ??= PickThrowAwayCard(cardsFollowingSuit, trump);

		return cardToPlay;
	}

	/// <inheritdoc/>
	public static bool OrderUpTrump(List<Card> hand, Suit trump)
		=> ScoreHandForSuit(hand, trump) >= _minHandScoreToCallTrump;

	/// <inheritdoc/>
	public static (bool call, Suit suit) BidOnTrump(List<Card> hand, Suit discountedSuit)
	{
		(int score, Suit suit) best = (0, default);
		
		// Loop through each suit and see if the player's hand for that suit is better
		var suits = Enum.GetValues(typeof(Suit)).Cast<Suit>();
		foreach (var suit in suits)
		{
			var score = ScoreHandForSuit(hand, suit);
			if (score > best.score)
			{
				best = (score, suit);
			}
		}

		// Make trump if the computer has a good enough hand
		if (best.score >= _minHandScoreToCallTrump)
		{
			return (true, best.suit);
		}

		// Computer doesn't want to make trump
		return (false, best.suit);
	}

	/// <summary>
	/// Picks the highest card possible. Prioritizes trump where possible.
	/// If not possible to play trump, it picks the highest offsuit card.
	/// </summary>
	/// <param name="cards">The cards to pick from</param>
	/// <param name="trump">The trump suit</param>
	private static Card PickHighestCardPreferringTrump(List<Card> cards, Suit trump)
	{
		var trumpCards = cards.Where(c => c.IsTrump(trump)).ToArray();
		if (trumpCards.Length == 0)
		{
			// No trump cards so pick the highest offsuit
			return PickHighestCardPreferringOffsuit(cards, trump);
		}

		var highest = trumpCards[0];
		foreach (var c in trumpCards)
		{
			// Accounts for bowers
			if (c.IsHigherThan(highest, trump))
			{
				highest = c;
			}
		}

		return highest;
	}

	/// <summary>
	/// Picks the highest card possible that is not a trump card.
	/// If that is not possible, it will pick the highest trump card.
	/// </summary>
	/// <param name="cards">The cards to pick from</param>
	/// <param name="trump">The trump suit</param>
	private static Card PickHighestCardPreferringOffsuit(List<Card> cards, Suit trump)
	{
		var nonTrumpCards = cards.Where(c => !c.IsTrump(trump)).ToArray();
		if (nonTrumpCards.Length == 0)
		{
			// No offsuit cards so pick the highest trump
			return PickHighestCardPreferringTrump(cards, trump);
		}

		var highest = nonTrumpCards[0];
		foreach (var c in nonTrumpCards)
		{
			// No need to account for bowers
			if (c.Rank > highest.Rank)
			{
				highest = c;
			}
		}

		return highest;
	}

	/// <summary>
	/// Pick the lowest possible card in the hand to throw away.
	/// </summary>
	/// <param name="cards">The cards to pick from</param>
	/// <param name="trump">The trump suit</param>
	private static Card PickThrowAwayCard(List<Card> cards, Suit trump)
	{
		var cardToPlay = cards[0];
		foreach (var c in cards)
		{
			if (!c.IsHigherThan(cardToPlay, trump))
			{
				cardToPlay = c;
			}
		}

		return cardToPlay;
	}

	/// <summary>
	/// Gets a score for the computer's hand given the trumpSuit.
	/// This can be used to judge how strong the computer's hand is.
	/// </summary>
	/// <param name="hand">The cards in the computer's hand</param>
	/// <param name="trumpSuit">The trump suit to evaluate the computer's hand with</param>
	/// <returns></returns>
	private static int ScoreHandForSuit(List<Card> hand, Suit trumpSuit)
	{
		var total = 0;

		foreach (var card in hand)
		{
			var cardScore = card.Rank switch
			{
				Rank.Nine => 0,
				Rank.Ten => 1,
				Rank.Jack => 3,
				Rank.Queen => 5,
				Rank.King => 6,
				Rank.Ace => 8,

				_ => throw new UnreachableException(),
			};

			// Weight trump much higher
			if (card.IsTrump(trumpSuit))
			{
				cardScore += 10;
				
				// Push bowers highest
				if (card.Rank == Rank.Jack)
				{
					if (card.Suit == trumpSuit)
					{
						cardScore += 9; // 22 total points for right bower
					}
					else
					{
						cardScore += 7; // 20 total points for left bower
					}
				}
			}

			total += cardScore;
		}

		return total;
	}
}

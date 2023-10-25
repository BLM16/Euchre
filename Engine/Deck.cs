namespace BLM16.Games.Euchre.Engine;

using System.Security.Cryptography;

/// <summary>
/// A deck of cards for Euchre.
/// </summary>
public class Deck
{
	private List<Card> _cards;

	public Deck()
	{
		_cards = AllCards;
		Shuffle();
	}

	/// <summary>
	/// Shuffle the order of the cards in the deck.
	/// </summary>
	public void Shuffle()
	{
		_cards = AllCards.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToList();
	}

	/// <summary>
	/// Use the current deck shuffle to deal the 4 hands and the kitty.
	/// </summary>
	/// <remarks>
	/// Deal starts left of the dealer thus index 0 is the player left of the dealer.
	/// </remarks>
	/// <returns>An array of all the hands, the kitty</returns>
	public (List<List<Card>>, Card[]) DealHands()
	{
		var hands = new Card[4][] { new Card[5], new Card[5], new Card[5], new Card[5] };

		// 4 players * 5 cards each = 20 cards
		for (var i = 0; i < 20; i++)
		{
			// i % 4 will cycle from 0-3 inclusive for the player
			// i / 4 increases from 0 to 5 for the card
			hands[i % 4][i / 4] = _cards[i];
		}

		// Kitty is the last 4 remaining cards
		var kitty = _cards.GetRange(20, 4).ToArray();

		return (hands.Select(a => a.ToList()).ToList(), kitty);
	}

	/// <summary>
	/// All the valid cards in the game (9 - A of all suits)
	/// </summary>
	private static List<Card> AllCards
	{
		get
		{
			var suits = Enum.GetValues(typeof(Suit)).Cast<Suit>();
			var ranks = Enum.GetValues(typeof(Rank)).Cast<Rank>();

			// For every rank, loop through the suits and create a card with that rank and suit
			return ranks.SelectMany(r => suits.Select(s => new Card(r, s))).ToList();
		}
	}
}

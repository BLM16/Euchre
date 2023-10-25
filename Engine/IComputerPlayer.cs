namespace BLM16.Games.Euchre.Engine;

public interface IComputerPlayer
{
	/// <summary>
	/// Picks the best card for the computer to play.
	/// </summary>
	/// <param name="hand">The cards in the computer player's hand</param>
	/// <param name="trump">The trump suit</param>
	/// <param name="playedCards">The cards already played in the current trick</param>
	/// <param name="leader">The offset of the player who led the trick</param>
	/// <param name="madeTrump">Did the computer player make the trump suit</param>
	/// <returns>The card for the computer to play</returns>
	public static abstract Card PickCard(List<Card> hand, Suit trump, Card?[] playedCards, int leader, bool madeTrump);

	/// <summary>
	/// Picks if the computer wants <paramref name="trump"/> as the trump suit.
	/// </summary>
	/// <param name="hand">The cards in the computer player's hand</param>
	/// <param name="trump">The turned up suit to decide trump</param>
	/// <returns>If the computer wants to order up the trump</returns>
	public static abstract bool OrderUpTrump(List<Card> hand, Suit trump);

	/// <summary>
	/// Picks if the computer wants to make trump or not, and what suit.
	/// </summary>
	/// <param name="hand">The cards in the computer player's hand</param>
	/// <param name="discountedSuit">The suit that was turned up that is no longer a valid choice</param>
	/// <returns>(does the computer want to make trump, the computer's best suit)</returns>
	public static abstract (bool call, Suit suit) BidOnTrump(List<Card> hand, Suit discountedSuit);
}

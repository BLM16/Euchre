namespace BLM16.Games.Engine.Tests;

using BLM16.Games.Euchre.Engine;

[TestClass]
public class ComputerPlayerTests
{
	/// <summary>
	/// As the trick leader the computer is expected to play as high as possible.
	/// If the computer made the trump, they should play their highest trump.
	/// If they did not make trump, they should play their highest offsuit.
	/// </summary>
	[TestMethod]
	public void PickCard_PicksCorrectCard_AsTrickLeaderAndTrumpMaker()
	{
		// Arrange
		var hand = new List<Card>()
		{
			new Card(Rank.Ace, Suit.Hearts),
			new Card(Rank.Jack, Suit.Diamonds),
			new Card(Rank.Queen, Suit.Spades),
			new Card(Rank.Queen, Suit.Diamonds),
			new Card(Rank.King, Suit.Hearts),
		};
		var trump = Suit.Hearts;
		var playedCards = new Card?[4];
		var leader = 0;
		var madeTrump = true;

		// Act
		var choice = ComputerPlayer.PickCard(hand, trump, playedCards, leader, madeTrump);

		// Assert
		Assert.AreEqual(new Card(Rank.Jack, Suit.Diamonds), choice);
	}

	/// <summary>
	/// As the trick leader the computer is expected to play as high as possible.
	/// If they did not make trump, they should play their highest offsuit.
	/// Cards are searched in order, Qspades = Qdiamonds in this context thus the first should be played.
	/// </summary>
	[TestMethod]
	public void PickCard_PicksCorrectCard_AsTrickLeader()
	{
		// Arrange
		var hand = new List<Card>()
		{
			new Card(Rank.Ace, Suit.Hearts),
			new Card(Rank.Jack, Suit.Diamonds),
			new Card(Rank.Queen, Suit.Spades),
			new Card(Rank.Queen, Suit.Diamonds),
			new Card(Rank.King, Suit.Hearts),
		};
		var trump = Suit.Hearts;
		var playedCards = new Card?[4];
		var leader = 0;
		var madeTrump = false;

		// Act
		var choice = ComputerPlayer.PickCard(hand, trump, playedCards, leader, madeTrump);

		// Assert
		Assert.AreEqual(new Card(Rank.Queen, Suit.Spades), choice);
	}

	/// <summary>
	/// The computer should follow suit when possible.
	/// </summary>
	[TestMethod]
	public void PickCard_PicksCorrectCard_FollowsSuitWithOneCardWhenPossible()
	{
		// Arrange
		var hand = new List<Card>()
		{
			new Card(Rank.Ace, Suit.Hearts),
			new Card(Rank.Jack, Suit.Diamonds),
			new Card(Rank.Queen, Suit.Spades),
			new Card(Rank.Queen, Suit.Diamonds),
			new Card(Rank.King, Suit.Hearts),
		};
		var trump = Suit.Hearts;
		var playedCards = new Card?[4]
		{
			new Card(Rank.Ace, Suit.Spades),
			null, null, null
		};
		var leader = 0;
		var madeTrump = false;

		// Act
		var choice = ComputerPlayer.PickCard(hand, trump, playedCards, leader, madeTrump);

		// Assert
		Assert.AreEqual(new Card(Rank.Queen, Suit.Spades), choice);
	}

	/// <summary>
	/// The computer should follow suit when possible.
	/// </summary>
	[TestMethod]
	public void PickCard_PicksCorrectCard_FollowsSuitWithMultipleCardsWhenPossible()
	{
		// Arrange
		var hand = new List<Card>()
		{
			new Card(Rank.Ace, Suit.Hearts),
			new Card(Rank.Jack, Suit.Diamonds),
			new Card(Rank.Queen, Suit.Spades),
			new Card(Rank.Queen, Suit.Diamonds),
			new Card(Rank.King, Suit.Hearts),
		};
		var trump = Suit.Hearts;
		var playedCards = new Card?[4]
		{
			new Card(Rank.Ace, Suit.Spades),
			new Card(Rank.Jack, Suit.Hearts),
			null, null
		};
		var leader = 0;
		var madeTrump = false;

		// Act
		var choice = ComputerPlayer.PickCard(hand, trump, playedCards, leader, madeTrump);

		// Assert
		Assert.AreEqual(new Card(Rank.Queen, Suit.Spades), choice);
	}

	/// <summary>
	/// The computer should follow suit when possible.
	/// If not possible, the computer should play the highest possible card.
	/// If the computer's partner is already winning and the computer is last,
	/// the computer should throw off.
	/// </summary>
	[TestMethod]
	public void PickCard_PicksCorrectCard_FollowsSuitAndThrowsOffWhenPartnerWinning()
	{
		// Arrange
		var hand = new List<Card>()
		{
			new Card(Rank.Ace, Suit.Hearts),
			new Card(Rank.Jack, Suit.Diamonds),
			new Card(Rank.Queen, Suit.Spades),
			new Card(Rank.Queen, Suit.Diamonds),
			new Card(Rank.King, Suit.Diamonds),
		};
		var trump = Suit.Diamonds;
		var playedCards = new Card?[4]
		{
			new Card(Rank.Ten, Suit.Diamonds),
			new Card(Rank.Ace, Suit.Diamonds), // Partner's hand is winning
			new Card(Rank.Ten, Suit.Clubs),
			null
		};
		var leader = 0;
		var madeTrump = false;

		// Act
		var choice = ComputerPlayer.PickCard(hand, trump, playedCards, leader, madeTrump);

		// Assert
		Assert.AreEqual(new Card(Rank.Queen, Suit.Diamonds), choice);
	}

	/// <summary>
	/// The computer should follow suit when possible.
	/// If not possible, the computer should play the highest possible card.
	/// </summary>
	[TestMethod]
	public void PickCard_PicksCorrectCard_PlaysHighestWhenFollowingSuitNotPossible()
	{
		// Arrange
		var hand = new List<Card>()
		{
			new Card(Rank.Ace, Suit.Hearts),
			new Card(Rank.Jack, Suit.Diamonds),
			new Card(Rank.Queen, Suit.Spades),
			new Card(Rank.Queen, Suit.Diamonds),
			new Card(Rank.King, Suit.Hearts),
		};
		var trump = Suit.Diamonds;
		var playedCards = new Card?[4]
		{
			new Card(Rank.King, Suit.Clubs),
			new Card(Rank.Queen, Suit.Hearts),
			null, null
		};
		var leader = 0;
		var madeTrump = false;

		// Act
		var choice = ComputerPlayer.PickCard(hand, trump, playedCards, leader, madeTrump);

		// Assert
		Assert.AreEqual(new Card(Rank.Jack, Suit.Diamonds), choice);
	}

	/// <summary>
	/// If the computer's partner is already winning and the computer is last,
	/// the computer should throw off.
	/// Cards are searched in order, Qspades = Qdiamonds in this context thus the first should be played.
	/// </summary>
	[TestMethod]
	public void PickCard_PicksCorrectCard_ThrowsOffWhenPartnerWinning()
	{
		// Arrange
		var hand = new List<Card>()
		{
			new Card(Rank.Ace, Suit.Hearts),
			new Card(Rank.Jack, Suit.Diamonds),
			new Card(Rank.Queen, Suit.Spades),
			new Card(Rank.Queen, Suit.Diamonds),
			new Card(Rank.King, Suit.Diamonds),
		};
		var trump = Suit.Diamonds;
		var playedCards = new Card?[4]
		{
			new Card(Rank.Ten, Suit.Clubs),
			new Card(Rank.Ace, Suit.Clubs), // Partner's hand is winning
			new Card(Rank.Jack, Suit.Clubs),
			null
		};
		var leader = 0;
		var madeTrump = false;

		// Act
		var choice = ComputerPlayer.PickCard(hand, trump, playedCards, leader, madeTrump);

		// Assert
		Assert.AreEqual(new Card(Rank.Queen, Suit.Spades), choice);
	}

	/// <summary>
	/// Computer plays a winning card if their partner is not winning.
	/// Should play the lowest possible winning card as the last player.
	/// </summary>
	[TestMethod]
	public void PickCard_PicksCorrectCard_PlaysLowestWinningCardAsLastPlayer()
	{
		// Arrange
		var hand = new List<Card>()
		{
			new Card(Rank.Ace, Suit.Hearts),
			new Card(Rank.Jack, Suit.Diamonds),
			new Card(Rank.Queen, Suit.Spades),
			new Card(Rank.Queen, Suit.Diamonds),
			new Card(Rank.King, Suit.Diamonds),
		};
		var trump = Suit.Diamonds;
		var playedCards = new Card?[4]
		{
			new Card(Rank.Ace, Suit.Clubs),
			new Card(Rank.Ten, Suit.Clubs), // Partner's hand is losing
			new Card(Rank.Jack, Suit.Clubs),
			null
		};
		var leader = 0;
		var madeTrump = false;

		// Act
		var choice = ComputerPlayer.PickCard(hand, trump, playedCards, leader, madeTrump);

		// Assert
		Assert.AreEqual(new Card(Rank.Queen, Suit.Diamonds), choice);
	}
}

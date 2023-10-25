namespace BLM16.Games.Euchre;

using BLM16.Games.Euchre.Engine;
using System.Globalization;

public partial class TutorialPage : ContentPage
{
	private TaskCompletionSource<bool> _inputCompletionSource;

	public TutorialPage()
	{
		_inputCompletionSource = new TaskCompletionSource<bool>();
		InitializeComponent();
		BindingContext = this;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Counters
		CountersView.IsVisible = true;
		TutorialMessage.Text = "These indicators show the current score and tricks for each team. They also show the current dealer and caller (who made trump).";
		await _inputCompletionSource.Task;
		_inputCompletionSource = new TaskCompletionSource<bool>();

		// Ordering up
		OrderUpView.IsVisible = true;
		BtnOrderUp.Source = (ImageSource)new Converters.CardSuitToImageSource().Convert(Suit.Hearts, typeof(ImageSource), null, CultureInfo.CurrentCulture);
		TutorialMessage.Text = "This menu will allow you to order up trump or pass.";
		await _inputCompletionSource.Task;
		_inputCompletionSource = new TaskCompletionSource<bool>();
		OrderUpView.IsVisible = false;

		// Bidding on trump
		TrumpBiddingView.IsVisible = true;
		TutorialMessage.Text = "This menu will allow you to select a trump suit or pass after everyone has passed.";
		await _inputCompletionSource.Task;
		_inputCompletionSource = new TaskCompletionSource<bool>();
		TrumpBiddingView.IsVisible = false;

		// The main view
		MainContent.IsVisible = true;
		TutorialMessage.Text = "This view will display the cards that have been played. The suit in the center is the current trump suit.";
		await _inputCompletionSource.Task;
		_inputCompletionSource = new TaskCompletionSource<bool>();

		// The player's hand
		PlayerHand.ItemsSource = new[]
		{
			new Card(Rank.Ace, Suit.Hearts),
			new Card(Rank.Queen, Suit.Diamonds),
			new Card(Rank.Jack, Suit.Spades),
			new Card(Rank.Ten, Suit.Clubs),
			new Card(Rank.Nine, Suit.Diamonds)
		};
		PlayerHand.IsVisible = true;
		TutorialMessage.Text = "This is your hand. You play a card by clicking it.";
		BtnNext.Text = "End Tutorial";
		await _inputCompletionSource.Task;
		await Shell.Current.GoToAsync("../");
	}

	private void BtnNext_Clicked(object sender, EventArgs e)
		=> _inputCompletionSource.SetResult(true);
}

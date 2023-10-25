namespace BLM16.Games.Euchre.Templates;

public partial class Card : ContentView
{
	public static readonly BindableProperty RankProperty = BindableProperty.Create(nameof(Rank), typeof(Engine.Rank), typeof(Card), null);
	public static readonly BindableProperty SuitProperty = BindableProperty.Create(nameof(Suit), typeof(Engine.Suit), typeof(Card), null);

	public Engine.Rank Rank
	{
		get => (Engine.Rank)GetValue(RankProperty);
		set => SetValue(RankProperty, value);
	}

	public Engine.Suit Suit
	{
		get => (Engine.Suit)GetValue(SuitProperty);
		set => SetValue(SuitProperty, value);
	}

	public Card()
	{
		InitializeComponent();
		BindingContext = this;
	}
}

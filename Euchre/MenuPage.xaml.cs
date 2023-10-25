namespace BLM16.Games.Euchre;

public partial class MenuPage : ContentPage
{
	public MenuPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	private async void BtnPlay_Clicked(object sender, EventArgs e)
		=> await Shell.Current.GoToAsync(nameof(MainPage));

	private async void BtnTutorial_Clicked(object sender, EventArgs e)
		=> await Shell.Current.GoToAsync(nameof(TutorialPage));
}

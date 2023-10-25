namespace BLM16.Games.Euchre;

public partial class AppShell : Shell
{
	public AppShell()
	{
		Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
		Routing.RegisterRoute(nameof(TutorialPage), typeof(TutorialPage));

		InitializeComponent();
	}
}

using SmartPlantApp.Services;

namespace SmartPlantApp;

public partial class MainPage : ContentPage
{
	int count = 0;

  private readonly MqttService _mqttService = new();

  public MainPage()
  {
    InitializeComponent();
  }

protected override async void OnAppearing()
  {
    base.OnAppearing();

    try
    {
      await _mqttService.ConnectAsync();
      Console.WriteLine("Connected to MQTT broker.");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"MQTT connection failed: {ex.Message}");
    }
  }

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}


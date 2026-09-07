using SmartPlantApp.Services;

namespace SmartPlantApp;

public partial class MainPage : ContentPage
{
    private readonly MqttService _mqttService = new();

    public MainPage()
    {
        InitializeComponent();

        _mqttService.MoistureReceived += moisture =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MoistureLabel.Text = moisture.ToString();
            });
        };

        _mqttService.TemperatureReceived += temperature =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TemperatureLabel.Text = $"{temperature:0.0} °C";
            });
        };
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
}

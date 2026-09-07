using SmartPlantApp.Services;
using System.Text.Json;

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

        _mqttService.HistoryReceived += payload =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ShowHistory(payload);
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

            await _mqttService.RequestHistoryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MQTT connection failed: {ex.Message}");
        }
    }

    private async void OnRefreshHistoryClicked(
        object sender,
        EventArgs e)
    {
        try
        {
            await _mqttService.RequestHistoryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"History request failed: {ex.Message}");
        }
    }

    private void ShowHistory(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            ReadingsHistoryLayout.Children.Clear();
            WateringHistoryLayout.Children.Clear();

            foreach (var reading in root.GetProperty("readings").EnumerateArray())
            {
                var measuredAt = reading.GetProperty("MeasuredAt").GetDateTime();
                var moisture = reading.GetProperty("Moisture").GetInt32();
                var temperature = reading.GetProperty("Temperature").GetDouble();
                var humidity = reading.GetProperty("Humidity").GetDouble();

                ReadingsHistoryLayout.Children.Add(
                    new Label
                    {
                        Text =
                            $"{measuredAt.ToLocalTime():dd-MM HH:mm} - " +
                            $"Bodem {moisture} - " +
                            $"Temp {temperature:0.0} °C - " +
                            $"Lucht {humidity:0.0}%"
                    });
            }

            foreach (var wateringEvent in
                     root.GetProperty("wateringEvents").EnumerateArray())
            {
                var startedAt =
                    wateringEvent.GetProperty("StartedAt").GetDateTime();

                var duration =
                    wateringEvent.GetProperty("DurationSeconds").GetInt32();

                var reason =
                    wateringEvent.GetProperty("Reason").GetString();

                WateringHistoryLayout.Children.Add(
                    new Label
                    {
                        Text =
                            $"{startedAt.ToLocalTime():dd-MM HH:mm} - " +
                            $"{duration} sec - {reason}"
                    });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to show history: {ex.Message}");
        }
    }
}

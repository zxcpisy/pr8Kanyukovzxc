using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pr8Kanyukov
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string API_URL = "https://api.weather.yandex.ru/v2/forecast";
        private const string API_KEY = "6c5c9d55-44c3-4922-82bb-9419557f85dd";
        private const string PERM_LATITUDE = "58.0105";
        private const string PERM_LONGITUDE = "56.2502";
        private readonly HttpClient _httpClient;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButton.IsEnabled = false;
            UpdateButton.Content = "Загрузка...";

            try
            {
                var weatherData = await GetWeatherDataAsync();
                WeatherDataGrid.ItemsSource = weatherData;
                MessageBox.Show($"Данные успешно обновлены!\nЗагружено записей: {weatherData.Count}",
                               "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных:\n{ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UpdateButton.IsEnabled = true;
                UpdateButton.Content = "Обновить данные о погоде";
            }
        }

        private async Task<List<WeatherInfo>> GetWeatherDataAsync()
        {
            string url = $"{API_URL}?lat={PERM_LATITUDE}&lon={PERM_LONGITUDE}&lang=ru_RU&limit=7&hours=true&extra=false";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Yandex-API-Key", API_KEY);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            return ParseWeatherData(jsonContent);
        }

        private List<WeatherInfo> ParseWeatherData(string jsonContent)
        {
            var weatherList = new List<WeatherInfo>();
            JObject json = JObject.Parse(jsonContent);

            var fact = json["fact"];
            if (fact != null)
            {
                weatherList.Add(new WeatherInfo
                {
                    DateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                    Temperature = $"{fact["temp"]}°C",
                    Pressure = $"{fact["pressure_mm"]} мм рт.ст.",
                    Humidity = $"{fact["humidity"]}%"
                });
            }

            var forecasts = json["forecasts"];
            if (forecasts != null)
            {
                foreach (var day in forecasts)
                {
                    string date = day["date"]?.ToString() ?? "";
                    DateTime parsedDate = DateTime.Parse(date);

                    var hours = day["hours"];
                    if (hours != null)
                    {
                        foreach (var hour in hours)
                        {
                            string hourValue = hour["hour"]?.ToString() ?? "00";
                            weatherList.Add(new WeatherInfo
                            {
                                DateTime = $"{parsedDate:dd.MM.yyyy} {hourValue}:00",
                                Temperature = $"{hour["temp"]}°C",
                                Pressure = $"{hour["pressure_mm"]} мм рт.ст.",
                                Humidity = $"{hour["humidity"]}%"
                            });
                        }
                    }
                }
            }

            return weatherList;
        }
    }

    public class WeatherInfo
    {
        public string DateTime { get; set; }
        public string Temperature { get; set; }
        public string Pressure { get; set; }
        public string Humidity { get; set; }
    }
}


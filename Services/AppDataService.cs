using System.Collections.ObjectModel;

namespace CultivoApp.Services
{
    public class AppDataService
    {
        private static AppDataService? _instance;
        public static AppDataService Instance => _instance ??= new AppDataService();

        public ObservableCollection<Mushroom> Mushrooms { get; }

        private AppDataService()
        {
            Mushrooms = new ObservableCollection<Mushroom>
            {
                new Mushroom
                {
                    Name = "Shimeji Marrom",
                    Temperature = "22–26 °C",
                    Humidity = "85–95%",
                    ImagePath = "Assets/shimeji_marrom.png"
                },
                new Mushroom
                {
                    Name = "Shimeji Rosa",
                    Temperature = "25–30 °C",
                    Humidity = "80–90%",
                    ImagePath = "Assets/shimeji_rosa.png"
                },
                new Mushroom
                {
                    Name = "Champignon",
                    Temperature = "18–22 °C",
                    Humidity = "80–90%",
                    ImagePath = "Assets/champignon.png"
                }
            };
        }
    }

    public class Mushroom
    {
        public string Name { get; set; } = "";
        public string Temperature { get; set; } = "";
        public string Humidity { get; set; } = "";
        public string ImagePath { get; set; } = "";
    }
}

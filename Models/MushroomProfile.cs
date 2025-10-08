namespace CultivoApp.Models
{
    public class MushroomProfile
    {
        public string Name { get; set; } = "Cogumelo Marrom";
        public (double Min, double Max) IdealTemperature { get; set; } = (22, 24);
        public (double Min, double Max) IdealHumidity { get; set; } = (80, 90);
    }
}
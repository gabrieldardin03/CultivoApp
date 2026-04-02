using System;

namespace CultivoApp.Models
{
    public class Mushroom
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Temperature { get; set; } = string.Empty; // Exibição geral
        public string Humidity { get; set; } = string.Empty;     // Exibição geral
        public string ImagePath { get; set; } = string.Empty;

        // 🔹 Novas propriedades específicas
        public double TempMin { get; set; }     // Temperatura mínima ideal
        public double TempMax { get; set; }     // Temperatura máxima ideal
        public double UmidMin { get; set; }     // Umidade mínima ideal
        public double UmidMax { get; set; }     // Umidade máxima ideal
    }
}

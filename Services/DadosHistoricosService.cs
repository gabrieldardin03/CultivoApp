// DadosHistoricosService.cs
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace CultivoApp.Services
{
    public class DadosHistoricosService
    {
        private static DadosHistoricosService? _instance;
        public static DadosHistoricosService Instance => _instance ??= new DadosHistoricosService();

        public ObservableCollection<DadoSensor> Registros { get; private set; } = new();

        private string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CultivoApp", "historico.json");

        private DadosHistoricosService()
        {
            Carregar();
        }

        public void Adicionar(double temperatura, double umidade)
        {
            var registro = new DadoSensor
            {
                Data = DateTime.Now,
                Temperatura = temperatura,
                Umidade = umidade
            };
            Registros.Add(registro);
            Salvar();
        }

        // ✅ NOVO: Função para limpar os dados
        public void Resetar()
        {
            Registros.Clear();
            Salvar();
        }

        public void Salvar()
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            var json = JsonSerializer.Serialize(Registros, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public void Carregar()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var dados = JsonSerializer.Deserialize<ObservableCollection<DadoSensor>>(json);
                if (dados != null)
                    Registros = dados;
            }
        }
    }

    public class DadoSensor
    {
        public DateTime Data { get; set; }
        public double Temperatura { get; set; }
        public double Umidade { get; set; }
    }
}
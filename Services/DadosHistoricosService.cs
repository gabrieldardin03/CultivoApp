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

        // Agora recebe os estados dos atuadores e do sistema
        public void Adicionar(double temperatura, double umidade, bool modoAuto, bool aquecedor, bool umidificador, bool exaustor)
        {
            var registro = new DadoSensor
            {
                Data = DateTime.Now,
                Temperatura = temperatura,
                Umidade = umidade,
                ModoAutomatico = modoAuto,
                AquecedorLigado = aquecedor,
                UmidificadorLigado = umidificador,
                ExaustorLigado = exaustor
            };
            Registros.Add(registro);
            Salvar();
        }

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

    // Novas propriedades adicionadas ao modelo para salvar a "fotografia" do momento
    public class DadoSensor
    {
        public DateTime Data { get; set; }
        public double Temperatura { get; set; }
        public double Umidade { get; set; }
        public bool ModoAutomatico { get; set; }
        public bool AquecedorLigado { get; set; }
        public bool UmidificadorLigado { get; set; }
        public bool ExaustorLigado { get; set; }
    }
}
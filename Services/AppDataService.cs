// AppDataService.cs
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CultivoApp.Models;
using CultivoApp.Services;

namespace CultivoApp.Services
{
    public class AppDataService
    {
        private static AppDataService? _instance;
        public static AppDataService Instance => _instance ??= new AppDataService();

        public ObservableCollection<Mushroom> Mushrooms { get; set; } = new();
        public Mushroom? LastSelected { get; set; }

        public double TemperaturaAtual { get; private set; }
        public double UmidadeAtual { get; private set; }
        public DateTime UltimaLeitura { get; private set; } = DateTime.MinValue;

        public event Action? DadosAtualizados;

        private AppDataService() => LoadData();

        // ✅ Atualizado para receber os estados reais vindos da estufa
        public void AtualizarDadosSensor(double temperatura, double umidade, bool modoAuto, bool aquecedor, bool umidificador, bool exaustor)
        {
            TemperaturaAtual = temperatura;
            UmidadeAtual = umidade;
            UltimaLeitura = DateTime.Now;

            // Salva no histórico persistente com o que estava ligado/desligado de verdade
            DadosHistoricosService.Instance.Adicionar(temperatura, umidade, modoAuto, aquecedor, umidificador, exaustor);
            
            // Dispara o evento para atualizar a interface/gráficos
            DadosAtualizados?.Invoke();
        }

        public void DispararAtualizacao() => DadosAtualizados?.Invoke();

        public void SaveData(Mushroom m)
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "last_selected.json");
                var json = JsonSerializer.Serialize(m, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                LastSelected = m;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar último cogumelo: {ex.Message}");
            }
        }

        public void LoadData()
        {
            try
            {
                var basePath = AppContext.BaseDirectory;
                var mushroomsPath = Path.Combine(basePath, "mushrooms.json");
                var lastPath = Path.Combine(basePath, "last_selected.json");

                if (File.Exists(mushroomsPath))
                {
                    var json = File.ReadAllText(mushroomsPath);
                    Mushrooms = JsonSerializer.Deserialize<ObservableCollection<Mushroom>>(json) ?? new();
                }

                if (File.Exists(lastPath))
                {
                    var json = File.ReadAllText(lastPath);
                    LastSelected = JsonSerializer.Deserialize<Mushroom>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar dados: {ex.Message}");
            }
        }

        public void SaveAll()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "mushrooms.json");
                var json = JsonSerializer.Serialize(Mushrooms, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar dados: {ex.Message}");
            }
        }
    }
}
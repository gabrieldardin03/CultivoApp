// HistoricoViewModel.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CultivoApp.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using OfficeOpenXml;
using SkiaSharp;

namespace CultivoApp.ViewModels
{
    public partial class HistoricoViewModel : ObservableObject
    {
        private readonly List<double> _temperaturas = new();
        private readonly List<double> _umidades = new();
        
        private const int LeiturasPorMinuto = 12;
        private int _maxPontosDoGrafico = 30 * LeiturasPorMinuto;
        private string _tituloEixoX = "Últimos 30 Minutos (Tempo Real)";

        public ISeries[] TemperaturaSeries { get; set; }
        public ISeries[] UmidadeSeries { get; set; }

        [ObservableProperty] 
        private Axis[] eixoX = { new Axis { Name = "Histórico", LabelsRotation = 0 } };
        
        [ObservableProperty] 
        private Axis[] eixoY = { new Axis { Name = "Valor", NamePaint = new SolidColorPaint(SKColors.Black) } };

        [ObservableProperty] private string mediaTemperaturaTexto = "Média: --";
        [ObservableProperty] private string desvioTemperaturaTexto = "Desvio: --";
        [ObservableProperty] private string mediaUmidadeTexto = "Média: --";
        [ObservableProperty] private string desvioUmidadeTexto = "Desvio: --";

        [ObservableProperty]
        private string textoLogSd = "Clique no botão 'Buscar Logs' para carregar os erros do ESP32...";

        public HistoricoViewModel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            TemperaturaSeries = new ISeries[]
            {
                new LineSeries<double> { Values = _temperaturas, Stroke = new SolidColorPaint(SKColors.OrangeRed, 2) }
            };
            UmidadeSeries = new ISeries[]
            {
                new LineSeries<double> { Values = _umidades, Stroke = new SolidColorPaint(SKColors.DeepSkyBlue, 2) }
            };

            EixoX = new Axis[] { new Axis { Name = _tituloEixoX, LabelsRotation = 0 } };
            
            AppDataService.Instance.DadosAtualizados += AtualizarHistoricoAoVivo;
        }

        private void AtualizarHistoricoAoVivo()
        {
            double temp = AppDataService.Instance.TemperaturaAtual;
            double umi = AppDataService.Instance.UmidadeAtual;

            _temperaturas.Add(temp);
            _umidades.Add(umi);

            while (_temperaturas.Count > _maxPontosDoGrafico) _temperaturas.RemoveAt(0);
            while (_umidades.Count > _maxPontosDoGrafico) _umidades.RemoveAt(0);

            TemperaturaSeries[0].Values = _temperaturas.ToArray();
            UmidadeSeries[0].Values = _umidades.ToArray();

            AtualizarEstatisticas();
        }

        [RelayCommand]
        private void MudarPeriodo(string? periodo)
        {
            periodo ??= "30M";

            switch (periodo)
            {
                case "5M":
                    _maxPontosDoGrafico = 5 * LeiturasPorMinuto;
                    _tituloEixoX = "Últimos 5 Minutos (Tempo Real)";
                    break;
                case "1H":
                    _maxPontosDoGrafico = 60 * LeiturasPorMinuto;
                    _tituloEixoX = "Última 1 Hora (Tempo Real)";
                    break;
                case "30M":
                default:
                    _maxPontosDoGrafico = 30 * LeiturasPorMinuto;
                    _tituloEixoX = "Últimos 30 Minutos (Tempo Real)";
                    break;
            }

            EixoX = new Axis[] { new Axis { Name = _tituloEixoX, LabelsRotation = 0 } };
            
            while (_temperaturas.Count > _maxPontosDoGrafico) _temperaturas.RemoveAt(0);
            while (_umidades.Count > _maxPontosDoGrafico) _umidades.RemoveAt(0);
        }
        
        // ================= COLUNA ESQUERDA: EXCEL DE VALORES =================
        [RelayCommand]
        private async Task ExportarHistorico()
        {
            var topLevel = GetTopLevel();
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Salvar Histórico de Valores (Excel)",
                SuggestedFileName = $"Historico_Valores_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                DefaultExtension = "xlsx",
                FileTypeChoices = new[] { new FilePickerFileType("Excel Workbook") { Patterns = new[] { "*.xlsx" } } }
            });

            if (file is null) return;

            var dados = DadosHistoricosService.Instance.Registros;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Valores");
                
                worksheet.Cells[1, 1].Value = "Data";
                worksheet.Cells[1, 2].Value = "Temperatura (°C)";
                worksheet.Cells[1, 3].Value = "Umidade (%)";
                worksheet.Cells[1, 4].Value = "Modo Automático";
                worksheet.Cells[1, 5].Value = "Aquecedor";
                worksheet.Cells[1, 6].Value = "Umidificador";
                worksheet.Cells[1, 7].Value = "Exaustor";

                for (int i = 0; i < dados.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = dados[i].Data.ToString("g");
                    worksheet.Cells[i + 2, 2].Value = dados[i].Temperatura;
                    worksheet.Cells[i + 2, 3].Value = dados[i].Umidade;
                    worksheet.Cells[i + 2, 4].Value = dados[i].ModoAutomatico ? "Sim" : "Não";
                    worksheet.Cells[i + 2, 5].Value = dados[i].AquecedorLigado ? "Ligado" : "Desligado";
                    worksheet.Cells[i + 2, 6].Value = dados[i].UmidificadorLigado ? "Ligado" : "Desligado";
                    worksheet.Cells[i + 2, 7].Value = dados[i].ExaustorLigado ? "Ligado" : "Desligado";
                }
                
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                await using (var stream = await file.OpenWriteAsync())
                {
                    package.SaveAs(stream);
                }
            }
        }

        [RelayCommand]
        private void ResetarHistorico()
        {
            DadosHistoricosService.Instance.Resetar();
            _temperaturas.Clear();
            _umidades.Clear();
            AtualizarEstatisticas();
        }

        // ================= COLUNA DIREITA: EXCEL DE ERROS =================
        [RelayCommand]
        private async Task BaixarLogSdAsync()
        {
            TextoLogSd = "A carregar histórico de erros do ESP32...";
            string resultado = await ApiService.DownloadLogEsp32Async();
            TextoLogSd = resultado;
        }

        [RelayCommand]
        private async Task ExportarErrosExcelAsync()
        {
            if (string.IsNullOrWhiteSpace(TextoLogSd) || TextoLogSd.StartsWith("Clique no botão"))
            {
                return;
            }

            var topLevel = GetTopLevel();
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Salvar Histórico de Erros (Excel)",
                SuggestedFileName = $"Historico_Erros_ESP32_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                DefaultExtension = "xlsx",
                FileTypeChoices = new[] { new FilePickerFileType("Excel Workbook") { Patterns = new[] { "*.xlsx" } } }
            });

            if (file is null) return;

            var linhas = TextoLogSd.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Erros e Logs");

                worksheet.Cells[1, 1].Value = "Linha";
                worksheet.Cells[1, 2].Value = "Registro de Log / Erro do ESP32";

                for (int i = 0; i < linhas.Length; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = i + 1;
                    worksheet.Cells[i + 2, 2].Value = linhas[i];
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                await using (var stream = await file.OpenWriteAsync())
                {
                    package.SaveAs(stream);
                }
            }
        }

        [RelayCommand]
        private void ResetarErros()
        {
            TextoLogSd = "Histórico de erros local limpo. Clique em 'Buscar Logs' para recarregar do ESP32.";
        }

        private void AtualizarEstatisticas()
        {
            if (_temperaturas.Count == 0)
            {
                MediaTemperaturaTexto = "Média: --";
                DesvioTemperaturaTexto = "Desvio: --";
                MediaUmidadeTexto = "Média: --";
                DesvioUmidadeTexto = "Desvio: --";
                return;
            }

            double mediaTemp = _temperaturas.Average();
            double desvioTemp = Math.Sqrt(_temperaturas.Average(v => Math.Pow(v - mediaTemp, 2)));
            double mediaUmi = _umidades.Average();
            double desvioUmi = Math.Sqrt(_umidades.Average(v => Math.Pow(v - mediaUmi, 2)));

            MediaTemperaturaTexto = $"Média: {mediaTemp:F1} °C";
            DesvioTemperaturaTexto = $"Desvio: {desvioTemp:F1}";
            MediaUmidadeTexto = $"Média: {mediaUmi:F1} %";
            DesvioUmidadeTexto = $"Desvio: {desvioUmi:F1}";
        }
        
        private TopLevel? GetTopLevel()
        {
            return (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }
    }
}
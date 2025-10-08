using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace CultivoApp.ViewModels
{
    public class MainWindowViewModel
    {
        public PlotModel GraficoModel { get; }

        public MainWindowViewModel()
        {
            GraficoModel = new PlotModel { Title = "Monitoramento da Estufa" };

            // Eixo X
            GraficoModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Tempo (minutos)"
            });

            // Eixo Y
            GraficoModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Valor"
            });

            // Série de temperatura
            var temperatura = new LineSeries
            {
                Title = "Temperatura (°C)",
                Color = OxyColors.OrangeRed,
                StrokeThickness = 2
            };
            temperatura.Points.Add(new DataPoint(0, 22));
            temperatura.Points.Add(new DataPoint(1, 23));
            temperatura.Points.Add(new DataPoint(2, 24));
            temperatura.Points.Add(new DataPoint(3, 25));

            // Série de umidade
            var umidade = new LineSeries
            {
                Title = "Umidade (%)",
                Color = OxyColors.SkyBlue,
                StrokeThickness = 2
            };
            umidade.Points.Add(new DataPoint(0, 80));
            umidade.Points.Add(new DataPoint(1, 82));
            umidade.Points.Add(new DataPoint(2, 81));
            umidade.Points.Add(new DataPoint(3, 83));

            // Adiciona ao gráfico
            GraficoModel.Series.Add(temperatura);
            GraficoModel.Series.Add(umidade);
        }
    }
}

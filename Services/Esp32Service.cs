using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading.Tasks;
using CultivoApp.Models;

namespace CultivoApp.Services
{
    public class Esp32Service : IDisposable
    {
        private readonly SerialPort? _serialPort;
        private readonly Random _rand = new Random();

        public Esp32Service(string? portName = null, int baudRate = 115200)
        {
            if (!string.IsNullOrWhiteSpace(portName))
            {
                try
                {
                    _serialPort = new SerialPort(portName, baudRate)
                    {
                        NewLine = "\n",
                        ReadTimeout = 1000,
                        WriteTimeout = 1000
                    };
                    _serialPort.Open();
                }
                catch
                {
                    // se falhar ao abrir a porta, usa null e cairá no modo simulado
                    _serialPort = null;
                }
            }
        }

        public async Task<SensorData> GetSensorDataAsync()
        {
            // Se porta aberta, tenta ler formato esperado: ex "T:24.3;H:78.5"
            if (_serialPort is not null && _serialPort.IsOpen)
            {
                try
                {
                    // solicitar leitura (depende do firmware no ESP)
                    // _serialPort.WriteLine("READ");
                    string line = await Task.Run(() => _serialPort.ReadLine()?.Trim() ?? string.Empty);

                    double temp = 0;
                    double hum = 0;
                    
                    // Exemplo de parse de dados:
                    // Se a linha for "T:24.3;H:78.5", ele extrai a temperatura e umidade
                    string[] parts = line.Split(';');
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (trimmed.StartsWith("T:"))
                        {
                            double.TryParse(trimmed.Substring(2), NumberStyles.Any, CultureInfo.InvariantCulture, out temp);
                        }
                        else if (trimmed.StartsWith("H:"))
                        {
                            double.TryParse(trimmed.Substring(2), NumberStyles.Any, CultureInfo.InvariantCulture, out hum);
                        }
                    }

                    return new SensorData
                    {
                        Temperature = temp,
                        Humidity = hum,
                        Timestamp = DateTime.Now
                    };
                }
                catch
                {
                    // em caso de erro de leitura, cai no simulador abaixo
                }
            }

            // Simulação (fallback)
            await Task.Delay(800);
            return new SensorData
            {
                Temperature = Math.Round(23 + _rand.NextDouble() * 2, 1),
                Humidity = Math.Round(78 + _rand.NextDouble() * 4, 1),
                Timestamp = DateTime.Now
            };
        }

        public void Dispose()
        {
            try
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                }
            }
            catch { }
        }
    }
}
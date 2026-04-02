using System;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CultivoApp.Models; // IMPORTANTE

public enum TipoConexao
{
    Serial,
    Wifi
}

public class Esp32Service
{
    private readonly TipoConexao _tipoConexao;
    private SerialPort? _serialPort;
    private HttpClient? _httpClient;
    private string? _ip;

    public Esp32Service(TipoConexao tipo, string? ip = null, string? portaSerial = null)
    {
        _tipoConexao = tipo;

        if (_tipoConexao == TipoConexao.Serial)
        {
            if (string.IsNullOrEmpty(portaSerial))
                throw new ArgumentException("Porta serial não informada.");

            _serialPort = new SerialPort(portaSerial, 115200);
            _serialPort.ReadTimeout = 3000;
            _serialPort.Open();
        }
        else
        {
            if (string.IsNullOrEmpty(ip))
                throw new ArgumentException("IP não informado.");

            _ip = ip;

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }
    }

    // ==============================
    // BUSCAR DADOS
    // ==============================

    public async Task<SensorData?> BuscarDadosAsync()
    {
        try
        {
            if (_tipoConexao == TipoConexao.Serial)
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                    return null;

                string linha = _serialPort.ReadLine();
                linha = linha.Trim();

                if (!linha.StartsWith("T:"))
                    return null;

                var partes = linha.Replace("T:", "")
                                  .Replace("H:", "")
                                  .Split(';');

                return new SensorData
                {
                    Temperatura = float.Parse(partes[0]),
                    Umidade = float.Parse(partes[1])
                };
            }
            else
            {
                if (_httpClient == null || _ip == null)
                    return null;

                string url = $"http://{_ip}/getData";
                var response = await _httpClient.GetStringAsync(url);

                return JsonSerializer.Deserialize<SensorData>(response);
            }
        }
        catch
        {
            return null;
        }
    }
}
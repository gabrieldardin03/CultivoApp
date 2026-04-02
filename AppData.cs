using EstufaInteligente.Services;

namespace EstufaInteligente
{
    public static class AppData
    {
        public static SensorService Sensor { get; } = new SensorService();

        static AppData()
        {
            Sensor.Iniciar();
        }
    }
}

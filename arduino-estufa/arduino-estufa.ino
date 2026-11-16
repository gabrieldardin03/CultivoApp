// ================================================================
// SISTEMA DE ESTUFA - CONTROLE TOTAL (DHCP, RECUPERAÇÃO HÍBRIDA, WATCHDOG V3 E MEMÓRIA DE ESTADO)
// ================================================================

// 1. Aqui importo todas as ferramentas que vou usar: Wi-Fi, servidor web, json, sensor, cartão SD e memória do sistema.
#include <WiFi.h>
#include <WebServer.h>
#include <ArduinoJson.h>
#include <Wire.h>
#include <Adafruit_SHT31.h>
#include <ArduinoOTA.h>
#include <ESPmDNS.h>
#include <Preferences.h>
#include "time.h"
#include "FS.h"
#include "SPI.h"
#include "SD.h"
#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"

// ================= HARDWARE =================

// 2. Defino os pinos onde liguei cada peça.
#define PINO_UMIDIFICADOR 25
#define PINO_EXAUSTOR     26
#define PINO_AQUECEDOR    27
#define SD_CS_PIN 5

// Como uso módulo relé padrão, a lógica é invertida (LOW liga, HIGH desliga).
const uint8_t RELE_LIGAR = LOW;
const uint8_t RELE_DESLIGAR = HIGH;

// ================= WIFI =================

const char* ssid = "Wi-fi DAVID";
const char* senha = "ga263333";

// ================= NTP =================

// 3. Configuração de relógio pra pegar a hora da internet (NTP).
const char* ntpServer = "pool.ntp.org";
const long gmtOffset_sec = -10800; // Fuso horário de Brasília (-3h)
const int daylightOffset_sec = 0;

// ================= OBJETOS =================

Adafruit_SHT31 sht31 = Adafruit_SHT31();
WebServer server(80);
Preferences preferencias; // Pra salvar as configurações na memória Flash

WiFiServer telnetServer(23);
WiFiClient telnetClient;

// ================= VARIÁVEIS DE CONTROLE =================

float temperatura = 0;
float umidade = 0;
bool modoAutomatico = true;
int falhasConsecutivas = 0;

String nomeCultura;
float tempMin, tempMax;
float umidMin, umidMax;

unsigned long sensorMillis = 0;
const long sensorInterval = 5000;
unsigned long ultimaComunicacao = 0;

// ================= MEMÓRIA DE SOBREVIVÊNCIA (RTC) =================
// 4. Se o ESP32 travar e reiniciar, essas variáveis NÃO apagam. Assim lembro como os relés estavam.
RTC_DATA_ATTR bool estadoAutoUmid = false;
RTC_DATA_ATTR bool estadoAutoAquec = false;
RTC_DATA_ATTR bool estadoAutoExaust = false;
RTC_DATA_ATTR bool cicloExaustorAtivo = false; 
RTC_DATA_ATTR bool houveTravamento = false;

// ================= CONTROLE DO SD =================
bool sdIniciado = false;
unsigned long ultimaTentativaSD = 0;
const unsigned long intervaloTentativaSD = 15000;

// ================= LIMPEZA SENSOR =================
bool limpandoSensor = false;
unsigned long ultimoCicloLimpeza = 0;
const unsigned long intervaloLimpezaSeguro = 3600000; 
unsigned long inicioLimpeza = 0;
const unsigned long tempoHeaterLigado = 2000;      
const unsigned long tempoEstabilizacao = 3000;    

// ================= TROCA DE AR (CO2) =================
unsigned long ultimoCicloExaustor = 0;
const unsigned long intervaloTrocaAr = 900000;    
const unsigned long tempoExaustorLigado = 60000;  
unsigned long ultimaChecagemWifi = 0;

// ================= WATCHDOG (CÃO DE GUARDA - V3) =================
hw_timer_t *watchdogTimer = NULL;

// 5. Se o código entrar em loop infinito ou travar feio, o cão de guarda reinicia a placa na marra aqui.
void IRAM_ATTR resetPorWatchdog() {
  ets_printf("TRAVAMENTO TOTAL DETECTADO! Reiniciando placa na marra...\n");
  esp_restart();
}

// ================= RECUPERAÇÃO I2C =================
// 6. Função salva-vidas: Se o sensor SHT31 travar por ruído do relé, dou uns "choques" manuais nos pinos pra ele desengasgar.
void recuperarBarramentoI2CHard() {
  Wire.end();
  pinMode(21, OUTPUT);
  pinMode(22, OUTPUT);
  digitalWrite(21, HIGH);
  digitalWrite(22, HIGH);
  delay(1);
  for (int i = 0; i < 9; i++) {
    digitalWrite(22, LOW);
    delayMicroseconds(10);
    digitalWrite(22, HIGH);
    delayMicroseconds(10);
  }
  delay(10);
  Wire.begin(21, 22);
  Wire.setClock(100000); // 100kHz padrão seguro para estabilidade
  Wire.setTimeOut(1000); 
  sht31.begin(0x44);
}

// ================= DATA E LOG =================
String getDataHora() {
  struct tm timeinfo;
  if (!getLocalTime(&timeinfo)) return "00/00/0000 00:00:00";
  char buffer[25];
  strftime(buffer, sizeof(buffer), "%d/%m/%Y %H:%M:%S", &timeinfo);
  return String(buffer);
}

// 7. Crio uma mensagem de log e mando para o cabo (Serial), para a rede (Telnet) e salvo no Cartão SD.
void salvarLog(String msg) {
  String linha = "[" + getDataHora() + "] " + msg;
  Serial.println(linha);
  if (telnetClient && telnetClient.connected()) telnetClient.println(linha);
  if (sdIniciado) {
    File file = SD.open("/log.txt", FILE_APPEND);
    if (file) { file.println(linha); file.close(); } 
    else { sdIniciado = false; }
  }
}

// ================= SETUP =================
void setup() {
  // 8. Desativo o detector de queda de tensão pra placa não reiniciar quando os relés derem pico de energia.
  WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0); 
  
  Serial.begin(115200);
  delay(1000);

  // Inicializo o cão de guarda com 10 segundos de limite.
  watchdogTimer = timerBegin(1000000); 
  timerAttachInterrupt(watchdogTimer, &resetPorWatchdog);
  timerAlarm(watchdogTimer, 10000000, true, 0); 

  recuperarBarramentoI2CHard();

  // Resgato as configurações que salvei na memória flash (temperaturas ideais, etc).
  preferencias.begin("estufa", false);
  nomeCultura = preferencias.getString("cultura", "Nenhuma Configurada");
  modoAutomatico = preferencias.getBool("modoAuto", true);
  tempMin = preferencias.getFloat("tempMin", 18.0);
  tempMax = preferencias.getFloat("tempMax", 25.0);
  umidMin = preferencias.getFloat("umidMin", 90.0);
  umidMax = preferencias.getFloat("umidMax", 95.0);

  // DIAGNÓSTICO DE BOOT: Imprimo para ver se puxou certo da memória
  Serial.println("\n--- DADOS RESGATADOS DA MEMÓRIA FLASH ---");
  Serial.print("Cultura Atual: "); Serial.println(nomeCultura);
  Serial.print("Temperatura Min/Max: "); Serial.print(tempMin); Serial.print(" / "); Serial.println(tempMax);
  Serial.print("Umidade Min/Max: "); Serial.print(umidMin); Serial.print(" / "); Serial.println(umidMax);
  Serial.println("-----------------------------------------\n");

  pinMode(PINO_UMIDIFICADOR, OUTPUT);
  pinMode(PINO_EXAUSTOR, OUTPUT);
  pinMode(PINO_AQUECEDOR, OUTPUT);

  // 9. Se reiniciei de um travamento (RTC), volto os relés EXATAMENTE pra onde estavam pra não ferrar as plantas.
  if (!modoAutomatico) {
    bool umidSalvo = preferencias.getBool("releUmid", false);
    bool aquecSalvo = preferencias.getBool("releAquec", false);
    bool exaustSalvo = preferencias.getBool("releExaust", false);

    digitalWrite(PINO_UMIDIFICADOR, umidSalvo ? RELE_LIGAR : RELE_DESLIGAR);
    digitalWrite(PINO_AQUECEDOR, aquecSalvo ? RELE_LIGAR : RELE_DESLIGAR);
    digitalWrite(PINO_EXAUSTOR, exaustSalvo ? RELE_LIGAR : RELE_DESLIGAR);
    Serial.println("Estado MANUAL restaurado.");
  } else {
    if (houveTravamento) {
      digitalWrite(PINO_UMIDIFICADOR, estadoAutoUmid ? RELE_LIGAR : RELE_DESLIGAR);
      digitalWrite(PINO_AQUECEDOR, estadoAutoAquec ? RELE_LIGAR : RELE_DESLIGAR);
      digitalWrite(PINO_EXAUSTOR, estadoAutoExaust ? RELE_LIGAR : RELE_DESLIGAR);
      
      if (cicloExaustorAtivo) ultimoCicloExaustor = millis(); 
      Serial.println("Estado AUTOMATICO restaurado após travamento.");
    } else {
      digitalWrite(PINO_UMIDIFICADOR, RELE_DESLIGAR);
      digitalWrite(PINO_AQUECEDOR, RELE_DESLIGAR);
      digitalWrite(PINO_EXAUSTOR, RELE_DESLIGAR);
    }
  }
  houveTravamento = true; 

  SPI.begin(18, 19, 23, SD_CS_PIN);
  delay(100);
  if (SD.begin(SD_CS_PIN)) sdIniciado = true;
  
  WiFi.begin(ssid, senha);
  unsigned long wifiStart = millis();
  while (WiFi.status() != WL_CONNECTED && millis() - wifiStart < 15000) {
    delay(500); Serial.print(".");
    timerWrite(watchdogTimer, 0); // Fico zerando o cão de guarda enquanto tento conectar no Wi-Fi.
  }
  WiFi.setSleep(false);
  ultimaComunicacao = millis();
  if (MDNS.begin("estufa")) Serial.println("http://estufa.local");
  configTime(gmtOffset_sec, daylightOffset_sec, ntpServer);

  // ================= API =================
  // 10. Aqui crio as "portas" da API pro meu app/site conversar com a placa.
  server.on("/getData", HTTP_GET, []() {
    ultimaComunicacao = millis();
    DynamicJsonDocument doc(512); 
    doc["cultura"] = nomeCultura;
    doc["temperatura"] = temperatura;
    doc["umidade"] = umidade;
    doc["modoAutomatico"] = modoAutomatico;
    doc["sdConectado"] = sdIniciado;
    doc["tempMin"] = tempMin;
    doc["tempMax"] = tempMax;
    doc["umidMin"] = umidMin;
    doc["umidMax"] = umidMax;

    // Envio o estado dos relés pro app saber o que está ligado
    doc["umidificadorLigado"] = (digitalRead(PINO_UMIDIFICADOR) == RELE_LIGAR);
    doc["ventiladorLigado"]   = (digitalRead(PINO_AQUECEDOR) == RELE_LIGAR);
    doc["exaustorLigado"]     = (digitalRead(PINO_EXAUSTOR) == RELE_LIGAR);

    String json;
    serializeJson(doc, json);
    server.send(200, "application/json", json);
  });

  server.on("/updateConfig", HTTP_POST, []() {
    ultimaComunicacao = millis();
    if (!server.hasArg("plain")) { server.send(400, "application/json", "{\"status\":\"erro\"}"); return; }
    String body = server.arg("plain");
    
    DynamicJsonDocument doc(512);
    if (deserializeJson(doc, body)) { server.send(400, "application/json", "{\"status\":\"erro json\"}"); return; }

    if (doc.containsKey("cultura")) { 
      nomeCultura = doc["cultura"].as<String>(); 
      preferencias.putString("cultura", nomeCultura); 
      salvarLog("Cultura alterada para: " + nomeCultura); // Loga imediatamente a mudança no PuTTY/SD
    }
    if (doc.containsKey("modoAutomatico")) { modoAutomatico = doc["modoAutomatico"]; preferencias.putBool("modoAuto", modoAutomatico); }
    if (doc.containsKey("tempMin") && doc["tempMin"] > 0) tempMin = doc["tempMin"]; 
    if (doc.containsKey("tempMax") && doc["tempMax"] > 0) tempMax = doc["tempMax"]; 
    if (doc.containsKey("umidMin") && doc["umidMin"] > 0) umidMin = doc["umidMin"]; 
    if (doc.containsKey("umidMax") && doc["umidMax"] > 0) umidMax = doc["umidMax"]; 

    preferencias.putFloat("tempMin", tempMin);
    preferencias.putFloat("tempMax", tempMax);
    preferencias.putFloat("umidMin", umidMin);
    preferencias.putFloat("umidMax", umidMax);

    if (!modoAutomatico) {
      if (doc.containsKey("umidificador")) {
        bool umid = doc["umidificador"];
        digitalWrite(PINO_UMIDIFICADOR, umid ? RELE_LIGAR : RELE_DESLIGAR);
        preferencias.putBool("releUmid", umid);
      }
      if (doc.containsKey("ventilador")) {
        bool aquec = doc["ventilador"];
        digitalWrite(PINO_AQUECEDOR, aquec ? RELE_LIGAR : RELE_DESLIGAR);
        preferencias.putBool("releAquec", aquec);
      }
      if (doc.containsKey("exaustor")) {
        bool exaust = doc["exaustor"];
        digitalWrite(PINO_EXAUSTOR, exaust ? RELE_LIGAR : RELE_DESLIGAR);
        preferencias.putBool("releExaust", exaust);
      }
    }
    server.send(200, "application/json", "{\"status\":\"ok\"}");
  });

  // 11. NOVA ROTA: Permite que o app baixe o arquivo de log completo do SD Card
  server.on("/downloadLog", HTTP_GET, []() {
    if (!sdIniciado) {
      server.send(500, "text/plain", "ERRO: Cartão SD não disponível.");
      return;
    }
    File logFile = SD.open("/log.txt");
    if (!logFile) {
      server.send(404, "text/plain", "ERRO: Arquivo log.txt não encontrado no SD.");
      return;
    }
    // Manda o arquivo inteiro como texto puro
    server.streamFile(logFile, "text/plain");
    logFile.close();
  });

  server.begin();
  telnetServer.begin();
  telnetServer.setNoDelay(true);
  ArduinoOTA.setHostname("estufa");
  ArduinoOTA.begin();
  salvarLog("SISTEMA_INICIADO");
}

// ================= LOOP =================
void loop() {
  // 12. Faço carinho no cão de guarda toda vez que passo pelo loop, assim ele sabe que não travei.
  timerWrite(watchdogTimer, 0); 

  server.handleClient();
  ArduinoOTA.handle();
  
  if (telnetServer.hasClient()) {
    if (!telnetClient || !telnetClient.connected()) telnetClient = telnetServer.available();
    else telnetServer.available().stop();
  }

  unsigned long agora = millis();
  static int falhasWifi = 0;
  // Se o Wi-fi cair, tento reconectar. Se falhar 6x seguidas, reinicio a placa pra arrumar de vez.
  if (WiFi.status() != WL_CONNECTED) {
    if (agora - ultimaChecagemWifi >= 10000) {
      ultimaChecagemWifi = agora;
      falhasWifi++;
      WiFi.disconnect(); 
      WiFi.reconnect();
      if (falhasWifi >= 6) { delay(1000); ESP.restart(); }
    }
  } else { falhasWifi = 0; }

  if (!sdIniciado && (agora - ultimaTentativaSD >= intervaloTentativaSD)) {
    ultimaTentativaSD = agora;
    if (SD.begin(SD_CS_PIN)) sdIniciado = true;
  }

  // 13. Prevenção: O sensor satura fácil. Aqui ligo o aquecedor interno dele pra secar a água acumulada.
  if (!limpandoSensor && (agora - ultimoCicloLimpeza >= intervaloLimpezaSeguro)) {
    salvarLog("LIMPEZA_SENSOR_INICIO");
    limpandoSensor = true;
    inicioLimpeza = agora;
    ultimoCicloLimpeza = agora;
    Wire.beginTransmission(0x44); Wire.write(0x30); Wire.write(0x66); Wire.endTransmission();
  }

  if (limpandoSensor) {
    if (agora - inicioLimpeza >= tempoHeaterLigado) {
      Wire.beginTransmission(0x44); Wire.write(0x30); Wire.write(0x67); Wire.endTransmission();
    }
    if (agora - inicioLimpeza >= (tempoHeaterLigado + tempoEstabilizacao)) {
      limpandoSensor = false; salvarLog("LIMPEZA_SENSOR_FIM");
    }
  }

  // 14. Leitura do Sensor e Proteção.
  if (!limpandoSensor && (agora - sensorMillis >= sensorInterval)) {
    sensorMillis = agora;
    float t = sht31.readTemperature();
    float h = sht31.readHumidity();

    // Verifico se o dado lido não é erro ou algo bizarro (ex: Temp 200 graus).
    if (!isnan(t) && !isnan(h) && t > -40 && t < 125 && h >= 0 && h <= 100) {
      falhasConsecutivas = 0; // Deu bom, zero os erros.
      temperatura = t;
      umidade = h;
      
      if (modoAutomatico) controlarAmbiente();

      String statusLog = "Temp:" + String(t,1) + "C Umid:" + String(h,1) + "%";
      statusLog += modoAutomatico ? " | Modo Auto" : " | Modo Manual";

      if (digitalRead(PINO_AQUECEDOR) == RELE_LIGAR) statusLog += " | Aquec. Ligado";
      if (digitalRead(PINO_UMIDIFICADOR) == RELE_LIGAR) statusLog += " | Umid. Ligado";
      if (digitalRead(PINO_EXAUSTOR) == RELE_LIGAR) statusLog += " | Exaust. Ligado";

      salvarLog(statusLog);
      
    } else {
      // 15. Se a leitura vier bugada, desligo tudo por segurança (pra não cozinhar as plantas por engano).
      falhasConsecutivas++;
      digitalWrite(PINO_UMIDIFICADOR, RELE_DESLIGAR);
      digitalWrite(PINO_AQUECEDOR, RELE_DESLIGAR);
      digitalWrite(PINO_EXAUSTOR, RELE_DESLIGAR);
      
      if (falhasConsecutivas == 1) {
        salvarLog("AVISO: Falha I2C. Recuperando (Bit-Bang)...");
        recuperarBarramentoI2CHard(); // Tento desengasgar o I2C com uns choques elétricos.
      } else if (falhasConsecutivas >= 2) {
        salvarLog("ERRO CRITICO: Sensor SHT31 travado. Reiniciando!");
        delay(1000); ESP.restart(); // Se falhar 2x, jogo a toalha e reinicio a placa.
      }
    }
  }
}

// ================= CONTROLE AUTOMÁTICO =================
void controlarAmbiente() {
  unsigned long agora = millis();

  // Ponto de equilíbrio: Calcula exatamente o meio ideal da cultura (Ex: Shimeji Branco = 22°C)
  float tempAlvo = (tempMin + tempMax) / 2.0;

  // 1. UMIDADE
  if (umidade <= umidMin) digitalWrite(PINO_UMIDIFICADOR, RELE_LIGAR);
  else if (umidade >= umidMax) digitalWrite(PINO_UMIDIFICADOR, RELE_DESLIGAR);

  // 2. AQUECEDOR (Liga no frio extremo e desliga no conforto)
  if (temperatura <= tempMin) {
    digitalWrite(PINO_AQUECEDOR, RELE_LIGAR);
  } 
  else if (temperatura >= tempAlvo) {
    digitalWrite(PINO_AQUECEDOR, RELE_DESLIGAR);
  }

  // 3. CICLO DO EXAUSTOR (Renovação de Ar / CO2)
  if (!cicloExaustorAtivo && (agora - ultimoCicloExaustor >= intervaloTrocaAr)) {
    cicloExaustorAtivo = true;
    ultimoCicloExaustor = agora;
  }
  if (cicloExaustorAtivo && (agora - ultimoCicloExaustor >= tempoExaustorLigado)) {
    cicloExaustorAtivo = false;
    ultimoCicloExaustor = agora; 
  }

  // 4. LÓGICA DE RESFRIAMENTO INDEPENDENTE
  // Uma variável isolada impede que o ESP32 confunda o resfriamento com a simples troca de ar de 1 minuto
  static bool modoResfriamento = false;
  
  if (temperatura >= tempMax) {
    modoResfriamento = true;  // Estufa bateu no teto? Liga o modo de emergência
  } 
  else if (temperatura <= tempAlvo) {
    modoResfriamento = false; // Refrescou até o meio ideal? Desliga a emergência
  }

  // O Exaustor liga se for a hora de trocar o CO2 (1 min) OU se a estufa estiver superaquecida
  if (cicloExaustorAtivo || modoResfriamento) {
    digitalWrite(PINO_EXAUSTOR, RELE_LIGAR);
  } else {
    digitalWrite(PINO_EXAUSTOR, RELE_DESLIGAR);
  }

  // Salvo o estado atual de tudo pra Memória RTC não esquecer em caso de travamento.
  estadoAutoUmid = (digitalRead(PINO_UMIDIFICADOR) == RELE_LIGAR);
  estadoAutoAquec = (digitalRead(PINO_AQUECEDOR) == RELE_LIGAR);
  estadoAutoExaust = (digitalRead(PINO_EXAUSTOR) == RELE_LIGAR);
}
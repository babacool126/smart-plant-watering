#include <DHT.h>

// Relais / pomp
const int RELAY_PIN = 7;

// Bodemvochtsensor
const int MOISTURE_PIN = A0;
const unsigned long MOISTURE_INTERVAL = 1000;

const int MOISTURE_DRY = 456;
const int MOISTURE_WET = 200;
const int MOISTURE_THRESHOLD = 280;

unsigned long lastMoistureRead = 0;
int lastMoistureRaw = 0;
int lastMoisturePercent = 0;

// DHT11
const int DHT_PIN = 2;
const int DHT_TYPE = DHT11;
const unsigned long DHT_INTERVAL = 2000;

DHT dht(DHT_PIN, DHT_TYPE);

unsigned long lastDhtRead = 0;
float lastTemperature = 0.0;
float lastHumidity = 0.0;
String serialCommand = "";

void setup() {
  Serial.begin(9600);

  dht.begin();

  // Relais instellen
  pinMode(RELAY_PIN, OUTPUT);

  // De relaismodule is HIGH-triggered:
  // LOW = relais uit / pomp uit
  // HIGH = relais aan / pomp aan
  digitalWrite(RELAY_PIN, LOW);

  Serial.println("Plant sensors gestart");
}

void loop() {
  unsigned long now = millis();

  // Commando's ontvangen via USB/Serial
  if (Serial.available() > 0) {
    serialCommand = Serial.readStringUntil('\n');
    serialCommand.trim();

    if (serialCommand == "PUMP_ON") {
      digitalWrite(RELAY_PIN, HIGH);
      Serial.println("Pomp AAN");
    }
    else if (serialCommand == "PUMP_OFF") {
      digitalWrite(RELAY_PIN, LOW);
      Serial.println("Pomp UIT");
    }
  }

  // Bodemvocht meten
  if (now - lastMoistureRead >= MOISTURE_INTERVAL) {
    lastMoistureRead = now;

    lastMoistureRaw = analogRead(MOISTURE_PIN);

    lastMoisturePercent = map(
      lastMoistureRaw,
      MOISTURE_DRY,
      MOISTURE_WET,
      0,
      100
    );

    lastMoisturePercent = constrain(
      lastMoisturePercent,
      0,
      100
    );

    Serial.print("Bodemvocht raw: ");
    Serial.print(lastMoistureRaw);
    Serial.print(" (");
    Serial.print(lastMoisturePercent);
    Serial.println("%)");

    if (lastMoistureRaw > MOISTURE_THRESHOLD) {
      Serial.println("-> grond te droog, plant heeft water nodig");
    }
  }

  // DHT11 meten
  if (now - lastDhtRead >= DHT_INTERVAL) {
    lastDhtRead = now;

    lastHumidity = dht.readHumidity();
    lastTemperature = dht.readTemperature();

    if (isnan(lastHumidity) || isnan(lastTemperature)) {
      Serial.println("Fout bij uitlezen DHT11");
    } else {
      Serial.print("Temperatuur: ");
      Serial.print(lastTemperature);
      Serial.println(" C");

      Serial.print("Luchtvochtigheid: ");
      Serial.print(lastHumidity);
      Serial.println("%");
    }
  }
}

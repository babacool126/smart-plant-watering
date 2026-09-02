// plant_sensors.ino

const int MOISTURE_PIN = A0;
const unsigned long MOISTURE_INTERVAL = 1000;

// Kalibratiewaarden, gemeten op jouw sensor
const int MOISTURE_DRY = 456;  // sensor in lucht / zeer droog
const int MOISTURE_WET = 200;  // natte grond

// Boven deze ruwe waarde beschouwen we de grond als te droog
const int MOISTURE_THRESHOLD = 280;

unsigned long lastMoistureRead = 0;
int lastMoistureRaw = 0;
int lastMoisturePercent = 0;

void setup() {
  Serial.begin(9600);
  Serial.println("Plant sensors gestart (non-blocking)");
}

void loop() {
  unsigned long now = millis();

  if (now - lastMoistureRead >= MOISTURE_INTERVAL) {
    lastMoistureRead = now;

    lastMoistureRaw = analogRead(MOISTURE_PIN);

    // Hoge raw = droog, lage raw = nat.
    // Daarom mappen we droog naar 0% en nat naar 100%.
    lastMoisturePercent = map(
      lastMoistureRaw,
      MOISTURE_DRY,
      MOISTURE_WET,
      0,
      100
    );

    lastMoisturePercent = constrain(lastMoisturePercent, 0, 100);

    Serial.print("Bodemvocht raw: ");
    Serial.print(lastMoistureRaw);
    Serial.print(" (");
    Serial.print(lastMoisturePercent);
    Serial.println("%)");

    if (lastMoistureRaw > MOISTURE_THRESHOLD) {
      Serial.println("-> grond te droog, plant heeft water nodig");
    }
  }
}

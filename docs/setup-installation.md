# Setup- en installatiehandleiding

Dit document beschrijft hoe het Smart Plant Watering-systeem wordt opgebouwd, gestart en getest.

De installatie bestaat uit de volgende onderdelen:

1. Arduino UNO met sensoren en pomp
2. Mosquitto MQTT-broker
3. PostgreSQL-database
4. C# Serial/MQTT-gateway
5. .NET MAUI Android-app

## 1. Vereisten

Voor het uitvoeren van het systeem zijn de volgende componenten nodig:

### Hardware

- Arduino UNO R3
- Bodemvochtsensor
- DHT11
- 5V relaismodule
- Dompelpomp 3-6V
- Batterijpack 4x AA
- USB-kabel voor de Arduino
- Android-telefoon

### Software

- Arduino IDE
- .NET 8 SDK
- Android SDK / ADB
- Mosquitto
- PostgreSQL
- Podman
- Git

## 2. Arduino

De Arduino leest de aangesloten sensoren uit en bestuurt het relais van de waterpomp.

Sluit de Arduino UNO via USB aan op de Windows-computer.

Open vervolgens de Arduino-sketch uit het project in de Arduino IDE.

Selecteer:

- Board: Arduino UNO
- De juiste seriële COM-poort (bijv. COM3)

Compileer en upload daarna de sketch naar de Arduino.

Na het uploaden stuurt de Arduino sensormetingen via USB-serial naar de C# Serial/MQTT-gateway.

De Arduino bestuurt daarnaast de pomp via de relaismodule.

## 3. Mosquitto

De Mosquitto MQTT-broker draait op de Linux-omgeving.

De broker wordt gebruikt voor communicatie tussen de Serial/MQTT-gateway en de .NET MAUI-app.

De configuratie en deploymentbestanden voor Mosquitto zijn beschikbaar in:

[`infrastructure/mosquitto/`](../infrastructure/mosquitto/)

De gebruikte MQTT-topics en JSON-berichtformaten zijn beschreven in:

[`mqtt-contract.md`](mqtt-contract.md)


## 4. PostgreSQL

PostgreSQL wordt gebruikt voor persistente opslag van sensormetingen en bewateringsacties.

De database draait als rootless Podman-container op de Linux-omgeving en wordt beheerd via systemd/Quadlet.

De PostgreSQL-data wordt opgeslagen in een persistent Podman-volume, zodat de databasegegevens behouden blijven wanneer de container opnieuw wordt aangemaakt of de VM wordt herstart.

De containerconfiguratie maakt gebruik van een aparte environmentfile voor de database-instellingen. Deze environmentfile is alleen leesbaar voor de `postgres`-gebruiker.

Het relationele databaseschema is beschreven in:

[`relational-schema.md`](relational-schema.md)

De database bevat onder andere de tabellen:

- `Plants`
- `SensorReadings`
- `WateringEvents`
- `__EFMigrationsHistory`

De database-initialisatie en wijzigingen in het schema worden vanuit de C#-applicatie beheerd via Entity Framework Core-migraties.

De PostgreSQL-container wordt gestart als systemd user service:

```bash
systemctl --user start plant-postgres.service
```

## 5. Serial/MQTT-gateway

De C# Serial/MQTT-gateway draait op de Windows-computer waarop de Arduino via USB is aangesloten.

De gateway vormt de koppeling tussen de Arduino, Mosquitto en PostgreSQL.

De gateway:

- leest sensormetingen via USB-serial;
- publiceert sensormetingen naar MQTT;
- ontvangt pompcommando's via MQTT;
- stuurt pompcommando's door naar de Arduino;
- slaat sensormetingen en bewateringsacties op in PostgreSQL;
- levert historische metingen via MQTT aan de mobiele app.

### Seriële verbinding

De Arduino wordt via USB aangesloten en gebruikt:

```text
COM3
9600 baud
```

Controleer vóór het starten van de gateway of de Arduino als `COM3` beschikbaar is.

Wanneer Windows een andere COM-poort toewijst, moet de COM-poort in `Program.cs` worden aangepast.

### MQTT

De gateway maakt verbinding met de Mosquitto-broker op:

```text
node-01.lab.thomaslab.nl:1883
```

De gebruikte MQTT-topics en JSON-berichtformaten zijn beschreven in:

[`mqtt-contract.md`](mqtt-contract.md)

### PostgreSQL-verbinding

De PostgreSQL-connectionstring wordt niet in de broncode opgeslagen.

De gateway leest deze uit de volgende environmentvariabele:

```text
PLANT_DB_CONNECTION
```

Deze environmentvariabele moet in PowerShell zijn ingesteld voordat de gateway wordt gestart.

### Gateway starten

Ga vanuit de repository naar de gateway:

```powershell
cd .\SerialMqttGateway
```

Start vervolgens de gateway:

```powershell
dotnet run
```

De gateway moet tijdens het gebruik van het systeem actief blijven.

Wanneer de gateway niet draait, worden geen nieuwe sensormetingen via MQTT gepubliceerd en verschijnen er geen actuele waarden in de mobiele app.

## 6. .NET MAUI-app bouwen

Ga in PowerShell naar de directory van de MAUI-app:

```powershell
cd SmartPlantApp
```

Maak vervolgens een standalone ARM64 Release-build:

```powershell
dotnet publish .\SmartPlantApp.csproj -f net8.0-android -c Release -r android-arm64
```

De APK wordt aangemaakt in:

```text
bin\Release\net8.0-android\android-arm64\publish\
```

De Android application ID is:

```text
nl.thomaslab.smartplantapp
```

De huidige applicatieversie is:

```text
1.0.0
```

De gegenereerde APK heet:

```text
nl.thomaslab.smartplantapp-Signed.apk
```

Een Release ARM64 APK moet worden gebruikt voor standalone installatie op een fysiek Android-apparaat. Een Debug-build kan afhankelijk zijn van Fast Deployment en daardoor niet correct functioneren wanneer deze los wordt geïnstalleerd.

## 7. Android-app installeren

Sluit de Android-telefoon via USB aan.

Controleer eerst of ADB het apparaat ziet:

```powershell
adb devices
```

Installeer daarna de APK:

```powershell
adb -s <device-id> install .\bin\Release\net8.0-android\android-arm64\publish\nl.thomaslab.smartplantapp-Signed.apk
```

Wanneer de installatie succesvol is, kan de telefoon van USB worden losgekoppeld.

De Smart Plant-app kan daarna zelfstandig op de telefoon worden gestart.

## 8. End-to-end-test

Voor een volledige systeemtest:

1. Sluit de Arduino via USB aan op de Windows-computer.
2. Start Mosquitto.
3. Start PostgreSQL.
4. Start de C# Serial/MQTT-gateway.
5. Open de Smart Plant-app op de Android-telefoon.
6. Controleer of actuele sensorwaarden in de app zichtbaar zijn.
7. Controleer of de pomp volgens de ingestelde logica kan worden aangestuurd.

De communicatiestroom is:

```text
Arduino
   |
   | USB-serial
   v
C# Serial/MQTT-gateway
   |                 |
   | MQTT            | EF Core
   v                 v
Mosquitto         PostgreSQL
   |
   v
.NET MAUI-app
```

## 9. Verdere configuratie

Voor technische details:

- MQTT-contract: [`mqtt-contract.md`](mqtt-contract.md)
- Relationeel schema: [`relational-schema.md`](relational-schema.md)

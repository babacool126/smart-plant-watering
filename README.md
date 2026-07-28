IoT-# smart-plant-watering

IoT-systeem voor automatische plantenbewatering met bodemvocht- en temperatuursensoren, Arduino, MQTT en een .NET MAUI-app.

## Inhoud

- [Probleemstelling](#probleemstelling)
- [Architectuur](#architectuur)
- [Techstack en keuzes](#techstack-en-keuzes)
- [Concurrency-aanpak](#concurrency-aanpak)
- [Hardware](#hardware)
- [Setup en installatie](#setup-en-installatie)
- [Demo](#demo)
- [Mogelijke uitbreidingen](#mogelijke-uitbreidingen)

## Probleemstelling

Kamerplanten hebben regelmatig water nodig, maar dit wordt in de praktijk vaak vergeten of gebeurt op basis van een inschatting. Hierdoor krijgen planten soms te weinig of juist te veel water, wat hun gezondheid negatief beïnvloedt. Het doel van dit project is om een IoT-systeem te ontwikkelen dat de bodemvochtigheid en temperatuur meet, deze gegevens registreert en op basis van een ingestelde drempel automatisch een plant kan bewateren. De scope van het project is beperkt tot één plant, één bodemvochtsensor, één temperatuursensor en één waterpomp, waarbij de gebruiker de metingen en bewateringsacties kan volgen via een .NET MAUI-app.

## Architectuur

### Systeemoverzicht

```
                  .NET MAUI GUI
                  (presentatielaag)
                        |
                  Service layer
                  (orkestreert taken)
                        |
        +---------------+----------------+
        |               |                |
  Serial service   MQTT service   Database service
  (Arduino comm.)  (pub/sub)      (EF Core repository)
        |               |                |
        v               v                v
   Arduino UNO     Mosquitto broker    Database
   (sensoren,       (lokaal)
    relais, pomp)
```
 
- **GUI → service layer**: de GUI kent alleen de service layer, niet de onderliggende techniek (Arduino/MQTT/database).
- **Service layer → serial service**: seriële communicatie (USB) met de Arduino UNO — sensordata ontvangen, pompcommando's versturen.
- **Service layer → MQTT service**: publiceert sensordata naar topics, luistert op commando-topics (MQTTnet).
- **Service layer → database service**: schrijft metingen en waterbeurten weg via EF Core.
*TODO: exacte topic-namen en berichtformaten (bv. JSON-payload per topic) toevoegen zodra vastgesteld.*
 
*TODO: korte toelichting per pijl (wat gaat er precies over, welk protocol/formaat).*

### Klassendiagram (conceptueel model)

```mermaid
classDiagram
  class Plant {
    +int id
    +string naam
    +int vochtDrempel
  }
  class SensorReading {
    +int id
    +datetime tijdstip
    +int bodemvocht
    +float temperatuur
  }
  class WateringEvent {
    +int id
    +datetime tijdstip
    +int duurSeconden
  }
  Plant "1" --> "*" SensorReading : heeft
  Plant "1" --> "*" WateringEvent : heeft
```

### Relationeel schema (logisch model)

*TODO: ERD toevoegen (mermaid erDiagram), afgeleid van het klassendiagram hierboven.*

## Techstack en keuzes

| Onderdeel | Keuze | Motivatie |
|---|---|---|
| Microcontroller | Arduino UNO R3 | *TODO* |
| App | .NET MAUI | Opvolger van Xamarin, C#-ervaring herbruikbaar |
| Communicatie Arduino ↔ app | USB serial | Geen extra hardware nodig, simpel te implementeren |
| Communicatie app ↔ backend | MQTT (MQTTnet) | Standaardprotocol voor IoT, dekt socketcommunicatie-leerdoel |
| Broker | Mosquitto (lokaal) | Geen internetafhankelijkheid tijdens demo |
| Opslag | EF Core (ORM) | *TODO: welke database (SQLite/SQL Server)* |

## Concurrency-aanpak

*TODO: beschrijf de drie taken die parallel lopen in de MAUI-app:*
- *Task 1: seriële uitlezing Arduino*
- *Task 2: MQTT publish/subscribe*
- *Task 3: automatische bewateringslogica (periodiek)*

*Beschrijf ook de thread-safe queue tussen reader en publisher, en de lock/semaphore rond de pompstatus om race conditions te voorkomen.*

## Hardware

- Arduino UNO R3
- Bodemvochtsensor (analoog)
- DHT11 (temperatuur)
- 5V relaismodule
- Dompelpomp 3-6V (Cerioll) + slang
- Batterijpack 4x AA (6V) voor de pomp

*Zie het bedradingsschema (los toe te voegen, bv. als afbeelding in `/docs`).*

## Setup en installatie

*TODO:*
1. *Arduino: welke libraries, welke sketch uploaden*
2. *Mosquitto: installatie en configuratie*
3. *MAUI-app: hoe te builden en te runnen*
4. *Database: connection string / migratie-commando's*

## Demo

*TODO: link naar het demofilmpje (Projectafsluiting).*

## Mogelijke uitbreidingen

- Tweede plant (extra bodemvochtsensor + extra pomp, zelfde architectuur)
- Waterniveausensor in het reservoir (voorkomt droog draaien van de pomp)
- Wireless communicatie Arduino ↔ app (ESP32/Bluetooth) i.p.v. USB serialsysteem voor automatische plantenbewatering met bodemvocht- en temperatuursensoren, Arduino, MQTT en een .NET MAUI-app.

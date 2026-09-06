# Smart Plant Watering

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

Het relationele schema is afgeleid van het klassendiagram en beschreven in
[`docs/relational-schema.md`](docs/relational-schema.md).

## Techstack en keuzes

| Onderdeel | Keuze | Motivatie |
|---|---|---|
| Microcontroller | Arduino UNO R3 | *TODO* |
| App | .NET MAUI | Opvolger van Xamarin, C#-ervaring herbruikbaar |
| Communicatie Arduino ↔ app | USB serial | Geen extra hardware nodig, simpel te implementeren |
| Communicatie app ↔ backend | MQTT (MQTTnet) | Standaardprotocol voor IoT, dekt socketcommunicatie-leerdoel |
| Broker | Mosquitto (lokaal) | Geen internetafhankelijkheid tijdens demo |
| Opslag | PostgreSQL + EF Core | Centrale relationele opslag; PostgreSQL draait als Podman-container met persistente opslag |

## Databasekeuze

Voor de persistente opslag van gegevens is gekozen voor **PostgreSQL**.

Voor het project zijn SQLite en PostgreSQL overwogen. SQLite heeft als voordeel
dat geen aparte databaseserver nodig is en is daardoor eenvoudig te gebruiken
voor lokale opslag. Voor dit project is echter gekozen voor centrale opslag op
de Linux-server.

PostgreSQL wordt als Podman-container uitgevoerd, naast de andere services van
het systeem. De database kan hierdoor gebruikmaken van het bestaande
containernetwerk. De databasegegevens worden opgeslagen in een persistent
volume, zodat deze behouden blijven wanneer de container opnieuw wordt
aangemaakt.

PostgreSQL sluit daarnaast goed aan op het relationele schema met `Plant`,
`SensorReading` en `WateringEvent`.

Hoewel PostgreSQL meer configuratie en beheer vereist dan SQLite, is deze extra
complexiteit binnen dit project beperkt doordat voor de infrastructuur al
Podman-containers worden gebruikt.

### Vergelijking

| Eigenschap | SQLite | PostgreSQL |
|---|---|---|
| Architectuur | Lokale database | Client/server-database |
| Aparte databaseservice | Nee | Ja |
| Centrale opslag | Minder geschikt | Ja |
| Containeriseerbaar | Niet noodzakelijk | Ja |
| Persistentie | Databasebestand | Persistent volume |
| Beheer | Eenvoudig | Meer configuratie |
| Geschikt voor dit project | Ja | **Gekozen** |

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
- Wireless communicatie Arduino ↔ app (ESP32/Bluetooth) i.p.v. USB serial



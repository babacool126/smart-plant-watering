# Relationeel schema

Dit document beschrijft het relationele databaseschema van het Smart Plant Watering-systeem.

Het schema is afgeleid van het klassendiagram met de klassen `Plant`, `SensorReading` en `WateringEvent`.

## Databaseschema

```mermaid
erDiagram
    PLANT ||--o{ SENSOR_READING : heeft
    PLANT ||--o{ WATERING_EVENT : heeft

    PLANT {
        int PlantId PK
        string Name
        int MoistureThreshold
    }

    SENSOR_READING {
        int SensorReadingId PK
        int PlantId FK
        int Moisture
        float Temperature
        float Humidity
        datetime MeasuredAt
    }

    WATERING_EVENT {
        int WateringEventId PK
        int PlantId FK
        datetime StartedAt
        int DurationSeconds
        string Reason
    }
```

## Tabellen

### Plant

De tabel `Plant` bevat de gegevens van een plant.

`PlantId` is de primaire sleutel en identificeert iedere plant uniek.

### SensorReading

De tabel `SensorReading` bevat de metingen van de sensoren, zoals bodemvocht, temperatuur en luchtvochtigheid.

Iedere meting is gekoppeld aan een plant via `PlantId`.

### WateringEvent

De tabel `WateringEvent` registreert wanneer een plant water heeft gekregen.

Iedere bewateringsactie is gekoppeld aan een plant via `PlantId`.

## Relaties

Een `Plant` kan nul of meerdere `SensorReading`-records hebben. Iedere `SensorReading` hoort bij precies één `Plant`.

Een `Plant` kan nul of meerdere `WateringEvent`-records hebben. Ieder `WateringEvent` hoort bij precies één `Plant`.

`PlantId` in `SensorReading` en `WateringEvent` is een vreemde sleutel (foreign key) die verwijst naar `Plant.PlantId`.

# Relationeel schema

Dit document beschrijft het relationele databaseschema van het Smart Plant Watering-systeem.

Het schema is afgeleid van het klassendiagram met de klassen `Plant`, `SensorReading` en `WateringEvent`.

```mermaid
erDiagram
    PLANT ||--o{ SENSOR_READING : has
    PLANT ||--o{ WATERING_EVENT : has

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

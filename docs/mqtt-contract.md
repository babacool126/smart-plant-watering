# MQTT topic- en berichtcontract

Dit document beschrijft de MQTT-topics en JSON-berichtformaten die worden gebruikt binnen het Smart Plant Watering-systeem.

Het contract wordt gedeeld door de Serial/MQTT-gateway en de .NET MAUI-applicatie.

## MQTT-topics

| Topic                       | Richting       | Doel                       |
| --------------------------- | -------------- | -------------------------- |
| `plant/sensors/moisture`    | Gateway → MQTT | Bodemvochtmeting           |
| `plant/sensors/temperature` | Gateway → MQTT | Temperatuurmeting          |
| `plant/sensors/humidity`    | Gateway → MQTT | Luchtvochtigheidsmeting    |
| `plant/pump/command`        | MQTT → Gateway | Commando voor de pomp      |
| `plant/pump/status`         | Gateway → MQTT | Huidige status van de pomp |

## Berichtformaten

### Bodemvocht

Topic: `plant/sensors/moisture`

```json
{
  "value": 387,
  "unit": "raw"
}
```

De waarde is de ruwe analoge meetwaarde die afkomstig is van de bodemvochtsensor.

### Temperatuur

Topic: `plant/sensors/temperature`

```json
{
  "value": 22.4,
  "unit": "celsius"
}
```

### Luchtvochtigheid

Topic: `plant/sensors/humidity`

```json
{
  "value": 54.2,
  "unit": "percent"
}
```

### Pompcommando

Topic: `plant/pump/command`

De pomp inschakelen:

```json
{
  "action": "on"
}
```

De pomp uitschakelen:

```json
{
  "action": "off"
}
```

De pomp kan optioneel voor een bepaalde tijd worden ingeschakeld:

```json
{
  "action": "on",
  "durationSeconds": 5
}
```

`durationSeconds` is optioneel. Wanneer deze waarde aanwezig is, moet de pomp na het opgegeven aantal seconden automatisch stoppen.

### Pompstatus

Topic: `plant/pump/status`

Wanneer de pomp actief is:

```json
{
  "state": "on"
}
```

Wanneer de pomp uitgeschakeld is:

```json
{
  "state": "off"
}
```

## Communicatiestroom

De Arduino leest de sensoren uit en stuurt de meetgegevens via USB-serial naar de Serial/MQTT-gateway. De gateway publiceert deze meetgegevens vervolgens naar de bijbehorende MQTT-topics.

Pompcommando's worden gepubliceerd naar `plant/pump/command`. De gateway abonneert zich op dit topic en stuurt het ontvangen commando via de serialverbinding door naar de Arduino.

De actuele status van de pomp wordt door de gateway gepubliceerd naar `plant/pump/status`.

```text
Arduino
   │
   │ USB-serial
   ▼
Serial/MQTT-gateway
   │
   ├── publish ──► plant/sensors/moisture
   ├── publish ──► plant/sensors/temperature
   ├── publish ──► plant/sensors/humidity
   │
   ◄── subscribe ─ plant/pump/command
   │
   └── publish ──► plant/pump/status
```

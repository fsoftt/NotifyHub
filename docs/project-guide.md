# NotifyHub — Guía Maestra de Construcción

## 1. Propósito del proyecto

NotifyHub es un proyecto de portafolio y aprendizaje para construir, desde cero, un **centro de notificaciones distribuido en .NET**, usando MongoDB/NoSQL y agregando progresivamente mensajería, proveedores externos, resiliencia, testing y observabilidad.

El objetivo principal **no es construir muchas tecnologías por construirlas**, sino aprender por qué cada pieza existe, qué problema resuelve y dónde debe vivir.

El proyecto debe comenzar sencillo y evolucionar de forma incremental.

---

# 2. Reglas obligatorias para cualquier asistente/Copilot

Estas reglas tienen prioridad durante toda la construcción.

## 2.1 Construcción incremental

No generar todo el proyecto de una vez.

Cada lección/sprint debe:

1. Explicar el problema.
2. Explicar la decisión arquitectónica.
3. Mostrar la estructura afectada.
4. Indicar exactamente qué archivos crear/modificar.
5. Dar código únicamente de lo que se implementará ahora.
6. Probar/verificar el resultado.
7. Mostrar qué queda pendiente.

No implementar funcionalidades futuras anticipadamente.

---

## 2.2 Diferenciar código implementable de ejemplos

Todo código debe estar claramente marcado:

### 🛠️ IMPLEMENTAR

Código que debe agregarse al proyecto actual.

### 💡 EJEMPLO

Código exclusivamente ilustrativo.

No agregar ejemplos al proyecto salvo que posteriormente se indique que deben implementarse.

---

## 2.3 Evitar spaghetti y sobrearquitectura

No crear clases, interfaces, servicios, repositorios o abstracciones solamente porque podrían ser útiles en el futuro.

Una abstracción debe tener una razón actual.

Evitar:

- Generic Repository.
- Generic Service.
- Managers genéricos.
- Wrappers innecesarios.
- Interfaces para cada clase sin necesidad.
- Clases creadas únicamente para demostrar patrones.
- Microservicios prematuros.
- CQRS/MediatR donde no aporten valor real.
- Complejidad distribuida antes de dominar el flujo básico.

Preferir código simple, explícito y fácil de seguir.

---

## 2.4 Vertical Slice

La arquitectura debe favorecer **Vertical Slice Architecture**.

No convertir el proyecto en una arquitectura horizontal donde toda la aplicación esté dividida únicamente por:

```text
Controllers/
Services/
Repositories/
Models/
DTOs/
```

Las funcionalidades deben agruparse por feature cuando corresponda.

Ejemplo:

```text
Features/
└── Notifications/
    └── Create/
        ├── Command.cs
        ├── Handler.cs
        └── Endpoint.cs
```

La infraestructura técnica puede mantenerse separada:

```text
Infrastructure/
├── Mongo/
├── Messaging/
└── Notifications/
```

---

## 2.5 Buenas prácticas sobre shortcuts

Si existe una solución rápida pero arquitectónicamente mala, explicar por qué y preferir la solución correcta.

No sacrificar diseño por ahorrar unas líneas de código.

Pero tampoco introducir complejidad que el proyecto todavía no necesita.

---

## 2.6 Convenciones de C#

No usar `_` como prefijo de campos privados.

Preferir:

```csharp
private readonly MongoContext mongoContext;
```

No:

```csharp
private readonly MongoContext _mongoContext;
```

Usar nombres claros y descriptivos.

Esta convención se refuerza mediante `.editorconfig` (ver `/.editorconfig` en la raíz del repo), no solo mediante revisión manual — así el analizador de .NET marca cualquier campo con prefijo `_` como advertencia, incluyendo código generado por asistentes.

---

# 3. Stack tecnológico

## Backend

- .NET / ASP.NET Core Web API
- C#
- Minimal APIs
- MongoDB
- MongoDB.Driver

## Mensajería

- RabbitMQ
- RabbitMQ.Client

## Email

- Resend

## Push

- Firebase Cloud Messaging (FCM)

## Contenedores

- Docker
- Docker Compose

## Testing

- xUnit
- FluentAssertions
- Testcontainers cuando corresponda

## Observabilidad

- ILogger
- Health Checks
- OpenTelemetry
- Métricas y tracing posteriormente

---

# 4. Arquitectura inicial

Comenzar con **un solo proyecto API**.

No crear microservicios inicialmente.

```text
NotifyHub/
│
├── NotifyHub.sln
│
├── docker-compose.yml
│
├── src/
│   └── NotifyHub.Api/
│       ├── Features/
│       │   └── Notifications/
│       │
│       ├── Infrastructure/
│       │   ├── Mongo/
│       │   ├── Messaging/
│       │   └── Notifications/
│       │
│       ├── Program.cs
│       └── appsettings.json
│
└── tests/
    └── NotifyHub.Api.Tests/
```

La estructura puede evolucionar conforme aparezcan necesidades reales.

---

# 5. Dominio funcional

NotifyHub administra notificaciones.

Una notificación puede utilizar múltiples canales:

```text
Notification
├── Email
├── Push
└── InApp
```

Cada canal tiene su propio estado:

```text
Pending
Sending
Sent
Failed
```

Ejemplo:

```text
Notification
├── Email → Sent
├── Push  → Failed
└── InApp → Pending
```

Los canales deben poder evolucionar independientemente.

---

# 6. NotifyHub NO administra usuarios

NotifyHub no será inicialmente un sistema completo de usuarios.

Una notificación debe tener un `Email` explícito cuando necesite enviar correo.

No usar:

```text
UserId = email
```

`UserId`, si existe, es un identificador/referencia y no debe utilizarse como dirección de correo.

Para Push, inicialmente se puede utilizar un token de dispositivo explícito.

No crear todavía:

- User service.
- Authentication system.
- Device management.
- User repository.

Esas funcionalidades solamente se agregarán si el proyecto realmente las necesita.

---

# 7. Modelo inicial de Notification

Conceptualmente:

```json
{
  "_id": "notification-id",
  "schemaVersion": 1,
  "userId": null,
  "email": "recipient@example.com",
  "pushRecipient": "device-token",
  "type": "ScoreAssigned",
  "content": {
    "title": "Nueva partitura",
    "message": "Se te ha asignado una nueva partitura."
  },
  "channels": [
    {
      "type": "Email",
      "status": "Pending",
      "sentAt": null,
      "deliveredAt": null
    },
    {
      "type": "Push",
      "status": "Pending",
      "sentAt": null,
      "deliveredAt": null
    }
  ],
  "read": false,
  "readAt": null,
  "createdAt": "..."
}
```

`schemaVersion` se incluye desde el primer documento. Es un campo barato de agregar ahora y costoso de retrofitear una vez existan documentos reales en Mongo sin él; permite migraciones futuras sin ambigüedad sobre la forma del documento.

El modelo real debe construirse gradualmente.

---

# 8. Fases del proyecto

## Fase 1 — Fundamentos

Objetivo: tener una API .NET funcionando con MongoDB.

### Pasos

1. Crear solution.
2. Crear Web API.
3. Configurar Docker Compose.
4. Ejecutar MongoDB.
5. Configurar MongoDB mediante Options.
6. Crear MongoContext.
7. Crear NotificationDocument.
8. Crear primera colección.
9. Crear endpoint básico.
10. Guardar la primera Notification.
11. Leerla desde MongoDB.

Al finalizar:

```text
API
 ↓
MongoDB
```

Debe funcionar sin RabbitMQ.

---

# Fase 2 — Modelo de dominio

Agregar progresivamente:

- Notification.
- NotificationChannel.
- NotificationChannelStatus.
- Content.
- Reglas básicas.
- Validaciones.

No crear un modelo complejo de usuario.

---

# Fase 3 — Vertical Slices

Crear las funcionalidades reales como slices:

```text
Features/
└── Notifications/
    ├── Create/
    ├── GetById/
    ├── List/
    ├── MarkAsRead/
    └── MarkAllAsRead/
```

Cada slice debe contener solamente lo que necesita.

> Nota: al construir `Create`, revisar la idempotencia a nivel de API descrita en la sección 16 (un cliente HTTP reintentando `POST /notifications` puede generar Notifications duplicadas; no resuelto todavía).

---

# Fase 4 — MongoDB + .NET

Objetivo: aprender MongoDB utilizando `MongoDB.Driver` desde .NET.

IMPORTANTE:

Esta guía NO debe convertirse en un curso de MongoDB shell.

El aprendizaje de MongoDB puro/conceptual se realizará aparte.

Aquí interesa principalmente cómo implementar correctamente desde .NET.

Temas:

- CRUD.
- Projections.
- UpdateOne.
- UpdateMany.
- Atomic updates.
- Compound indexes.
- Aggregation.
- Cursor pagination.
- Cursor encoding/decoding.
- Bulk operations.
- TTL.
- Transactions como concepto.
- Optimización.

No introducir Generic Repository.

Usar `MongoDB.Driver` directamente donde sea apropiado.

---

# Fase 5 — Mensajería

Esta fase debe reconstruirse cuidadosamente desde cero.

NO comenzar directamente con retries, leases, DLQ, heartbeat, idempotencia y recovery.

Primero construir el camino feliz.

## Orden obligatorio

### 5.1 Arquitectura de mensajería

Definir responsabilidades.

### 5.2 RabbitMQ connection

Conexión y lifecycle.

### 5.3 RabbitMQ topology

- Exchange.
- Queue.
- Binding.
- Routing key.

### 5.4 Publisher

Publicar un evento sencillo.

> ⚠️ Nota (Outbox, adelanto): desde este punto existe una ventana de inconsistencia entre el guardado en Mongo y la publicación en RabbitMQ — ver sección 17 (Outbox). El problema existe desde el primer Publisher, no solo al final de la fase. Se documenta aquí a propósito; su resolución se difiere deliberadamente hasta 5.14, para primero dominar el camino feliz antes de resolver la atomicidad.

### 5.5 Consumer

Consumir y hacer ACK.

### 5.6 Contract

Crear:

```text
NotificationCreatedMessage
```

Inicialmente debe ser pequeño.

Por ejemplo:

```json
{
  "messageId": "...",
  "notificationId": "..."
}
```

No duplicar innecesariamente todo el NotificationDocument.

### 5.7 Notification processing

Separar:

```text
RabbitMQ Consumer
       ↓
Notification processing
```

El consumer no debe contener lógica de negocio de canales.

### 5.8 Email

Separar:

```text
Email channel
     ↓
IEmailSender
     ↓
ResendEmailSender
```

El procesador de Email no debe conocer detalles internos de Resend.

### 5.9 Push

Separar:

```text
Push channel
     ↓
IPushSender
     ↓
FirebasePushSender
```

No acoplar NotificationConsumer directamente a Firebase.

### 5.10 Error classification

Distinguir:

```text
Transient error
Permanent error
```

Ejemplos:

```text
Timeout
Network failure
Temporary provider failure
        ↓
Retry
```

versus:

```text
Invalid recipient
Invalid push token
Malformed request
        ↓
Permanent failure
```

La clasificación específica del proveedor debe ocurrir en su adapter.

El sistema de mensajería debe recibir una señal genérica de fallo permanente/transitorio.

### 5.11 Retry

Implementar:

```text
retry.1
retry.2
retry.3
DLQ
```

No usar `requeue: true` como mecanismo principal de retries controlados.

El flujo debe ser:

```text
Publish to retry queue
        ↓
ACK original message
```

No:

```text
Nack/requeue
+
Publish retry
```

### 5.12 Idempotency

Separar dos problemas:

#### Message idempotency

Evitar procesar dos veces el mismo mensaje.

#### External side-effect idempotency

Evitar duplicar el efecto externo.

No asumir que porque el consumer es idempotente el proveedor externo también lo es.

Para Resend se puede utilizar su mecanismo de idempotency key.

Para proveedores que no tengan el mismo mecanismo, estudiar una estrategia propia.

### 5.13 Connection recovery

Implementar recuperación de conexión/canal cuando corresponda.

Diferenciar:

- retry técnico del publish;
- retry de negocio del mensaje.

No multiplicar retries innecesariamente.

### 5.14 Outbox

Finalmente implementar Outbox para resolver:

```text
Mongo save
+
RabbitMQ publish
```

y el problema:

```text
Mongo save ✅
RabbitMQ publish ❌
```

El Outbox debe ser introducido cuando el flujo básico ya sea comprendido.

---

# 9. Responsabilidades de las piezas de mensajería

Una clase debe poder explicarse en una frase.

## RabbitMqPublisher

Responsabilidad:

> Publicar mensajes en RabbitMQ.

No debe:

- procesar notificaciones;
- enviar emails;
- decidir reglas de negocio;
- manejar estados de Notification.

---

## RabbitMqConsumerWorker

Responsabilidad:

> Mantener un consumer de RabbitMQ ejecutándose como BackgroundService.

No debe:

- enviar emails directamente;
- conocer Resend;
- conocer Firebase;
- contener reglas de negocio de Notification.

---

## NotificationConsumer

Responsabilidad:

> Recibir el evento y coordinar el procesamiento de la Notification.

No debe contener detalles específicos de proveedores.

---

## Channel processors

Ejemplo:

```text
EmailNotificationProcessor
PushNotificationProcessor
```

Responsabilidad:

> Procesar un canal específico.

---

## Provider adapters

Ejemplos:

```text
ResendEmailSender
FirebasePushSender
```

Responsabilidad:

> Traducir nuestra abstracción interna al API del proveedor externo.

---

# 10. Email

La abstracción:

```csharp
public interface IEmailSender
{
    Task SendAsync(
        string idempotencyKey,
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken);
}
```

Proveedor:

```text
IEmailSender
      ↓
ResendEmailSender
      ↓
Resend API
```

Configuración sensible:

```text
Email:ApiKey
```

debe estar en User Secrets durante desarrollo y en un secret manager apropiado en producción.

No guardar API keys en Git.

Para pruebas se puede utilizar:

```text
onboarding@resend.dev
```

hasta que sea necesario configurar un dominio propio.

---

# 11. Push

La abstracción:

```csharp
public interface IPushSender
{
    Task SendAsync(
        string idempotencyKey,
        string recipient,
        string title,
        string body,
        CancellationToken cancellationToken);
}
```

Proveedor:

```text
IPushSender
      ↓
FirebasePushSender
      ↓
FCM
      ↓
Device
```

No crear todavía un sistema completo de usuarios/dispositivos.

Inicialmente:

```text
PushRecipient = FCM registration token
```

es suficiente para aprender el flujo.

---

# 12. Estados de canales

Inicialmente:

```text
Pending
Sending
Sent
Failed
```

Flujo normal:

```text
Pending
   ↓
Sending
   ↓
Sent
```

Error:

```text
Pending
   ↓
Sending
   ↓
Failed
```

Los estados deben actualizarse en Mongo de forma atómica cuando sea necesario.

---

# 13. Retry por canal

Una Notification puede tener:

```text
Email → Sent
Push  → Failed
```

Cuando se reintente el mensaje:

```text
Email → skip
Push  → retry
```

Nunca volver a enviar un canal que ya está correctamente `Sent`.

La unidad de retry de RabbitMQ puede ser el mensaje de Notification, mientras que el estado de cada canal determina qué trabajo sigue pendiente.

---

# 14. Publisher confirms

Los publisher confirms protegen principalmente la publicación:

```text
Publisher
    ↓
RabbitMQ
    ↓
Confirm
```

No confundir con consumer ACK:

```text
RabbitMQ
    ↓
Consumer
    ↓
ACK
```

Son mecanismos diferentes.

Publisher confirm:

> El broker confirmó la publicación.

Consumer ACK:

> El consumer confirmó el procesamiento del delivery.

---

# 15. Connection lifecycle

No crear una conexión RabbitMQ nueva para cada mensaje.

Preferir conexiones/canales de larga duración apropiados al lifecycle de la aplicación.

El publisher debe poder detectar una conexión inválida y reconstruirla.

No convertir esto en un framework genérico de connection pooling salvo que el problema real lo justifique.

---

# 16. Idempotency

Crear idempotencia solamente cuando exista un flujo que realmente pueda duplicarse.

Para mensajes:

```text
MessageId
   ↓
ProcessedMessage
```

Para efectos externos:

```text
NotificationId + Channel
```

puede servir como clave lógica.

Pero una clave local no garantiza que un proveedor externo haya sido idempotente.

Caso crítico:

```text
Provider
   ↓
Email enviado ✅
   ↓
NotifyHub crash 💥
   ↓
Mongo no actualizó Sent
```

Al retry:

```text
Provider
   ↓
¿Enviar nuevamente?
```

Este problema debe explicarse antes de intentar resolverlo.

### Idempotencia a nivel de API (pendiente)

Lo anterior cubre idempotencia de mensajes y de efectos externos hacia proveedores. Un problema relacionado pero distinto es un **cliente HTTP reintentando `POST /notifications`** (por ejemplo, tras un timeout de red), lo cual podría crear Notifications duplicadas antes de que exista cualquier mensaje en RabbitMQ.

Este caso no se resuelve todavía. Se revisitará al construir el slice `Create` (Fase 3), donde se decidirá si se requiere una idempotency key provista por el cliente o alguna otra estrategia.

---

# 17. Outbox

El Outbox es una de las piezas importantes de NotifyHub.

Problema:

```text
Mongo insert
     ↓
     ✅
     ↓
RabbitMQ publish
     ↓
     ❌ crash
```

La Notification existe pero RabbitMQ nunca recibió el evento.

Solución conceptual:

```text
MongoDB
├── Notification
└── OutboxMessage
          │
          ▼
    OutboxProcessor
          │
          ▼
      RabbitMQ
```

El Outbox se procesa de forma asíncrona.

No implementarlo antes de entender el flujo básico.

---

# 18. Redis

Agregar Redis solamente después de dominar:

- MongoDB.
- RabbitMQ.
- retries.
- idempotency.
- outbox.

Posibles usos:

- caching;
- rate limiting;
- deduplication;
- preferencias;
- throttling.

No introducir Redis simplemente porque es una tecnología popular.

---

# 19. Testing

El proyecto debe terminar teniendo diferentes niveles de pruebas.

## Unit tests

Para:

- reglas;
- retry policy;
- clasificación de errores;
- encoding/decoding de cursors;
- componentes deterministas.

## Integration tests

Para:

- MongoDB;
- RabbitMQ;
- procesamiento real de mensajes;
- Outbox.

Utilizar Testcontainers cuando tenga sentido.

## API tests

Probar endpoints completos.

---

# 20. Observabilidad

Después de que el sistema funcione:

## Logging

Usar `ILogger`.

Logs estructurados.

No loggear:

- API keys;
- passwords;
- tokens sensibles;
- contenido sensible innecesario.

## Health checks

Agregar checks para:

```text
MongoDB
RabbitMQ
```

y posteriormente proveedores cuando corresponda.

## OpenTelemetry

Agregar:

- traces;
- metrics;
- correlation/trace context.

Especialmente interesante para:

```text
HTTP
 ↓
Mongo
 ↓
RabbitMQ
 ↓
Consumer
 ↓
Provider
```

---

# 21. Docker Compose inicial

El entorno local debe comenzar simple.

```yaml
services:
  mongodb:
    image: mongo:8
    container_name: notifyhub-mongodb
    restart: unless-stopped
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db

  rabbitmq:
    image: rabbitmq:4-management
    container_name: notifyhub-rabbitmq
    restart: unless-stopped
    ports:
      - "5672:5672"
      - "15672:15672"

volumes:
  mongodb_data:
```

No agregar Redis, Kafka, Kubernetes, etc. desde el comienzo.

---

# 22. Configuración

Ejemplo:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "NotifyHub"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672
  }
}
```

Los secretos deben utilizar User Secrets:

```text
RabbitMq:UserName
RabbitMq:Password
Email:ApiKey
Firebase:ProjectId
Firebase:ClientEmail
Firebase:PrivateKey
```

No almacenar secretos reales en `appsettings.json`.

---

# 23. Roadmap completo

```text
FASE 1 — FUNDAMENTOS
⬜ Crear solución
⬜ ASP.NET Core API
⬜ Docker Compose
⬜ MongoDB
⬜ Configuration
⬜ MongoContext
⬜ First document
⬜ CRUD básico

FASE 2 — DOMINIO
⬜ Notification
⬜ Content
⬜ Channels
⬜ Status
⬜ Reglas
⬜ Validación

FASE 3 — VERTICAL SLICES
⬜ Create
⬜ GetById
⬜ List
⬜ MarkAsRead
⬜ MarkAllAsRead

FASE 4 — MONGODB + .NET
⬜ Projections
⬜ UpdateOne
⬜ UpdateMany
⬜ Atomic updates
⬜ Indexes
⬜ Aggregation
⬜ Cursor pagination
⬜ Bulk operations
⬜ TTL
⬜ Transactions
⬜ Optimization

FASE 5 — MESSAGING
⬜ RabbitMQ connection
⬜ Topology
⬜ Publisher
⬜ Consumer
⬜ Contract
⬜ Notification processing
⬜ Email
⬜ Resend
⬜ Push
⬜ Firebase
⬜ Error classification
⬜ Retry
⬜ DLQ
⬜ Idempotency
⬜ Connection recovery
⬜ Outbox

FASE 6 — REDIS
⬜ Cache
⬜ Rate limiting
⬜ Deduplication
⬜ Other justified use cases

FASE 7 — RELIABILITY
⬜ Failure scenarios
⬜ Retry strategy
⬜ Idempotency
⬜ Outbox
⬜ DLQ handling
⬜ Concurrency
⬜ Recovery

FASE 8 — TESTING
⬜ Unit tests
⬜ Integration tests
⬜ Testcontainers
⬜ API tests
⬜ Consumer tests

FASE 9 — OBSERVABILITY
⬜ Structured logging
⬜ Health checks
⬜ OpenTelemetry
⬜ Tracing
⬜ Metrics

FASE 10 — PORTFOLIO
⬜ Docker Compose
⬜ README
⬜ Architecture diagram
⬜ ADRs
⬜ API documentation
⬜ Test documentation
⬜ CI/CD
```

---

# 24. Cómo debe enseñar el asistente

Cuando el usuario diga:

> Siguiente

continuar exactamente desde el siguiente punto pendiente del roadmap.

No repetir todo el curso.

Cada lección debe tener esta estructura:

```text
# Lección X — Nombre

## Objetivo

Qué aprenderemos.

## Problema

Qué problema estamos resolviendo.

## Decisión

Por qué elegimos esta solución.

## Arquitectura

Diagrama sencillo.

## Implementación

### 🛠️ IMPLEMENTAR

Código real.

## Ejemplo

### 💡 EJEMPLO

Solo si ayuda a comprender.

## Verificación

Cómo comprobar que funciona.

## Errores comunes

Solo los relevantes.

## Estado

Qué está completado.

## Qué queda

Lista corta de próximos pasos.
```

---

# 25. Regla para preguntas durante una lección

Si el usuario pregunta:

> ¿Por qué hacemos esto?

Responder la duda antes de continuar.

No avanzar automáticamente.

Si el usuario pregunta:

> ¿Dónde llamamos este método?

Mostrar exactamente el punto de llamada y explicar el flujo.

Si el usuario detecta una inconsistencia arquitectónica:

1. Reconocerla.
2. Explicar el problema.
3. Corregir el diseño.
4. Actualizar el código necesario.
5. Continuar desde el nuevo diseño.

No defender una solución simplemente porque fue propuesta anteriormente.

---

# 26. Objetivo final

El resultado no debe ser solamente:

> "Una API que envía notificaciones."

Debe demostrar que el desarrollador entiende:

```text
.NET
MongoDB
NoSQL modeling
Vertical Slice
RabbitMQ
Async processing
Retries
DLQ
Idempotency
Publisher confirms
Consumer ACK
Connection recovery
Outbox
External providers
Resilience
Testing
Observability
```

Pero cada concepto debe introducirse **cuando el proyecto tiene una necesidad real de utilizarlo**.

La prioridad es:

```text
Entendible
    ↓
Correcto
    ↓
Testeable
    ↓
Resiliente
    ↓
Escalable
```

No:

```text
Complejo
    ↓
Difícil de entender
    ↓
Intentar justificar la complejidad
```

---

# 27. Regla final

Construir NotifyHub **desde cero**, sin asumir código existente.

Cuando se utilice esta guía en un nuevo chat o con Copilot:

1. Leer este archivo completo como contexto.
2. Identificar el primer punto pendiente.
3. No asumir que existen archivos que todavía no se han creado.
4. Dar rutas exactas para cada archivo.
5. Marcar cada código como 🛠️ IMPLEMENTAR o 💡 EJEMPLO.
6. Implementar únicamente la lección actual.
7. Verificar antes de continuar.
8. Mantener Vertical Slice.
9. Evitar sobreingeniería.
10. Mantener una lista corta de lo que queda.

El proyecto debe construirse como un proceso de aprendizaje guiado, no como un dump de código.

# API de Telemetría de TrackHub

## Características Principales

- **Proyección de Última Posición**: Almacena la posición más reciente por transportador que alimenta el mapa en vivo
- **Historial de Posiciones**: Almacén de trayectos de solo anexión, con anexión por lotes idempotente y persistencia de direcciones geocodificadas
- **Ejecuciones de Sincronización de Operadores**: Un registro por cada intento de sincronización de dispositivos/posiciones (conteos, resultado, error)
- **Chequeos de Salud de Operadores**: Sondeos de conectividad/salud; el resumen de salud y sincronización del operador se deriva de estas tablas en tiempo de lectura
- **Purga de Retención**: Trabajo en segundo plano programado dentro del host que elimina el historial vencido por cuenta, respetando los días de retención de cada cuenta
- **Visibilidad por Grupos**: Los roles Administrador/Gerente leen a nivel de toda la cuenta; los demás usuarios se limitan a los grupos a los que pertenecen
- **Esquema por Propietario**: Es dueño del esquema `telemetry` con acceso de solo lectura, entre esquemas, a las tablas de alcance del esquema `app`
- **Interfaz GraphQL**: Consultas eficientes y flexibles con el servidor GraphQL Hot Chocolate
- **Arquitectura Limpia**: Arquitectura por capas que garantiza mantenibilidad y capacidad de prueba

---

## Inicio Rápido

### Requisitos Previos

- SDK de .NET 10.0
- PostgreSQL 14+
- Servidor de Autoridad de TrackHub en ejecución (para autenticación)

### Instalación

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/shernandezp/TrackHub.Telemetry.git
   cd TrackHub.Telemetry
   ```

2. **Configurar la conexión a la base de datos** en `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHub;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Iniciar la aplicación**:
   ```bash
   dotnet run --project src/Web
   ```

4. **Acceder al Playground de GraphQL** en `https://localhost:5001/graphql`

> Las tablas de telemetría residen en el esquema `telemetry` y son creadas/migradas por el servicio Manager; la API de Telemetría las mapea y las sirve. En producción, conéctese con un rol que tenga lectura/escritura en `telemetry` y solo lectura en las tablas de alcance del esquema `app`.

---

## Componentes y Recursos

| Componente               | Descripción                                           | Documentación                                                                 |
|--------------------------|-------------------------------------------------------|-------------------------------------------------------------------------------|
| Hot Chocolate            | Servidor GraphQL para .NET                            | [Documentación de Hot Chocolate](https://chillicream.com/docs/hotchocolate/v13)  |
| .NET Core                | Plataforma de desarrollo para aplicaciones modernas   | [Documentación de .NET Core](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |
| Postgres                 | Sistema de gestión de bases de datos relacionales     | [Documentación de Postgres](https://www.postgresql.org/)                      |

---

## Descripción General

La **API de Telemetría de TrackHub** es dueña de los datos de posición y telemetría de operadores de alto volumen y de escritura intensiva de TrackHub. Almacena y sirve estos datos; nunca se comunica con los proveedores de GPS (esa es la función del Router). Sigue los principios de **Arquitectura Limpia** del proyecto, usando **GraphQL** para las interacciones de la API y **Postgres** para el almacenamiento.

---

## Entidades

### Telemetría (esquema `telemetry`, propias)

- **TransporterPosition**: La proyección de última posición — la posición más reciente por transportador.
- **TransporterPositionHistory**: El almacén de trayectos de solo anexión, deduplicado mediante una clave de idempotencia.
- **OperatorSyncRun**: Una fila por cada intento de sincronización de dispositivos/posiciones, con conteos de dispositivos y posiciones, resultado y error.
- **OperatorHealthCheck**: Resultados de los sondeos de conectividad/salud del operador.

### Alcance (esquema `app`, solo lectura)

Proyecciones mínimas y de solo lectura de las tablas de datos maestros que el servicio necesita para el alcance por cuenta y la visibilidad por grupos: usuarios, grupos, enlaces usuario–grupo y transportador–grupo, transportadores, asignaciones dispositivo–transportador, dispositivos, operadores y características de cuenta.

---

## Operaciones GraphQL

### Consultas

- **transporterPositionByOperator**: Últimas posiciones de un operador, según la visibilidad del solicitante.
- **positionHistory**: Historial de posiciones almacenado, filtrado por cuenta/transportador/dispositivo.
- **positionHistoryRange**: Lectura de reproducción sobre un rango de tiempo (ordenada, con tope de puntos), condicionada por `gps.positionHistory`.
- **operatorSyncRuns**: Telemetría de ejecuciones de sincronización registrada.
- **operatorHealth**: Instantánea de salud actual del operador, derivada de las tablas de chequeos de salud y de ejecuciones de sincronización.
- **operatorHealthHistory**: Registros recientes de chequeos de salud de un operador.
- **operatorHealthSummary**: Conteos agregados de disponibilidad/latencia/fallos en una ventana de tiempo.

### Mutaciones

- **bulkTransporterPosition**: Actualiza la proyección de última posición (la posición más reciente por transportador).
- **appendPositionHistory** / **appendPositionHistoryBatch**: Anexa filas de historial (idempotente; condicionado por `gps.positionHistory`).
- **persistResolvedAddress**: Escribe la dirección geocodificada en las filas de posición almacenadas.
- **recordOperatorSyncRun**: Registra un intento de sincronización.
- **recordOperatorHealth**: Registra un chequeo de salud del operador.
- **purgeExpiredPositionHistory**: Elimina el historial anterior a una fecha de corte para una cuenta.

### ¿Por qué GraphQL?

El uso de **GraphQL** permite consultas eficientes y personalizables, dejando que los clientes soliciten solo los datos que necesitan para minimizar el ancho de banda y mejorar el rendimiento.

## Licencia

Este proyecto está licenciado bajo la Licencia Apache 2.0. Consulte el [archivo LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.

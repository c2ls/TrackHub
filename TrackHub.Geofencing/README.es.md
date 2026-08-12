# API de Geocercado de TrackHub

## Características Principales

- **Gestión de Datos Geoespaciales**: Crear y gestionar límites geográficos utilizando tipos de datos espaciales PostGIS
- **Interfaz GraphQL**: Consultas eficientes y flexibles con servidor GraphQL Hot Chocolate
- **Análisis de Posición en Tiempo Real**: Consultar posiciones de transportadores relativas a geocercas definidas
- **Detección de Eventos de Geocerca**: Detección automática de eventos de entrada/salida usando consultas espaciales NetTopologySuite
- **Procesamiento de Posiciones**: Ingesta masiva de posiciones con contención de geocercas en tiempo real usando PostGIS ST_Contains
- **Arquitectura Limpia**: Arquitectura en capas que asegura mantenibilidad y capacidad de prueba
- **Acceso por Usuario**: Acceso a datos basado en vistas con filtrado de permisos de usuario
- **PostgreSQL + PostGIS**: Capacidades de base de datos espacial de nivel empresarial

---

## Inicio Rápido

### Requisitos Previos

- .NET 10.0 SDK
- PostgreSQL 14+ con extensión PostGIS habilitada
- TrackHub Authority Server ejecutándose (para autenticación)

### Instalación

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/shernandezp/TrackHub.Geofencing.git
   cd TrackHub.Geofencing
   ```

2. **Habilitar extensión PostGIS** en PostgreSQL:
   ```sql
   CREATE EXTENSION postgis;
   ```

3. **Configurar la conexión a la base de datos** en `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "ManagerConnection": "Host=localhost;Database=trackhub_manager;Username=postgres;Password=yourpassword"
     }
   }
   ```

4. **Ejecutar las migraciones de la base de datos**:
   ```bash
   dotnet ef database update
   ```

5. **Iniciar la aplicación**:
   ```bash
   dotnet run --project src/Web
   ```

6. **Acceder al Playground GraphQL** en `https://localhost:5001/graphql`

---

## Componentes y Recursos Utilizados

| Componente                | Descripción                                             | Documentación                                                                 |
|---------------------------|---------------------------------------------------------|-------------------------------------------------------------------------------|
| Hot Chocolate             | Servidor GraphQL para .Net        | [Documentación Hot Chocolate](https://chillicream.com/docs/hotchocolate/v13)                           |
| .NET Core                 | Development platform for modern applications          | [.NET Core Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |
| Postgres                  | Sistema de gestión de bases de datos relacional         | [Documentación Postgres](https://www.postgresql.org/)                         |

---

## Descripción General

La **API de Geofencing de TrackHub** proporciona servicios para gestionar las características de geocercado de TrackHub. Sigue los principios de la **Arquitectura Limpia** del proyecto, aprovechando **GraphQL** para las interacciones de la API y **Postgres** para la gestión de la base de datos.

### Características Principales

La API ofrece las siguientes funcionalidades:
- Gestión de límites geográficos, conocidos como geocercas.
- Generación de informes basados en datos de geocercas.

---

## Entidades

### Geofencing

- **Geofence**: Representa las geocercas utilizando la extensión PostGIS de Postgres para manejar datos espaciales.
- **GeofenceEvent**: Rastrea eventos de entrada/salida de transportadores con marcas de tiempo y ubicación. Los eventos se crean automáticamente cuando se procesan posiciones y un transportador entra o sale de una geocerca.
- **VwUser**: Una vista de la tabla de usuarios dentro del esquema de geofencing, que ofrece una interfaz simplificada para acceder a datos relacionados con usuarios.
- **VwTransporterPosition**: Una vista que combina datos de transportadores con sus respectivas posiciones en formato geométrico para facilitar consultas geoespaciales.

---

## Operaciones GraphQL

### Mutaciones

- **processPositions**: Procesa posiciones de transportadores para detectar eventos de entrada/salida de geocercas

### Consultas

- **geofenceEventsAfter**: Recupera eventos después de un cursor dado (ID de evento) para polling por servicios posteriores. Soporta paginación basada en cursor donde cada consumidor rastrea su propia posición.
- **transportersInGeofence**: Obtiene todos los transportadores actualmente dentro de cualquier geocerca
- **geofenceEvents**: Recupera eventos de geocercas para reportes, filtrados por cuenta, usuario, rango de fechas y transportador opcional. Retorna nombre del transportador, nombre de la geocerca, marcas de tiempo de entrada/salida, tiempo total y coordenadas.

### ¿Por qué GraphQL?

El uso de **GraphQL** permite consultas eficientes y personalizables, permitiendo a los clientes solicitar solo los datos que necesitan para minimizar el ancho de banda y mejorar el rendimiento de la aplicación. Con GraphQL, las aplicaciones pueden recuperar detalles específicos sobre usuarios, transportadores o dispositivos, optimizando tanto la eficiencia operativa como la experiencia del usuario.

## Licencia

Este proyecto está bajo la Licencia Apache 2.0. Consulta el archivo [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.


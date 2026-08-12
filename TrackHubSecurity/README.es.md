# API de Seguridad para TrackHub

## Características Principales

- **Control de Acceso Basado en Roles (RBAC)**: Gestión integral de permisos con roles, políticas y acciones
- **Interfaz GraphQL**: Consultas eficientes y flexibles para datos de seguridad usando servidor Hot Chocolate
- **Permisos Granulares**: Control de acceso a nivel de recurso y acción para máxima seguridad
- **Gestión de Usuarios**: Operaciones CRUD completas para cuentas de usuario con asignación de roles
- **Validación de Contraseñas**: Requisitos de fortaleza de contraseña incluyendo longitud mínima, mayúsculas, minúsculas, dígitos y caracteres especiales
- **Motor de Políticas**: Definir y aplicar políticas de acceso personalizadas a nivel de usuario y rol
- **Gestión de Clientes de Servicio**: Registro seguro y gestión de permisos para clientes de servicios externos
- **Plantillas de Roles por Defecto**: Roles preconfigurados de Administrador, Gerente y Usuario para configuración rápida
- **Arquitectura Limpia**: Código mantenible y testeable siguiendo principios SOLID

---


## Inicio Rápido

### Requisitos Previos

- .NET 10.0 SDK
- PostgreSQL 14+
- TrackHub Authority Server ejecutándose (para autenticación)

### Instalación

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/shernandezp/TrackHubSecurity.git
   cd TrackHubSecurity
   ```

2. **Configurar la conexión a la base de datos** en `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "SecurityConnection": "Host=localhost;Database=trackhub_security;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Ejecutar las migraciones de la base de datos**:
   ```bash
   dotnet ef database update
   ```

4. **Sembrar datos iniciales** (roles, recursos, acciones):
   ```bash
   dotnet run --project src/DBInitializer
   ```

5. **Iniciar la aplicación**:
   ```bash
   dotnet run --project src/Web
   ```

6. **Acceder al Playground GraphQL** en `https://localhost:5001/graphql`

### Roles por Defecto

| Rol | Descripción |
|-----|-------------|
| **Administrador** | Acceso completo a todos los recursos y acciones |
| **Gerente** | Acceso intermedio para gestión de recursos |
| **Usuario** | Acceso limitado basado en responsabilidades asignadas |

---

## Componentes y Recursos Utilizados

| Componente                | Descripción                                             | Documentación                                                                 |
|---------------------------|---------------------------------------------------------|-------------------------------------------------------------------------------|
| Hot Chocolate             | Servidor GraphQL para .Net        | [Documentación Hot Chocolate](https://chillicream.com/docs/hotchocolate/v13)                           |
| .NET Core                 | Plataforma de desarrollo para aplicaciones modernas     | [Documentación .NET Core](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |
| Postgres                  | Sistema de gestión de bases de datos relacional         | [Documentación Postgres](https://www.postgresql.org/)                         |

---

## Descripción General

La API de Seguridad es un servicio GraphQL responsable de gestionar la base de datos de seguridad de TrackHub. Expone métodos esenciales para manejar el control de acceso basado en roles (RBAC), permitiendo la gestión de roles de usuarios, permisos y recursos.

## Entidades Clave

Las entidades principales en este servicio son:

- **Action (Acción)**: Define las operaciones (por ejemplo, crear, leer, actualizar, eliminar) que se pueden realizar sobre un recurso. Las acciones se asignan a recursos y determinan qué operaciones están permitidas.

- **Client (Cliente)**: Almacena información sobre los clientes tipo servicio que interactúan con la API. Gestiona los permisos y accesos para clientes externos, asegurando interacciones seguras.

- **Policy (Política)**: Representa un conjunto de reglas que gobiernan el control de acceso. Las políticas son esenciales para hacer cumplir medidas de seguridad, ya sea a nivel de usuario o de rol, y controlan qué acciones pueden realizarse sobre qué recursos.

- **Resource (Recurso)**: Representa los diferentes componentes o módulos de la aplicación que requieren control de acceso. Los recursos pueden incluir APIs, puntos finales, elementos de la interfaz de usuario u otras entidades que necesiten gestión de permisos.

- **ResourceAction (Acción de Recurso)**: Asocia acciones específicas a recursos, definiendo qué operaciones están permitidas sobre cada recurso.

- **ResourceActionPolicy (Política de Acción de Recurso)**: Define políticas de acceso para combinaciones específicas de recursos y acciones, proporcionando control detallado sobre los permisos.

- **ResourceActionRole (Rol de Acción de Recurso)**: Asocia roles específicos con combinaciones de recursos y acciones, determinando qué acciones puede realizar cada rol sobre un recurso.

- **Role (Rol)**: Define los roles dentro de la aplicación (por ejemplo, Administrador, Gerente, Usuario) para RBAC. Los roles determinan el acceso a diversos recursos y acciones en el sistema.

- **User (Usuario)**: Contiene información específica del usuario, como credenciales y datos de perfil. Los usuarios son asignados a roles y políticas para definir sus derechos de acceso.

- **UserPolicy (Política de Usuario)**: Contiene políticas que se aplican a usuarios individuales, permitiendo un control más granular sobre el acceso de los usuarios más allá de los roles.

- **UserRole (Rol de Usuario)**: Asigna roles a los usuarios, determinando su nivel de acceso a recursos. Hay tres roles predeterminados:
  - **Administrador**: Acceso completo a todos los recursos y acciones.
  - **Gerente**: Acceso intermedio, con permisos para gestionar recursos específicos.
  - **Usuario**: Acceso limitado, con permisos para interactuar con recursos específicos según las responsabilidades del usuario.

## Enfoque de Arquitectura Limpia

Esta API sigue los principios de Arquitectura Limpia, los cuales enfatizan la separación de preocupaciones y garantizan que la lógica de negocio se mantenga desacoplada de frameworks, bases de datos y fuentes de datos externas. Esta arquitectura promueve la escalabilidad, mantenibilidad y flexibilidad, facilitando la evolución de la aplicación con el tiempo.

Se utiliza GraphQL para ofrecer un mecanismo de consultas flexible y eficiente, permitiendo a los clientes solicitar solo los datos que necesitan, lo que optimiza el rendimiento y reduce el sobrecargado de datos.

## Beneficios del Control de Acceso Basado en Roles (RBAC)

Al aprovechar RBAC, la API de Seguridad asegura que los usuarios y clientes solo tengan acceso a los recursos y acciones con los que están autorizados para interactuar. Esto minimiza el riesgo de acceso no autorizado y mantiene el principio de mínimo privilegio en todo el sistema.

## Licencia

Este proyecto está bajo la Licencia Apache 2.0. Consulta el archivo [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.


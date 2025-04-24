# Biblioteca de Sockets Genérica

## Descripción

Esta biblioteca implementa una solución genérica de sockets de red para .NET Core 8, diseñada como un componente reutilizable para futuros proyectos del curso. Proporciona una abstracción de alto nivel sobre los sockets de red, permitiendo una comunicación cliente-servidor eficiente y robusta sin tener que lidiar con los detalles de bajo nivel de la implementación de sockets.

## Objetivos

El objetivo principal es crear una biblioteca (.dll) que encapsule la funcionalidad de sockets de red y que pueda ser fácilmente integrada en cualquier proyecto .NET Core 8, siguiendo buenas prácticas de programación, patrones de diseño y principios SOLID.

## Características principales

- **Escucha de mensajes**: Implementación sencilla para recibir datos a través de sockets.
- **Envío de mensajes**: Métodos optimizados para transmitir datos de forma eficiente.
- **Manejo de reintentos**: Sistema automático de reintentos en caso de fallos de comunicación.
- **Gestión de timeouts**: Control configurable de tiempos de espera en las operaciones.
- **Manejo de excepciones**: Sistema robusto de gestión de errores específicos de la red.
- **Diseño desacoplado**: La biblioteca opera independientemente de la lógica de procesamiento de mensajes.

## Requisitos

- .NET Core 8 SDK
- Visual Studio 2022 (recomendado) o cualquier otro IDE compatible con .NET Core 8

## Instalación

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/Preko700/TE2.git
   ```

2. Agregar referencia a la biblioteca en tu proyecto:
   - Opción 1: Agregar directamente el archivo .dll como referencia en tu proyecto
   - Opción 2: Incluir el proyecto en tu solución y agregar una referencia al proyecto

## Uso básico

### Configuración del servidor

```csharp
using SocketLibrary;

// Crear y configurar un servidor
var serverConfig = new SocketServerConfig
{
    IpAddress = "127.0.0.1",
    Port = 8080,
    MaxConnections = 10,
    ConnectionTimeout = TimeSpan.FromSeconds(30)
};

var server = new SocketServer(serverConfig);

// Registrar manejador de mensajes
server.OnMessageReceived += (sender, messageData) =>
{
    Console.WriteLine($"Mensaje recibido: {messageData.Message}");
    return Task.CompletedTask;
};

// Iniciar el servidor
await server.StartAsync();
```

### Configuración del cliente

```csharp
using SocketLibrary;

// Crear y configurar un cliente
var clientConfig = new SocketClientConfig
{
    ServerIpAddress = "127.0.0.1",
    ServerPort = 8080,
    ConnectionTimeout = TimeSpan.FromSeconds(10),
    RetryAttempts = 3
};

var client = new SocketClient(clientConfig);

// Conectar al servidor
await client.ConnectAsync();

// Enviar un mensaje
await client.SendMessageAsync("Hola mundo");

// Cerrar la conexión cuando ya no se necesite
await client.DisconnectAsync();
```

## Estructura del proyecto

```
SocketLibrary/
├── Core/                     # Componentes principales
│   ├── SocketBase.cs         # Clase base abstracta para sockets
│   ├── SocketServer.cs       # Implementación del servidor
│   ├── SocketClient.cs       # Implementación del cliente
│   └── MessageEventArgs.cs   # Argumentos para eventos de mensajes
│
├── Configuration/            # Configuraciones
│   ├── SocketConfig.cs       # Configuración base
│   ├── ServerConfig.cs       # Configuración específica del servidor
│   └── ClientConfig.cs       # Configuración específica del cliente
│
├── Interfaces/               # Definiciones de interfaces
│   ├── ISocket.cs            # Interfaz base para sockets
│   ├── ISocketServer.cs      # Interfaz para servidores
│   └── ISocketClient.cs      # Interfaz para clientes
│
├── Handlers/                 # Manejadores de eventos
│   ├── MessageHandler.cs     # Manejo genérico de mensajes
│   └── ErrorHandler.cs       # Manejo de excepciones
│
└── Utilities/                # Utilidades
    ├── RetryPolicy.cs        # Políticas de reintento
    └── SocketExtensions.cs   # Métodos de extensión
```

## Principios de diseño aplicados

Esta biblioteca ha sido desarrollada siguiendo:

1. **Principios SOLID**:
   - Single Responsibility: Cada clase tiene una única responsabilidad.
   - Open/Closed: El código está abierto para extensión pero cerrado para modificación.
   - Liskov Substitution: Las clases derivadas pueden sustituir a sus clases base.
   - Interface Segregation: Interfaces específicas para clientes específicos.
   - Dependency Inversion: Dependencia de abstracciones, no de implementaciones concretas.

2. **Patrones de diseño**:
   - Patrón Observer: Para la notificación de eventos de mensajes.
   - Patrón Factory: Para la creación de instancias de socket.
   - Patrón Strategy: Para diferentes estrategias de reintentos y manejo de errores.

3. **Buenas prácticas**:
   - Manejo adecuado de recursos con using statements.
   - Operaciones asíncronas para no bloquear el hilo principal.
   - Cancelación de operaciones mediante CancellationTokens.
   - Logging extensivo para facilitar la depuración.

## Excepciones personalizadas

La biblioteca incluye varias excepciones personalizadas para manejar diferentes escenarios de error:

- `SocketConnectionException`: Errores al establecer conexiones.
- `SocketTimeoutException`: Errores por tiempo de espera agotado.
- `MessageSerializationException`: Errores en la serialización/deserialización de mensajes.

## Contribución

Para contribuir a este proyecto, por favor:

1. Crea un fork del repositorio
2. Crea una rama para tu funcionalidad (`git checkout -b feature/nueva-funcionalidad`)
3. Haz commit de tus cambios (`git commit -am 'Añadir nueva funcionalidad'`)
4. Haz push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Crea un nuevo Pull Request

## Autor

- [Jimena Castillo Campos](https://github.com/JimenaCastillo)
- [Adrián Monge Mairena](https://github.com/Preko700)

## Licencia

Este proyecto está licenciado bajo la Licencia Apache 2.0 - ver el archivo [LICENSE](LICENSE) para más detalles.

# Sales Analysis ETL

Proyecto .NET 8 para el sistema de análisis de ventas. Incluye extracción de datos desde archivos CSV y una API REST local, y carga directa a un Data Warehouse en SQL Server.

## Estructura

```text
Sales-Analysis-ETL-V2/
├── src/
│   ├── SalesAnalysis.Api/          # API REST local conectada a la base transaccional
│   └── SalesAnalysis.Etl.Worker/   # Worker Service que ejecuta extracción y carga al DW
│       └── data_sources/            # Archivos CSV de entrada
└── SalesAnalysisEtl.slnx            # Solución .NET
```

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server con las bases de datos:
  - `ventas_oltp` (base transaccional)
  - `olap_ventas` (base analítica / Data Warehouse)

## Configuración

### API local

La API se conecta a la base transaccional mediante la cadena de conexión en `src/SalesAnalysis.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "connection": "Data Source=OXXCIBER;Database=ventas_oltp;Integrated Security=True;..."
}
```

### Worker

El Worker se conecta al Data Warehouse. La cadena de conexión se configura en `src/SalesAnalysis.Etl.Worker/appsettings.json`.

## Ejecución

### 1. Compilar la solución

```bash
dotnet build SalesAnalysisEtl.slnx
```

### 2. Iniciar la API local

```bash
dotnet run --project src/SalesAnalysis.Api
```

La API expone los siguientes endpoints:

- `GET http://localhost:5000/api/customers`
- `GET http://localhost:5000/api/products`

### 3. Ejecutar el Worker ETL

En otra terminal:

```bash
dotnet run --project src/SalesAnalysis.Etl.Worker
```

El Worker realiza lo siguiente:

- Extrae datos desde los archivos CSV en `data_sources/`.
- Extrae clientes y productos desde la API REST local (fuente disponible para uso futuro).
- Carga las dimensiones `CustomerDim`, `ProductDim`, `DateDim` y la tabla de hechos `FactTable` en `olap_ventas`.
- Registra eventos y métricas en la consola.

## Notas

- El Worker utiliza carga **Full Load**: trunca las tablas del DW y las vuelve a cargar.
- La API REST local se mantiene como fuente externa aunque la carga actual use principalmente CSV.
- Los extractores y repositorios están diseñados mediante interfaces para facilitar mantenibilidad y escalabilidad.

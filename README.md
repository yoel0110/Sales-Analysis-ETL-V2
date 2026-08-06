# Sales Analysis ETL - Actividad 1

Proyecto .NET 8 para la Actividad 1: Creación de la Arquitectura y Desarrollo del Proceso de Extracción del sistema de análisis de ventas.

## Estructura

```text
Sales-Analysis-ETL-V2/
├── src/
│   ├── SalesAnalysis.Api/          # API REST local conectada a la base transaccional
│   └── SalesAnalysis.Etl.Worker/   # Worker Service que ejecuta el proceso de extracción
├── data_sources/                    # Archivos CSV de entrada
├── staging/                         # Archivos JSON generados por el Worker
└── SalesAnalysisEtl.slnx            # Solución .NET
```

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server con la base de datos `ventas_oltp` configurada

## Configuración

La API se conecta a la base transaccional mediante la cadena de conexión en `src/SalesAnalysis.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "connection": "Data Source=OXXCIBER;Database=ventas_oltp;Integrated Security=True;..."
}
```

El Worker consume la API local mediante la configuración en `src/SalesAnalysis.Etl.Worker/appsettings.json`:

```json
"ApiSources": {
  "CustomersUrl": "http://localhost:5000/api/customers",
  "ProductsUrl": "http://localhost:5000/api/products"
}
```

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

En otra terminal, con la API en ejecución:

```bash
dotnet run --project src/SalesAnalysis.Etl.Worker
```

El Worker realiza lo siguiente:

- Extrae datos desde los archivos CSV en `data_sources/`.
- Extrae clientes y productos desde la API REST local.
- Guarda los datos extraídos como archivos JSON en `staging/`.
- Registra eventos y métricas en la consola.

## Archivos Staging Generados

```text
staging/csv_customers.json
staging/csv_products.json
staging/csv_orders.json
staging/csv_order_details.json
staging/api_customers.json
staging/api_products.json
```

## Notas

- Esta actividad cubre únicamente la fase de **extracción** del proceso ETL.
- No se implementa carga a base analítica ni dashboard en esta entrega.
- La API REST local representa la fuente externa indicada en la definición del problema.
- Los extractores están diseñados mediante interfaces para facilitar mantenibilidad y escalabilidad.

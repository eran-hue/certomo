# Milezero Template

An event-driven microservices architecture template designed to process signals with high resilience, reliability, and fault tolerance. Built with .NET, MassTransit (RabbitMQ), and PostgreSQL.

## 🏗️ Architecture

The system consists of several decoupled microservices communicating asynchronously via RabbitMQ:

1.  **ApiGateway**: Accepts HTTP POST requests (`/api/signals`) and publishes `SignalReceived` events.
2.  **OrchestratorService**: Consumes `SignalReceived`, determines necessary work, and publishes `InitiateProcessing` commands.
3.  **DataProcessorService** (3 Instances): Consumes `InitiateProcessing`, performs work (with simulated delays/failures), and publishes `DataProcessed`.
4.  **AggregationService**: Stateful service that collects `DataProcessed` events. It waits for all parts to arrive (or timeouts) and publishes `AggregationCompleted`.
5.  **NotificationService**: Listens for completion events and logs "notifications".

## 🛡️ Key Features

-   **Resilience**:
    -   **Retry Policies**: Exponential backoff configured for all consumers.
    -   **Dead Letter Queues (DLQ)**: Failed messages are safely routed to `_error` queues.
    -   **Circuit Breakers**: Configured via MassTransit/Polly (implicit in retry config).
-   **Idempotency**:
    -   Database-level unique constraints ensure no result is double-counted.
    -   In-memory checks for duplicate message handling.
-   **Graceful Degradation**:
    -   **Timeouts**: If a processor fails to respond within 30 seconds, the Aggregator finalizes the result with partial data.

## 🚀 Getting Started

### Prerequisites
-   Docker Desktop
-   .NET 8/9 SDK (optional, for local dev outside Docker)

### Running the System
1.  Clone the repository.
2.  Run with Docker Compose:
    ```bash
    docker-compose up --build
    ```
3.  Wait for all services to start (RabbitMQ and Postgres need to be healthy first).

### Sending a Signal
You can send a signal using `curl` or any HTTP client:

```bash
curl -X POST http://localhost:8080/api/signals \
   -H "Content-Type: application/json" \
   -d '{"value": 10}'
```

Expected Flow:
1.  **API Gateway** accepts request -> returns 202 Accepted.
2.  **Orchestrator** receives signal -> dispatches to Processors.
3.  **Processors** (x3) process data -> publish results (approx 1s delay).
4.  **Aggregator** collects 3 results -> saves to DB -> publishes complete event.
5.  **Notification** logs the final result.

## 🧪 Testing

### Integration Tests
Run the integration tests (requires Docker for Testcontainers):
```bash
dotnet test
```

### Resilience Verification
Scripts are provided in `tests/resilience/` to simulate failures manually while the system is running:

-   **Kill a Processor**:
    ```bash
    ./tests/resilience/kill_processor.bat
    ```
    *Observation*: System should eventually recover or Aggregator should timeout and produce a partial result.

-   **Simulate DB Outage**:
    ```bash
    ./tests/resilience/simulate_db_outage.bat
    ```
    *Observation*: Services should retry DB operations until it comes back online.

## 📂 Project Structure

-   `src/Shared.Kernel`: Shared DTOs, Events, Entities, and Interfaces.
-   `src/ApiGateway`: ASP.NET Core Web API.
-   `src/OrchestratorService`: Workflow logic.
-   `src/DataProcessorService`: Worker service (multiple instances).
-   `src/AggregationService`: Stateful aggregation logic.
-   `src/NotificationService`: Final event listener.
-   `tests/`: Unit and Integration tests.

---
*Generated with the assistance of Roo AI.*

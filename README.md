# NotifyHub

NotifyHub is a learning project that demonstrates building a notification system using modern cloud-native building blocks. The solution is intended as a portfolio / learning exercise and focuses on reliability patterns and messaging. It helps manage notifications delivered via email, SMS and push.

Primary concepts demonstrated (some items are TODOs):
- NoSQL (e.g., MongoDB) for storage
- RabbitMQ for messaging and delivery orchestration (Messaging) (TODO: finalize flows)
- Background service for processing messages (TODO: implement if not present)
- Retries and retry policies for transient failures
- Idempotency to avoid duplicate deliveries
- Dead Letter Queue (DLQ) handling (TODO)
- Redis for caching / deduplication (TODO)
- Reliability best practices (TODO)
- Observability (metrics / tracing / logs) (TODO)

Status: educational / in-progress. Use this repository to learn, experiment and extend features.

Projects in this solution
- NotifyHub.Api — main API to create and manage notifications
- (Possible other projects) — background workers, libraries, tests

What it does
- Accepts requests to create notifications (email, sms, push)
- Publishes tasks to RabbitMQ for processing
- Uses a NoSQL store for persistence and Redis for fast lookup/deduplication (if configured)
- Applies retry and idempotency logic when delivering notifications

Run locally — prerequisites
- .NET 10 SDK
- Docker (or local instances of RabbitMQ, MongoDB, Redis)
- Git

Quick local setup (recommended: Docker)

1) Start dependent services

You can run the following containers with Docker. Create a `docker-compose.yml` or run the commands below.

Example docker run commands:

docker run -d --name notifyhub-rabbitmq -p 5672:5672 -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=guest -e RABBITMQ_DEFAULT_PASS=guest rabbitmq:3-management

docker run -d --name notifyhub-mongo -p 27017:27017 mongo:6

docker run -d --name notifyhub-redis -p 6379:6379 redis:7

After RabbitMQ starts you can open the management UI at: http://localhost:15672 (default guest/guest)

2) Configure the application

The API reads configuration from appsettings.json, environment variables and (during development) user secrets. Sensitive values such as RabbitMQ username and password should be stored in user secrets or environment variables.

Create user secrets for RabbitMQ credentials (recommended for local development)

1. Open a terminal and change to the API project folder:

   cd src/NotifyHub.Api

2. Initialize user-secrets for this project (if not already initialized):

   dotnet user-secrets init

3. Set the RabbitMQ username and password (replace values):

   dotnet user-secrets set "RabbitMq:Username" "your-rabbit-username"
   dotnet user-secrets set "RabbitMq:Password" "your-rabbit-password"

   # Set an Email API key used by the email delivery provider (replace value)
   dotnet user-secrets set "Email:ApiKey" "your-email-api-key"

Other settings you may want to set in user secrets or environment variables:
- MongoDb:ConnectionString — e.g. mongodb://localhost:27017/notifyhub
- Redis:ConnectionString — e.g. localhost:6379
- Notification:FromEmail — sender address for email deliveries
- Any API keys or SMTP credentials required for real delivery providers
- Email:ApiKey — API key for the configured email provider (set via user secrets or environment variable)

3) Run the API

From repo root or solution folder:

   dotnet restore
   dotnet build
   dotnet run --project src/NotifyHub.Api

If there is a background worker project included, run it similarly (dotnet run --project path/to/worker).

Configuration notes
- appsettings.json contains base configuration. For development you should override secrets with user-secrets or environment variables. .NET configuration order will pick user secrets for the API project in the Development environment.
- If running containers with Docker, either point connection strings at the container host (localhost) or use Docker network hostnames when using docker-compose.

Security and secrets
- Never commit real credentials to source control. Use `dotnet user-secrets` for local development and an appropriate secret store (Azure Key Vault, AWS Secrets Manager, etc.) for production.

Extending the project
- Implement the TODO items to expand the learning scope:
  - RabbitMQ message topology and durable queues
  - Implement background worker(s) to consume and deliver notifications
  - Add DLQ behavior and monitoring for poison messages
  - Add Redis-backed deduplication and caching
  - Add observability: metrics (Prometheus), tracing (OpenTelemetry) and structured logs

Notes for contributors
- This repository is a learning playground. Small, self-contained PRs with clear goals are preferred.

License
- If not already specified in the repo, add a license file (for personal/portfolio use choose an appropriate license).

Contact / portfolio
- This repository is part of a learning and portfolio effort. Use it as a reference to demonstrate messaging and reliability patterns.

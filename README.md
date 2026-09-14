# NotifyHub

A learning/portfolio project: a distributed notification hub built from scratch in .NET, using MongoDB, RabbitMQ, and progressively-introduced resilience, testing, and observability practices.

The goal isn't to use many technologies — it's to understand why each piece exists, what problem it solves, and where it belongs.

## Status

🚧 Not started yet. Repository scaffolding only.

## Guide

The full build guide — rules, architecture decisions, phased roadmap — lives at [`docs/project-guide.md`](docs/project-guide.md).

Real progress is tracked in [`docs/roadmap.md`](docs/roadmap.md).

## Workflow

`main` is protected. All changes land via a branch + pull request — see section 28 of the project guide.

## Stack (planned)

- .NET / ASP.NET Core Web API, MongoDB, RabbitMQ
- Resend (email), Firebase Cloud Messaging (push)
- Docker Compose for local infra
- xUnit, FluentAssertions, Testcontainers
- OpenTelemetry, Health Checks

## License

[MIT](LICENSE)

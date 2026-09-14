# NotifyHub Roadmap

Tracks real progress against the phases defined in [`docs/project-guide.md`](project-guide.md) (section 23). Updated as work lands; shown after every assistant response while this project is active.

## Phase 0 — Repository & workflow

- [x] Public GitHub repo created (SSH remote)
- [x] MIT license
- [x] Project guide committed (`docs/project-guide.md`, English)
- [x] `.editorconfig` enforcing no-underscore private fields
- [x] Branch protection on `main` (PRs required)
- [x] `.gitignore` (.NET build output)
- [x] CI (`dotnet build` + `dotnet test` on push/PR)

## Phase 1 — Foundations

- [x] Create solution
- [x] ASP.NET Core API
- [x] Docker Compose
- [x] MongoDB
- [x] Configuration
- [x] MongoContext
- [x] First document
- [x] Basic CRUD

## Phase 2 — Domain

- [x] Notification (UserId, Content, Channels, UpdatedAt added)
- [x] Content
- [x] Channels
- [x] Status
- [ ] Rules
- [ ] Validation

## Phase 3 — Vertical Slices

- [x] Create (done early — Phase 1, `Features/Notifications/Create/`)
- [x] GetById (done early — Phase 1, `Features/Notifications/GetById/`)
- [ ] List
- [ ] MarkAsRead
- [ ] MarkAllAsRead

## Phase 4 — MongoDB + .NET

- [ ] Projections
- [ ] UpdateOne
- [ ] UpdateMany
- [ ] Atomic updates
- [ ] Indexes
- [ ] Aggregation
- [ ] Cursor pagination
- [ ] Bulk operations
- [ ] TTL
- [ ] Transactions
- [ ] Optimization

## Phase 5 — Messaging

- [ ] RabbitMQ connection
- [ ] Topology
- [ ] Publisher
- [ ] Consumer
- [ ] Contract
- [ ] Notification processing
- [ ] Email (Resend)
- [ ] Push (Firebase)
- [ ] Error classification
- [ ] Retry
- [ ] DLQ
- [ ] Idempotency
- [ ] Connection recovery
- [ ] Outbox

## Phase 6 — Redis

- [ ] Cache
- [ ] Rate limiting
- [ ] Deduplication
- [ ] Other justified use cases

## Phase 7 — Reliability

- [ ] Failure scenarios
- [ ] Retry strategy
- [ ] Idempotency
- [ ] Outbox
- [ ] DLQ handling
- [ ] Concurrency
- [ ] Recovery

## Phase 8 — Testing

- [~] Unit tests (started Lesson 7 — grows incrementally per section 2.7, not a single deliverable)
- [ ] Integration tests
- [ ] Testcontainers
- [ ] API tests
- [ ] Consumer tests

## Phase 9 — Observability

- [ ] Structured logging
- [ ] Health checks
- [ ] OpenTelemetry
- [ ] Tracing
- [ ] Metrics

## Phase 10 — Portfolio

- [ ] Docker Compose
- [ ] README
- [ ] Architecture diagram
- [ ] ADRs
- [ ] API documentation
- [ ] Test documentation
- [ ] CI/CD

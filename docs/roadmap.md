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
- [x] Rules (channel-type uniqueness, Push requires recipients)
- [x] Validation (Create's Command: email format, required fields, non-empty channels)

## Phase 3 — Vertical Slices

- [x] Create (done early — Phase 1, `Features/Notifications/Create/`)
- [x] GetById (done early — Phase 1, `Features/Notifications/GetById/`)
- [x] List (email filter + simple limit, not cursor pagination — that's Phase 4)
- [x] MarkAsRead
- [x] MarkAllAsRead

## Phase 4 — MongoDB + .NET

- [x] Projections (List returns NotificationSummary, not the full document)
- [x] UpdateOne (done early — Phase 3, MarkAsRead's FindOneAndUpdate)
- [x] UpdateMany (done early — Phase 3, MarkAllAsRead)
- [x] Atomic updates (done early — Phase 3, array-filtered updates on both)
- [x] Indexes (compound Email+CreatedAt index, created at startup)
- [ ] Aggregation — no real feature needs it yet; deferred rather than inventing one
- [x] Cursor pagination (List's keyset cursor, CreatedAt+Id tie-break)
- [ ] Bulk operations — real need arrives with Phase 5's batch message/channel processing
- [ ] TTL — real need arrives with Phase 5's processed_messages idempotency table
- [x] Transactions (Create's Notification + OutboxMessage insert, single-node replica set)
- [ ] Optimization — ongoing activity, not a single deliverable; revisited once there's real load/query data to look at

## Phase 5 — Messaging

- [x] Messaging architecture (5.1 — responsibilities already defined in guide section 9, applied as-is)
- [x] RabbitMQ connection
- [x] Topology (notifications.exchange direct → notifications.queue, routing key notification.created)
- [x] Publisher (RabbitMqPublisher, publisher confirms enabled, verified standalone)
- [x] Contract (NotificationCreatedMessage)
- [x] Outbox (transactional Create + OutboxProcessor background service, verified end-to-end including the rollback path)
- [ ] Consumer
- [ ] Notification processing
- [ ] Email (Resend)
- [ ] Push (Firebase)
- [ ] Error classification
- [ ] Retry
- [ ] DLQ
- [ ] Idempotency
- [ ] Connection recovery

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

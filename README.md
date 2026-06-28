# Purchase Request Approval — Elsa Workflow Demo

A backend demo showing how to build a multi-step approval workflow using [Elsa Workflows v3](https://docs.elsaworkflows.io) with ASP.NET Core, PostgreSQL, and a custom identity bridge.

The workflow covers the full lifecycle of a purchase request: submission → manager review → finance review → outcome.

---

## How it works

An employee submits a purchase request via HTTP. The workflow suspends, waits for a manager to approve or reject, and if approved, hands off to finance for a second review. Each approval step is an external task — the workflow holds state between steps, so nothing is lost if the server restarts.

![image](/docs/image.png)

## Stack

| Layer       | Technology                                              |
| ----------- | ------------------------------------------------------- |
| Runtime     | .NET 10 / ASP.NET Core                                  |
| Workflows   | Elsa Workflows 3.5.3                                    |
| Identity    | ASP.NET Core Identity (custom bridge to Elsa)           |
| Database    | PostgreSQL (EF Core, separate schemas for app and Elsa) |
| Workflow UI | Elsa Studio (Docker)                                    |

---

## Running locally

**Prerequisites:** Docker, .NET 10 SDK, Make

```bash
make up
```

That's it. Starts PostgreSQL and Elsa Studio in the background, waits for the database to be ready, then runs the API in the foreground.

- <http://localhost:5000> — API
- <http://localhost:3000> — Elsa Studio (workflow designer)

**Studio login:** `admin` / `Admin@12345!`

---

## Roles

| Role     | Access                               |
| -------- | ------------------------------------ |
| Admin    | Full access                          |
| Manager  | Approves/rejects at the manager step |
| Finance  | Approves/rejects at the finance step |
| Employee | Submits requests                     |

---

## API

### Submit a purchase request

```bash
POST /purchase-request
Content-Type: application/json

{
  "employee": "Ali",
  "amount": 1200,
  "description": "Laptop"
}
```

Response includes `instanceId` — keep it, you need it for approvals.

### Check pending tasks for an instance

```bash
GET /api/approvals/{instanceId}/tasks
```

### Manager decision

```bash
POST /api/approvals/{instanceId}/manager
Content-Type: application/json

{ "decision": "approved" }   # or "rejected"
```

### Finance decision

```bash
POST /api/approvals/{instanceId}/finance
Content-Type: application/json

{ "decision": "approved" }   # or "rejected"
```

---

## Project structure

```text
backend/
├── Controllers/
│   ├── AuthController.cs          # Login, token issuance
│   └── ApprovalsController.cs     # Manager + finance approval endpoints
├── Data/                          # EF Core DbContext and entities
├── Identity/                      # Role constants
├── Infrastructure/
│   ├── AspNetIdentityUserProvider.cs           # Bridges ASP.NET Identity → Elsa identity model
│   └── AspNetIdentityUserCredentialsValidator.cs
├── Security/                      # JWT options and token service
├── Seeding/                       # Seeds roles and admin user on startup
└── Extensions/                    # Service registration helpers

workflows/
└── purchase-request-approval.json  # Workflow definition (import via Elsa Studio)
```

---

## Notes

- The workflow definition lives in `workflows/` as a JSON export. Import it in Elsa Studio under **Workflow Definitions → Import**.
- Workflow state is persisted to PostgreSQL — approvals survive server restarts.
- The approval endpoints resolve the internal Elsa `taskId` from the bookmark store so callers only need the `instanceId`.

---

## Tear down

```bash
make down          # Stop containers, keep data
make clean         # Stop and wipe everything
```

# ELSA Workflows — Demo Setup

## Demo (PostgreSQL + separate containers)

**File:** `docker-compose.full.yml`

```bash
docker compose -f docker-compose.full.yml up
```

| URL                                                              | Purpose                   |
| ---------------------------------------------------------------- | ------------------------- |
| [http://localhost:13000](http://localhost:13000)                 | Elsa Studio (designer UI) |
| [http://localhost:12000](http://localhost:12000)                 | Elsa Server (API)         |
| [http://localhost:12000/swagger](http://localhost:12000/swagger) | Swagger / OpenAPI docs    |

**Login:** `admin` / `password`

This setup mirrors a real deployment: Studio and Server are separate services,
and PostgreSQL is used for persistence.

---

## Stopping & cleaning up

```bash
# Stop containers (keeps volumes/data)
docker compose down

# Stop AND delete all data
docker compose down -v
```

## References

- Docs: [https://docs.elsaworkflows.io](https://docs.elsaworkflows.io)
- Docker Compose guide: [https://docs.elsaworkflows.io/getting-started/containers/docker-compose](https://docs.elsaworkflows.io/getting-started/containers/docker-compose)

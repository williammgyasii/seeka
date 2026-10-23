## Context

The API and Hangfire already share one Npgsql connection string and EF Core
migrations are applied only through the explicit `--migrate` command. Docker
Compose currently constructs that connection string from a local PostgreSQL
service. Neon CLI 4.13.1 is installed and authenticated to the selected
organisation.

## Goals / Non-Goals

**Goals:**

- Use an isolated Neon development project with PostgreSQL 18.
- Connect the API directly to the branch compute endpoint with TLS required.
- Keep credentials out of tracked files and logs.
- Preserve the explicit migration workflow and current data model.

**Non-Goals:**

- Migrating disposable local fixture data.
- Adding Neon SDK dependencies to application code.
- Introducing a pooled endpoint, production database, or branching workflow.

## Decisions

### Provision one dedicated project through Neon CLI

Create `seeka-dev` in `aws-us-east-2` with database `seeka` and role
`seeka_owner`. A separate project provides cleaner ownership and deletion than
placing the database in an unrelated existing project. PostgreSQL 18 matches
the version already exercised by the local stack.

### Use the direct endpoint temporarily

Request the connection string without `--pooled`, convert it to Npgsql's
key/value format, and require TLS. This follows the requested direct connection
and is adequate for one development API instance. A pooled endpoint is the
preferred later option when instance or connection concurrency grows.

### Inject configuration at the process boundary

Docker Compose reads `ConnectionStrings__Seeka` from an ignored `.env`; local
`dotnet run` uses ASP.NET Core user secrets. Tracked examples contain only a
placeholder. The API continues to own no Neon-specific code.

### Replace, rather than retain, the default local database service

Remove PostgreSQL from the default Compose topology so a successful startup
cannot silently use a different database. Developers who need offline work can
restore a local override later, but dual default paths would make data location
ambiguous.

## Risks / Trade-offs

- [Direct connections consume one Neon connection per pool member] → Keep one
  API instance now and reassess the pooled endpoint before scaling.
- [A shared remote development database requires network access] → Document the
  dependency and retain the explicit failure rather than silently falling back.
- [A leaked `.env` would expose the database] → Ignore `.env`, use placeholders
  in examples, and document credential rotation.
- [Existing local fixture data is not copied] → Treat it as disposable and
  verify the new database with a fresh registration workflow.

## Migration Plan

1. Provision the Neon project/database and obtain the direct connection secret.
2. Store the connection locally in `.env` and ASP.NET Core user secrets.
3. Update Compose and tracked configuration/documentation.
4. Run the existing explicit migration command against Neon.
5. Start the API and web containers, then verify health and a persisted workflow.
6. Stop the obsolete local PostgreSQL container without deleting its volume
   until Neon verification succeeds.

Rollback uses the previous local connection string and PostgreSQL Compose
service from the prior configuration; the retained volume preserves its data
during verification.

## Why

The development stack currently depends on a machine-local PostgreSQL
container, so its data and availability are tied to one Docker environment.
Using a dedicated Neon database gives the application a durable shared
development resource while retaining PostgreSQL compatibility.

## What Changes

- Provision a dedicated Neon development project and PostgreSQL database.
- Configure the API and Hangfire to use Neon's direct, TLS-protected endpoint.
- Remove the local PostgreSQL service from the default Docker Compose stack.
- Keep the connection credential outside tracked configuration.
- Document provisioning, migration, rotation, and local startup.

## Capabilities

### New Capabilities

None. This is an infrastructure substitution with no new product behavior.

### Modified Capabilities

None. Existing job-seeker workflow requirements remain unchanged.

## Impact

The change affects Neon infrastructure, API database configuration,
`docker-compose.yml`, development configuration templates, secret handling, and
setup documentation. EF Core, Npgsql, migrations, and the application data model
remain unchanged.

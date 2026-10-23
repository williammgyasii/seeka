## 1. Provision Neon

- [x] 1.1 Create the `seeka-dev` Neon project with PostgreSQL 18, database `seeka`, and role `seeka_owner`; verify the CLI returns the expected project and database metadata
- [x] 1.2 Obtain the non-pooled TLS connection parameters and store them in ignored local configuration; verify no credential appears in tracked examples or command output retained by the project

## 2. Connect the stack

- [x] 2.1 Replace the default local PostgreSQL Compose dependency with the required external connection setting; verify Compose configuration resolves successfully
- [x] 2.2 Remove the embedded development database credential, add secret-safe templates and ignore rules, and update setup/rotation documentation; verify tracked configuration contains placeholders only

## 3. Migrate and verify

- [x] 3.1 Run the explicit EF Core migration command against Neon; verify the migration history and application tables exist
- [x] 3.2 Run backend tests and builds, start the API/web stack against Neon, and verify health plus persistence through a real workflow
- [x] 3.3 Validate the OpenSpec change and record all tasks complete

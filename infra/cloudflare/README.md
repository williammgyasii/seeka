# Cloudflare infrastructure

Terraform owns stable resource identities and settings:

- the `seeka` Direct Upload Pages project;
- the private `seeka-api` Worker identity;
- the Pages-to-Worker service binding;
- the `app.seekajobs.tech` custom domain on the Pages project, and its proxied
  CNAME in the `seekajobs.tech` zone (looked up by name, so no zone ID is
  committed).

The deployment token therefore needs Zone Read and DNS Edit on `seekajobs.tech`
in addition to its account permissions.

Wrangler owns Worker versions, container images and rollouts, and Pages
deployment artifacts. Do not add those mutable artifacts to Terraform.

## One-time state bootstrap

Create the private state bucket with an authenticated Wrangler session:

```bash
wrangler r2 bucket create seeka-terraform-state
```

For local administration, derive short-lived credentials restricted to this
bucket from an authorised parent R2 token and export them without writing them
to the repository:

```bash
export AWS_ACCESS_KEY_ID=...
export AWS_SECRET_ACCESS_KEY=...
export AWS_SESSION_TOKEN=...
export AWS_ENDPOINT_URL_S3=https://$CLOUDFLARE_ACCOUNT_ID.r2.cloudflarestorage.com
```

Production CI requests one-hour `object-read-write` credentials from
Cloudflare's R2 temporary-credentials API immediately before Terraform runs.
Only the parent deployment token is stored in the protected GitHub environment.

The backend uses Terraform's native S3 lockfile. R2 provides strongly consistent
conditional object writes, so concurrent applies cannot both acquire the lock.

## Validate and apply

```bash
terraform -chdir=infra/cloudflare fmt -check -recursive
terraform -chdir=infra/cloudflare init
terraform -chdir=infra/cloudflare validate
terraform -chdir=infra/cloudflare test
terraform -chdir=infra/cloudflare plan -out=seeka.tfplan
terraform -chdir=infra/cloudflare apply seeka.tfplan
```

Supply `TF_VAR_cloudflare_account_id` and `TF_VAR_cloudflare_api_token` through
the local shell or the protected GitHub production environment. Never put
credential values in `terraform.tfvars`, plans, workflow YAML, or committed
files.

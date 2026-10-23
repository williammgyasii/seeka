# Cloudflare infrastructure

Terraform owns stable resource identities and settings:

- the `seeka` Direct Upload Pages project;
- the private `seeka-api` Worker identity;
- the Pages-to-Worker service binding.

Wrangler owns Worker versions, container images and rollouts, and Pages
deployment artifacts. Do not add those mutable artifacts to Terraform.

## One-time state bootstrap

Create the private state bucket with an authenticated Wrangler session:

```bash
wrangler r2 bucket create seeka-terraform-state
```

Create a narrowly scoped R2 S3 credential for this bucket in Cloudflare, then
export it without writing it to the repository:

```bash
export AWS_ACCESS_KEY_ID=...
export AWS_SECRET_ACCESS_KEY=...
export AWS_ENDPOINT_URL_S3=https://$CLOUDFLARE_ACCOUNT_ID.r2.cloudflarestorage.com
```

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

resource "cloudflare_worker" "api" {
  account_id = var.cloudflare_account_id
  name       = "seeka-api"

  observability = {
    enabled            = true
    head_sampling_rate = 1
    logs = {
      enabled            = true
      head_sampling_rate = 1
      invocation_logs    = true
      persist            = true
    }
  }

  subdomain = {
    enabled          = false
    previews_enabled = false
  }
}

resource "cloudflare_pages_project" "web" {
  account_id        = var.cloudflare_account_id
  name              = "seeka"
  production_branch = var.production_branch

  lifecycle {
    ignore_changes = [
      deployment_configs.preview.wrangler_config_hash,
      deployment_configs.production.wrangler_config_hash,
    ]
  }

  deployment_configs = {
    preview = {
      compatibility_date = "2026-10-23"
      services = {
        SEEKA_API = {
          service = cloudflare_worker.api.name
        }
      }
    }
    production = {
      compatibility_date = "2026-10-23"
      services = {
        SEEKA_API = {
          service = cloudflare_worker.api.name
        }
      }
    }
  }
}

data "cloudflare_zone" "seeka" {
  filter = {
    name = var.zone_name
  }
}

resource "cloudflare_pages_domain" "app" {
  account_id   = var.cloudflare_account_id
  project_name = cloudflare_pages_project.web.name
  name         = var.app_hostname
}

resource "cloudflare_dns_record" "app" {
  zone_id = data.cloudflare_zone.seeka.id
  name    = var.app_hostname
  type    = "CNAME"
  content = cloudflare_pages_project.web.subdomain
  proxied = true
  ttl     = 1
  comment = "Seeka web app on Cloudflare Pages; managed by Terraform."
}

# Created in the dashboard first so a bucket-scoped R2 token could exist before the first deploy.
import {
  to = cloudflare_r2_bucket.resumes
  id = "${var.cloudflare_account_id}/seeka-resumes/default"
}

resource "cloudflare_r2_bucket" "resumes" {
  account_id = var.cloudflare_account_id
  name       = "seeka-resumes"
}

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

mock_provider "cloudflare" {}

variables {
  cloudflare_account_id = "00000000000000000000000000000000"
  cloudflare_api_token  = "test-token"
}

run "delivery_resources_have_safe_contracts" {
  command = plan

  assert {
    condition     = cloudflare_worker.api.name == "seeka-api"
    error_message = "The Worker identity must remain stable for the Pages service binding."
  }

  assert {
    condition     = cloudflare_worker.api.subdomain.enabled == false
    error_message = "The API must not expose a workers.dev route."
  }

  assert {
    condition     = cloudflare_pages_project.web.production_branch == "main"
    error_message = "Pages production deployments must follow main."
  }

  assert {
    condition     = cloudflare_pages_project.web.deployment_configs.production.services.SEEKA_API.service == "seeka-api"
    error_message = "Pages must bind API traffic to the private Worker."
  }
}

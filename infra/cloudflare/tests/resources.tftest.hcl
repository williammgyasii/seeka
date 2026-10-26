mock_provider "cloudflare" {}

override_resource {
  target = cloudflare_r2_bucket.resumes
  values = {
    id = "seeka-resumes"
  }
}

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

run "app_is_served_at_the_branded_domain" {
  command = plan

  override_data {
    target = data.cloudflare_zone.seeka
    values = {
      id = "11111111111111111111111111111111"
    }
  }

  assert {
    condition     = cloudflare_pages_domain.app.name == "app.seekajobs.tech"
    error_message = "The Pages project must serve app.seekajobs.tech."
  }

  assert {
    condition     = cloudflare_pages_domain.app.project_name == "seeka"
    error_message = "The custom domain must attach to the seeka Pages project."
  }

  assert {
    condition     = cloudflare_dns_record.app.zone_id == "11111111111111111111111111111111"
    error_message = "The DNS record must live in the looked-up seekajobs.tech zone."
  }

  assert {
    condition     = cloudflare_dns_record.app.name == "app.seekajobs.tech" && cloudflare_dns_record.app.type == "CNAME"
    error_message = "app.seekajobs.tech must be a CNAME."
  }

  assert {
    condition     = cloudflare_dns_record.app.proxied == true
    error_message = "The app record must be proxied so Cloudflare terminates TLS."
  }
}

run "resumes_have_a_private_bucket" {
  command = plan

  assert {
    condition     = cloudflare_r2_bucket.resumes.name == "seeka-resumes"
    error_message = "The API stores résumés in the seeka-resumes bucket."
  }
}

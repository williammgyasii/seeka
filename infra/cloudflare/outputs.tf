output "pages_project_name" {
  description = "Pages project used by Wrangler Direct Upload."
  value       = cloudflare_pages_project.web.name
}

output "pages_subdomain" {
  description = "Generated public Pages hostname."
  value       = cloudflare_pages_project.web.subdomain
}

output "worker_name" {
  description = "Private Worker identity used by Wrangler deployments."
  value       = cloudflare_worker.api.name
}

output "marketing_url" {
  description = "Public marketing site."
  value       = "https://${cloudflare_pages_domain.marketing_apex.name}"
}

output "app_url" {
  description = "Branded production URL for the web app."
  value       = "https://${cloudflare_pages_domain.app.name}"
}

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

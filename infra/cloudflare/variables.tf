variable "cloudflare_account_id" {
  description = "Cloudflare account that owns the Pages and Workers resources."
  type        = string
}

variable "cloudflare_api_token" {
  description = "Scoped token supplied through CLOUDFLARE_API_TOKEN or TF_VAR_cloudflare_api_token."
  type        = string
  sensitive   = true
}

variable "production_branch" {
  description = "Git revision allowed to publish production deployments."
  type        = string
  default     = "main"
}

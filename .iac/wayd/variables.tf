variable "location" {
  type        = string
  description = "The location/region where the resource group and most resources should exist."
  default     = "westus3"
}

variable "swa_location" {
  type        = string
  description = "The Azure region for the Static Web App. SWA is only available in a limited set of regions."
  default     = "westus2"
}

variable "environment" {
  type        = string
  description = "Deployment environment name (e.g. dev, staging, prod). Used for tagging."
  default     = "dev"
}

variable "project" {
  type        = string
  description = "Project name used for tagging resources."
  default     = "wayd"
}

variable "sql_admin_pass" {
  type        = string
  description = "The password for the SQL Server administrator."
  sensitive   = true
}

variable "aad_api_client_secret" {
  type        = string
  description = "The client secret for the AAD API app registration."
  sensitive   = true
}

variable "app_reg_client_id" {
  type        = string
  description = "The client ID for the AAD client-side (frontend) app registration."
}

variable "api_app_reg_client_id" {
  type        = string
  description = "The client ID for the AAD API (backend) app registration."
}

variable "app_reg_api_scope" {
  type        = string
  description = "The full API scope URI for the AAD API app registration (e.g. api://<client-id>/access_as_user)."
}

variable "aad_tenant_id" {
  type        = string
  description = "The Azure AD tenant ID."
}

variable "aad_domain" {
  type        = string
  description = "The Azure AD tenant primary domain (e.g. contoso.onmicrosoft.com)."
}

variable "swagger_openid_client_id" {
  type        = string
  description = "The client ID for the Swagger OpenID app registration (used for the Swagger UI 'Try it out' auth flow)."
}

variable "sql_ad_admin_object_id" {
  type        = string
  description = "The Azure AD object ID of the group or user configured as the SQL Server AD administrator."
}

variable "local_jwt_secret" {
  type        = string
  description = "The secret key for signing local JWT tokens. Must be at least 32 characters."
  sensitive   = true
}

variable "local_jwt_token_expiration_minutes" {
  type        = number
  description = "The expiration time in minutes for local JWT tokens."
  default     = 60
}

variable "local_jwt_refresh_token_expiration_days" {
  type        = number
  description = "The expiration time in days for local JWT refresh tokens."
  default     = 7
}

variable "docker_tag" {
  type        = string
  description = "The tag for the Docker image."
  default     = "latest"
}

variable "client_url" {
  type        = string
  description = "Additional client URL to allow in CORS for the API. Leave empty if the SWA host is the only allowed client."
  default     = ""
}

variable "signalr_name" {
  type        = string
  description = "The name of the Azure SignalR Service instance."
  default     = "sigr-wayd-dev"
}

variable "signalr_sku" {
  type        = string
  description = "The SKU name for the Azure SignalR Service (e.g., Free_F1, Standard_S1)."
  default     = "Free_F1"
}

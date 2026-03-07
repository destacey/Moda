variable "location" {
  type        = string
  description = "The location/region where the resource group should exist."
  default     = "westus3"
}

variable "sql_admin_pass" {
  type        = string
  description = "The password for the SQL Server administrator."
  default     = ""
  sensitive   = true
}

variable "aad_api_client_secret" {
  type        = string
  description = "The client secret for the AAD API."
  default     = ""
  sensitive   = true
}

variable "app_reg_client_id" {
  type        = string
  description = "The client ID for the AAD App Registration."
  default     = ""
  sensitive   = true
}

variable "app_reg_api_scope" {
  type        = string
  description = "The API scope for the AAD App Registration."
  default     = ""
  sensitive   = true
}

variable "aad_tenant_id" {
  type        = string
  description = "The tenant ID for the AAD App Registration."
  default     = ""
  sensitive   = true
}

variable "local_jwt_secret" {
  type        = string
  description = "The secret key for signing local JWT tokens. Must be at least 32 characters."
  default     = ""
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
  description = "The client url for CORS for the API."
  default     = ""
}

variable "signalr_name" {
  type        = string
  description = "The name of the Azure SignalR Service instance."
  default     = "sigr-moda-dev"
}

variable "signalr_sku" {
  type        = string
  description = "The SKU name for the Azure SignalR Service (e.g., Free_F1, Standard_S1)."
  default     = "Free_F1"
}

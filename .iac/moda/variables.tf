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

variable "docker_tag" {
  type        = string
  description = "The tag for the Docker image."
  default     = "latest"
}

variable "api_url" {
  type        = string
  description = "The URL for the API."
  default     = ""
}

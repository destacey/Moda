# Configure the Azure provider
terraform {
  required_version = ">= 1.14.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.69"
    }
  }
  cloud {
    organization = "wayd"
    workspaces {
      tags = ["wayd"]
    }
  }
}

# Subscription and tenant IDs are sourced from ARM_SUBSCRIPTION_ID / ARM_TENANT_ID
# environment variables set on the Terraform Cloud workspace.
provider "azurerm" {
  features {}
}

data "azurerm_client_config" "current" {}

locals {
  # e.g. project="wayd", environment="dev" -> "wayd-dev"
  name_stem = "${var.project}-${var.environment}"

  common_tags = {
    environment = var.environment
    project     = var.project
    managed_by  = "terraform"
  }
}

resource "azurerm_resource_group" "wayd_dev_rg" {
  name     = "rg-${local.name_stem}"
  location = var.location
  tags     = local.common_tags
}


# Configure the Azure provider
terraform {
  required_version = ">= 1.1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.24.0"
    }
  }
  cloud {
    organization = "moda"
    workspaces {
      tags = ["moda"]
    }
  }
}

// Leave this here, but don't use it. When using aliased providers, there has to be
// a provider without an alias in order for plans and applies to work. We declare one here
// to satisfy the CLI's validation, and we use the aliased providers for the resources
// because we'll have one per subscription.
provider "azurerm" {
  features {}
  subscription_id = "ed2e6819-f47c-416b-a022-23dfbf609330" // Can also be set via ARM_SUBSCRIPTION_ID
  tenant_id       = "f399216f-be6b-4062-8700-54952e44e7ef" // Can also be set via ARM_TENANT_ID
}

data "azurerm_client_config" "current" {}


resource "azurerm_resource_group" "moda_dev_rg" {
  name     = "rg-moda-dev"
  location = var.location
}


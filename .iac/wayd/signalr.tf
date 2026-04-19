resource "azurerm_signalr_service" "wayd_signalr" {
  name                = coalesce(var.signalr_name, "sigr-${local.name_stem}")
  resource_group_name = azurerm_resource_group.wayd_dev_rg.name
  location            = azurerm_resource_group.wayd_dev_rg.location

  sku {
    name     = var.signalr_sku
    capacity = var.signalr_capacity
  }

  service_mode = "Default"

  cors {
    allowed_origins = [
      # Must match the name pattern used in container-apps.tf for wayd_frontend.
      "https://${var.project}-client-${var.environment}.${azurerm_container_app_environment.wayd_cae.default_domain}",
    ]
  }

  tags = local.common_tags
}

resource "azurerm_static_web_app" "wayd_swa" {
  name                = "swa-wayd"
  resource_group_name = azurerm_resource_group.wayd_dev_rg.name
  location            = var.swa_location
  sku_tier            = "Free"
  sku_size            = "Free"

  tags = local.common_tags
}

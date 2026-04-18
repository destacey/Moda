resource "azurerm_static_site" "moda_swa" {
  name                = "swa-moda"
  resource_group_name = azurerm_resource_group.moda_dev_rg.name
  location            = "westus2"
  sku_tier            = "Free"
  sku_size            = "Free"
}

# resource "azurerm_resource_group_template_deployment" "client_appsettings" {
#   deployment_mode     = "Incremental"
#   name                = "client-appsettings"
#   resource_group_name = azurerm_resource_group.moda_dev_rg.name

#   template_content = file("swa-config.json")
#   parameters_content = jsonencode({
#     staticSiteName = {
#       value = azurerm_static_site.moda_swa.name
#     }
#     tenantId = {
#       value = data.azurerm_client_config.current.tenant_id
#     }
#     clientId = {
#       value = var.app_reg_client_id
#     }
#     apiScope = {
#       value = var.app_reg_api_scope
#     }
#     apiBaseUrl = {
#       value = azurerm_linux_web_app.api.default_hostname
#     }
#   })
# }

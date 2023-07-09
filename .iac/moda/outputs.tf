output "sql_server_connection_string" {
  value     = local.sql_conn_string
  sensitive = true
}

output "api_hostname" {
  value = azurerm_container_app.moda_backend.ingress.0.fqdn
}

output "client_hostname" {
  value = azurerm_container_app.moda_frontend.ingress.0.fqdn
}

output "swa_token" {
  value     = azurerm_static_site.moda_swa.api_key
  sensitive = true
}

output "swa_hostname" {
  value = azurerm_static_site.moda_swa.default_host_name
}

output "sql_server_connection_string" {
  value     = local.sql_conn_string
  sensitive = true
}

output "api_hostname" {
  value = azurerm_container_app.wayd_backend.ingress.0.fqdn
}

output "client_hostname" {
  value = azurerm_container_app.wayd_frontend.ingress.0.fqdn
}

output "signalr_hostname" {
  value = azurerm_signalr_service.wayd_signalr.hostname
}

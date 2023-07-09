locals {
  sql_conn_string = "Server=tcp:${azurerm_mssql_server.moda_sql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.moda_db.name};Persist Security Info=False;User ID=modaadmin;Password=${var.sql_admin_pass};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}

resource "azurerm_container_app_environment" "moda_cae" {
  name                       = "cae-moda"
  resource_group_name        = azurerm_resource_group.moda_dev_rg.name
  location                   = azurerm_resource_group.moda_dev_rg.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.moda.id

}

resource "azurerm_log_analytics_workspace" "moda" {
  location            = azurerm_resource_group.moda_dev_rg.location
  name                = "la-moda"
  resource_group_name = azurerm_resource_group.moda_dev_rg.name
}

resource "azurerm_container_app" "moda_frontend" {
  name                         = "moda-client"
  container_app_environment_id = azurerm_container_app_environment.moda_cae.id
  resource_group_name          = azurerm_resource_group.moda_dev_rg.name
  revision_mode                = "Single"

  ingress {
    external_enabled = true
    target_port      = 3000

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  identity {
    type = "SystemAssigned"
  }

  template {
    min_replicas = 0
    max_replicas = 3

    container {
      name   = "moda-client"
      image  = "awaldow/moda-client:${var.docker_tag}"
      cpu    = 0.25
      memory = "0.5Gi"

      # liveness_probe {
      #   port                    = 8080
      #   transport               = "HTTP"
      #   failure_count_threshold = 3
      #   path                    = "/health"
      #   interval_seconds        = 30
      #   initial_delay           = 15
      # }

      readiness_probe {
        port                    = 3000
        transport               = "HTTP"
        path                    = "/api/health"
        timeout                 = 3
        failure_count_threshold = 5
        interval_seconds        = 30
      }

      startup_probe {
        port                    = 3000
        transport               = "HTTP"
        path                    = "/api/health"
        timeout                 = 2
        failure_count_threshold = 5
        interval_seconds        = 10
      }
    }
  }
}

resource "azurerm_container_app" "moda_backend" {
  name                         = "moda-api"
  container_app_environment_id = azurerm_container_app_environment.moda_cae.id
  resource_group_name          = azurerm_resource_group.moda_dev_rg.name
  revision_mode                = "Single"

  ingress {
    external_enabled = true
    target_port      = 8080

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  secret {
    name  = "sql-conn-string"
    value = local.sql_conn_string
  }

  secret {
    name  = "aad-client-secret"
    value = var.aad_api_client_secret
  }

  identity {
    type = "SystemAssigned"
  }

  template {
    min_replicas = 0
    max_replicas = 3

    container {
      name   = "moda-api"
      image  = "awaldow/moda-api:${var.docker_tag}"
      cpu    = 0.25
      memory = "0.5Gi"

      # liveness_probe {
      #   port                    = 8080
      #   transport               = "HTTP"
      #   failure_count_threshold = 3
      #   path                    = "/health"
      #   interval_seconds        = 30
      #   initial_delay           = 15
      # }

      readiness_probe {
        port                    = 8080
        transport               = "HTTP"
        path                    = "/startup"
        timeout                 = 3
        failure_count_threshold = 5
        interval_seconds        = 30
      }

      startup_probe {
        port                    = 8080
        transport               = "HTTP"
        path                    = "/startup"
        timeout                 = 2
        failure_count_threshold = 5
        interval_seconds        = 10
      }

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "ASPNETCORE_URLS"
        value = "http://+:8080"
      }

      env {
        name        = "DatabaseSettings__ConnectionString"
        secret_name = "sql-conn-string"
      }

      env {
        name        = "HangfireSettings__Storage__ConnectionString"
        secret_name = "sql-conn-string"
      }

      env {
        name        = "SecuritySettings__AzureAd__ClientSecret"
        secret_name = "aad-client-secret"
      }

      env {
        name  = "SecuritySettings__AzureAd__ClientId"
        value = "fdca5e6f-46a2-455c-b2f3-06a9a6877190"
      }

      env {
        name  = "SecuritySettings__AzureAd__Domain"
        value = "dstaceyoutlook.onmicrosoft.com"
      }

      env {
        name  = "SecuritySettings__AzureAd__RootIssuer"
        value = "https://sts.windows.net/f399216f-be6b-4062-8700-54952e44e7ef/"
      }

      env {
        name  = "SecuritySettings__AzureAd__TenantId"
        value = "f399216f-be6b-4062-8700-54952e44e7ef"
      }

      env {
        name  = "SecuritySettings__Swagger__ApiScope"
        value = "api://fdca5e6f-46a2-455c-b2f3-06a9a6877190/access_as_user"
      }

      env {
        name  = "SecuritySettings__Swagger__AuthorizationUrl"
        value = "https://login.microsoftonline.com/f399216f-be6b-4062-8700-54952e44e7ef/oauth2/v2.0/authorize"
      }

      env {
        name  = "SecuritySettings__Swagger__OpenIdClientId"
        value = "4d566fb3-7966-4c77-9864-113020fd646f"
      }

      env {
        name  = "SecuritySettings__Swagger__TokenUrl"
        value = "https://login.microsoftonline.com/f399216f-be6b-4062-8700-54952e44e7ef/oauth2/v2.0/token"
      }

      env {
        name  = "CorsSettings__WebClient"
        value = "https://${azurerm_static_site.moda_swa.default_host_name},https://${azurerm_container_app.moda_frontend.ingress.0.fqdn}"
      }
    }
  }
}

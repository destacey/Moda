# Moda

Moda is a work management system used to plan, manage, and create associations across work items, projects, teams, planning and products.  It helps track, align, and deliver work across organizations.

[Moda Docs](https://destacey.github.io/Moda)

## Local Debugging

The easiest way to debug locally is to run the `Compound: Launch Moda with Compose` launch configuration. In order for this to work, you need to create the following file in the root of the project:
a `.env` file with the following contents:

```env
AAD_CLIENT_ID='{your AAD client ID}'
AAD_TENANT_ID='{your AAD tenant ID}'
AAD_LOGON_AUTHORITY='https://login.microsoftonline.com/{your AAD tenant ID}'
API_SCOPE='{scope to attach to API requests; for AAD this is usually api://{client ID}/access_as_user}'
API_BASE_URL='https://localhost:5001'
```

These values will be used during the `docker compose up` command to set environment variables in the container. Due to limitations in how Next.js allows us to set environment variables, instead of using the values as build time we do some shell-gaming to get them to replace at container startup. As a consequence, a container restart is not sufficient to reflect changes, it has to be torn down and brought back up. This is accomplished by the launch configuration, as it will both Compose up when you run it, and when it gets cancelled it will compose down to make sure the env is clean.

## Deployment

If you plan to use the client container, the following environment variables must be set:

```env
NEXT_PUBLIC_AZURE_AD_CLIENT_ID='{your client ID}'
NEXT_PUBLIC_AZURE_AD_TENANT_ID='{your tenant ID}'
NEXT_PUBLIC_MICROSOFT_LOGON_AUTHORITY='{your login authority}'
NEXT_PUBLIC_API_SCOPE='{your API scope}'
NEXT_PUBLIC_API_BASE_URL='{Your API URL}'
```

The API container needs the following:

```env
CorsSettings__WebClient={your client URL}
DatabaseSettings__ConnectionString={connection string to your database}
HangfireSettings__Storage__ConnectionString={connection string to your database}
SecuritySettings__AzureAd__ClientSecret={client secret to your API app reg}
SecuritySettings__AzureAd__ClientId={client ID to your API app reg}
SecuritySettings__AzureAd__Domain={your domain}
SecuritySettings__AzureAd__RootIssuer={your root issuer/sts url for AAD}
SecuritySettings__AzureAd__TenantId={your tenant ID}
```

Additionally, by default Moda logs to the console via Serilog. If you wish to configure any of the other supported sinks (currently Seq, Datadog and Application Insights), provide the appropriate Serilog__Using__x and Serilog__WriteTo__x settings as env vars for your moda-api container. An example with DataDog (taken from some TF for the `azurerm_container_app` resource type):

```terraform
      env {
        name  = "Serilog__Using__1"
        value = "Serilog.Sinks.Datadog.Logs"
      }

      env {
        name  = "Serilog__WriteTo__1__Name"
        value = "DatadogLogs"
      }

      env {
        name  = "Serilog__WriteTo__1__Args__apiKey"
        value = var.dd_api_key
      }

      env {
        name  = "Serilog__WriteTo__1__Args__source"
        value = "csharp"
      }

      env {
        name  = "Serilog__WriteTo__1__Args__host"
        value = "moda-api"
      }

      env {
        name  = "Serilog__WriteTo__1__Args__tags__0"
        value = "product:moda"
      }

      env {
        name  = "Serilog__WriteTo__1__Args__tags__1"
        value = "service:moda-api"
      }
```

> Note, if you use `__0` for the WriteTo and Using values, you will override the default console logging

## License

See [LICENSE](LICENSE.md)

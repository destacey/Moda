# Moda
Moda is a work management system used to plan, manage, and create associations across work items, projects, teams, planning and products.  It helps track, align, and deliver work across organizations.

[Moda Docs](https://destacey.github.io/Moda)

## Local Debugging
The easiest way to debug locally is to run the `Compound: Launch Moda with Compose` launch configuration. In order for this to work, you need to create the following file in the root of the project:
a `.env` file with the following contents:
```
AAD_CLIENT_ID='{your AAD client ID}'
AAD_TENANT_ID='{your AAD tenant ID}'
AAD_LOGON_AUTHORITY='https://login.microsoftonline.com/{your AAD tenant ID}'
API_SCOPE='{scope to attach to API requests; for AAD this is usually api://{client ID}/access_as_user}'
API_BASE_URL='https://localhost:5001'
```

These values will be used during the `docker compose up` command to set environment variables in the container. Due to limitations in how Next.js allows us to set environment variables, instead of using the values as build time we do some shell-gaming to get them to replace at container startup. As a consequence, a container restart is not sufficient to reflect changes, it has to be torn down and brought back up. This is accomplished by the launch configuration, as it will both Compose up when you run it, and when it gets cancelled it will compose down to make sure the env is clean.

## License

See [LICENSE](LICENSE.md)


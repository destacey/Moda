# App Integration

The app integratin domain enables the ability to define and manage integrations with external applications.

## Models

- [Connection](#connection)
- [Connector](#connector)

### Connection

A connection represents an integration with an external application via a Moda connector.  This includes information needed to establish a connection like authentication and endoint configuration data.

### Connector

A connector represents an API that allows an external service to integrate with Moda.  Current options:

- Azure DevOps Boards

Multiple connections for the same connector can be configured.  This is expected when an organization has multiple instances or requires different authentication to connect for different data.

## ERD

```mermaid
erDiagram
    Connector ||--o{ Connection : "has many"
```

## Architecture

The following diagram shows the high-level architecture:

![architecture](./app-integration-architecture.drawio.svg)

## Available Connectors

- [Azure DevOps Boards](#azure-devops-boards)

### Azure DevOps Boards

The Azure DevOps Boards connector enables Moda to connect and retrieve data for the areas below and sychronize the it.  This sychronization is one-way.

| Azure DevOps Boards | Moda |
|--|--|
| Project | Workspace |
| Work Item | Work Item |
| Work Item Type | Work Type |
| Area | establishes the link between a Work Item and a Team |
| Iteration | establishes the link between a Work Item and a Sprint, Increment, or Planning Interval |

#### Configuration

The information required to configure the connector is:

- Organization - this is the Azure DevOps Organization name.
- PersonalAccessToken - this is the token that enables Moda to connect into an instance of Azure DevOps and read work item data.
  - The required access for the token within Azure DevOps is
    | Scope | Access |
    |--|--|
    | Work Items | Read |

A tenant may have multiple Azure DevOps Boards connectors, but each connector must have a unique Organization.  Put differently, each Azure DevOps Organization can only be linked by one connector.

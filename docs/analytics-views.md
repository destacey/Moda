# Analytics Views (V1 Proposal)

## Purpose

Analytics Views provide a reusable, permission-aware way to define and run tabular analytics queries over Moda data. They are designed for dashboards, charts, exports, and API consumers.

This is a Moda-native capability. It does not depend on Azure DevOps Analytics Views.

## Goals

- Saved query definitions with stable IDs.
- Shared and governed access (private, team, tenant).
- Connector-agnostic datasets (Azure DevOps data works because it is already synchronized into Moda models).
- Safe dynamic querying with strict allowlists.

## Non-Goals (V1)

- Ad hoc SQL execution.
- User-defined joins across arbitrary tables.
- Full OLAP cube semantics.
- BI semantic model publishing.

## Core Concepts

- Analytics View: a saved query definition.
- Dataset: a predefined analytical projection (for example Work Items).
- Field: an allowlisted selectable/groupable/filterable column in a dataset.
- Filter: an operator applied to a field.
- Time Grain: day/week/month rollup for date fields.

## Domain Model

### Aggregate: AnalyticsView

- `Id: Guid`
- `Key: string` (human-friendly unique key, optional in V1)
- `Name: string`
- `Description: string?`
- `Dataset: AnalyticsDataset` (enum)
- `DefinitionJson: string` (validated JSON document)
- `Visibility: Visibility` (reuse existing domain enum)
- `OwnerType: AnalyticsOwnerType` (User, Team, Tenant)
- `OwnerId: Guid?`
- `IsActive: bool`
- `Created/CreatedBy/LastModified/LastModifiedBy`

### Value Objects / Enums

- `AnalyticsDataset`
  - `WorkItems`
  - `Dependencies`
  - `PlanningIntervalObjectives`
- `AnalyticsOwnerType`
  - `User`
  - `Team`
  - `Tenant`
- `AnalyticsTimeGrain`
  - `None`
  - `Day`
  - `Week`
  - `Month`
- `AnalyticsSortDirection`
  - `Asc`
  - `Desc`
- `AnalyticsFilterOperator`
  - `Equals`, `NotEquals`, `In`, `NotIn`
  - `Contains`, `StartsWith`, `EndsWith`
  - `GreaterThan`, `GreaterThanOrEqual`
  - `LessThan`, `LessThanOrEqual`
  - `Between`
  - `IsNull`, `IsNotNull`

## Definition Schema (stored in `DefinitionJson`)

```json
{
  "version": 1,
  "dataset": "WorkItems",
  "columns": [
    { "field": "workspace.key", "alias": "Workspace" },
    { "field": "type.name", "alias": "Type" },
    { "field": "statusCategory", "alias": "Status" },
    { "field": "createdDate", "alias": "Created", "timeGrain": "Week" }
  ],
  "filters": [
    { "field": "workspace.key", "operator": "In", "values": ["ABC", "PLAT"] },
    { "field": "isDeleted", "operator": "Equals", "values": [false] }
  ],
  "groupBy": ["workspace.key", "type.name", "statusCategory", "createdDate:Week"],
  "measures": [
    { "type": "Count", "field": "id", "alias": "WorkItemCount" }
  ],
  "sort": [
    { "field": "createdDate:Week", "direction": "Desc" }
  ]
}
```

## API Contracts (V1)

### Endpoints

- `GET /api/analytics/views`
  - list views visible to caller.
- `GET /api/analytics/views/{idOrKey}`
  - details.
- `POST /api/analytics/views`
  - create.
- `PUT /api/analytics/views/{id}`
  - update metadata + definition.
- `POST /api/analytics/views/{id}/run`
  - execute view and return paged tabular result.
- `GET /api/analytics/views/{id}/export?format=csv`
  - export result.
- `DELETE /api/analytics/views/{id}`
  - soft delete.

### Request/Response Types

- `AnalyticsViewListDto`
  - `id, name, dataset, visibility, ownerDisplayName, modifiedOn`
- `AnalyticsViewDetailsDto`
  - list fields plus parsed definition.
- `CreateAnalyticsViewRequest`
  - `name, description, dataset, visibility, ownerType, ownerId, definition`
- `UpdateAnalyticsViewRequest`
  - `id, name, description, visibility, ownerType, ownerId, definition, isActive`
- `RunAnalyticsViewRequest`
  - `pageNumber, pageSize, overrides?` (optional runtime filters in later version)
- `AnalyticsViewResultDto`
  - `columns: ColumnMeta[]`
  - `rows: object[]`
  - `totalRows: int`

## Query Execution Model

Use a dataset registry that defines allowlisted fields and expression builders per dataset.

### Dataset Registry Contract

- `IAnalyticsDatasetDefinition`
  - `Dataset: AnalyticsDataset`
  - `Fields: IReadOnlyCollection<AnalyticsFieldDefinition>`
  - `BuildBaseQuery(IModaDbContext db, ClaimsPrincipal user): IQueryable<TProjection>`
  - `BuildColumnExpression(fieldName)`
  - `BuildFilterExpression(fieldName, operator, values)`

This prevents arbitrary field access and enforces security-by-construction.

## Security and Governance

- Enforce `Visibility` and owner checks in query handlers.
- Apply existing Moda permission model with new `ApplicationResource.AnalyticsViews`.
- Persist only validated definitions.
- Limit page sizes and maximum grouped row counts.
- Audit create/update/delete/run events.

## Initial Datasets

### 1. WorkItems

Candidate fields:
- `id`, `key`, `title`
- `workspace.id`, `workspace.key`, `workspace.name`
- `type.name`, `type.tier`
- `status.name`, `statusCategory`
- `createdDate`, `changedDate`, `doneDate`
- `assignedToId`, `teamId`
- `ownership`, `connector`

Measures:
- `Count(id)`
- later: `Avg(cycleTimeDays)`, `Sum(storyPoints)`

### 2. Dependencies

Candidate fields:
- `id`, `state`, `scope`, `planningHealth`
- `blockedItem.key`, `blockingItem.key`
- `createdDate`, `resolvedDate`
- team/workspace identifiers

Measure:
- `Count(id)`

### 3. PlanningIntervalObjectives

Candidate fields:
- planning interval key/name/date range
- objective key/title/status/type
- owner/team references

Measure:
- `Count(objectiveId)`

## Implementation Map (Moda Structure)

### Domain

Create:
- `Moda.Services/Moda.AppIntegration` is not the right bounded context.
- Add a new service module: `Moda.Services/Moda.Analytics`.
- Domain project:
  - `Moda.Services/Moda.Analytics/src/Moda.Analytics.Domain/Models/AnalyticsView.cs`
  - supporting enums/value objects.

Rationale: analytics is cross-domain and not tied to connector configuration.

### Application

- `Moda.Services/Moda.Analytics/src/Moda.Analytics.Application/AnalyticsViews/Commands/*`
- `Moda.Services/Moda.Analytics/src/Moda.Analytics.Application/AnalyticsViews/Queries/*`
- `Moda.Services/Moda.Analytics/src/Moda.Analytics.Application/AnalyticsViews/Dtos/*`
- Dataset registry + query compiler in:
  - `Moda.Services/Moda.Analytics/src/Moda.Analytics.Application/Execution/*`

### Infrastructure

- Extend shared `ModaDbContext` mapping with `AnalyticsView` entity.
- Add migration in `Moda.Infrastructure.Migrators.MSSQL`.
- Configure DI for dataset registry implementations.

### Web API

- Controller:
  - `Moda.Web/src/Moda.Web.Api/Controllers/Analytics/AnalyticsViewsController.cs`
- Request models:
  - `Moda.Web/src/Moda.Web.Api/Models/Analytics/Views/*`

### Frontend

- New app area:
  - `Moda.Web/src/moda.web.reactclient/src/app/analytics/views/*`
- RTK Query API slice:
  - `Moda.Web/src/moda.web.reactclient/src/store/features/analytics/analytics-views-api.ts`

## Delivery Plan

1. V1.0 Metadata + Execution (MVP)
- CRUD views.
- Run + paginate result.
- CSV export.
- WorkItems dataset only.

2. V1.1 Additional datasets
- Dependencies and Planning Interval Objectives.
- Team-scoped sharing UX.

3. V1.2 Performance
- Materialized daily aggregates for expensive time-series queries.
- Cached results for repeated runs.

## Risks and Mitigations

- Risk: dynamic query complexity.
  - Mitigation: strict dataset registry + allowlisted fields/operators.
- Risk: slow grouped queries.
  - Mitigation: enforced limits, indexes, materialization later.
- Risk: permission leakage.
  - Mitigation: visibility checks in query handlers and field-level governance.

## Acceptance Criteria (MVP)

- Users can create/save/update/delete private views on WorkItems.
- Users can run a view and receive a paged table.
- Users can export CSV for a view run.
- Authorization rules enforced for view visibility.
- Audit trail exists for CRUD operations.

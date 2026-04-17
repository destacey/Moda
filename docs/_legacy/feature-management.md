# Feature Management

Wayd uses [Microsoft.FeatureManagement](https://learn.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags-dotnet-core) to control feature visibility across the application. Feature flags are stored in the database and managed through the Settings UI.

## Overview

Feature flags allow features to be toggled on or off at runtime without a code deployment. They are used to:

- Gate new features behind a flag during development
- Gradually roll out features to users
- Provide a kill switch for features that can be disabled instantly

All feature flags are **code-first** — a developer defines the flag in code and adds the corresponding checks. The flag is automatically created in the database on application startup via the seeder.

## Types of Feature Flags

| Type       | Created By         | Archivable | Purpose                                   |
| ---------- | ------------------ | ---------- | ----------------------------------------- |
| **System** | Seeder (from code) | No         | Gate application features defined in code |
| **User**   | Admin (from UI)    | Yes        | Reserved for future use                   |

Currently, all feature flags should be defined as **system flags** in code.

## Adding a New Feature Flag

### Step 1: Define the Flag

Add the flag definition to `Wayd.Common.Domain/FeatureManagement/FeatureFlags.cs`:

```csharp
public static class FeatureFlags
{
    public static readonly FeatureFlagDefinition PlanningPoker = new(
        Names.PlanningPoker,
        "Planning Poker",
        "Controls visibility of the Planning Poker feature.");

    // Add your new flag here
    public static readonly FeatureFlagDefinition MyNewFeature = new(
        Names.MyNewFeature,
        "My New Feature",
        "Description of what this flag controls.");

    public static class Names
    {
        public const string PlanningPoker = "planning-poker";
        public const string MyNewFeature = "my-new-feature"; // kebab-case
    }
}
```

**Naming conventions:**

- Flag names use **kebab-case** (e.g., `planning-poker`, `bulk-work-item-import`)
- The `Names` nested class provides **compile-time constants** for use in attributes like `[FeatureGate]`
- Display names are human-readable (e.g., "Planning Poker")

The `FeatureFlagSeeder` automatically discovers all `FeatureFlagDefinition` fields via reflection and creates any missing flags in the database on startup. New flags are created as **disabled** by default.

### Step 2: Gate the Backend

#### Controller-level gating

Use `[FeatureGate]` to gate an entire controller. When the flag is disabled, all endpoints return **404**:

```csharp
[FeatureGate(FeatureFlags.Names.MyNewFeature)]
public class MyController(ISender sender) : ControllerBase
{
    // All endpoints gated by the flag
}
```

#### Action-level gating

Apply `[FeatureGate]` to individual actions if only some endpoints need gating:

```csharp
[FeatureGate(FeatureFlags.Names.MyNewFeature)]
[HttpGet]
public async Task<ActionResult> GetList() { ... }
```

#### In command/query handlers

Use `IFeatureManager` for runtime checks in business logic:

```csharp
public class MyCommandHandler(ISender sender, IFeatureManager featureManager)
{
    public async Task<Result> Handle(MyCommand command, CancellationToken ct)
    {
        if (!await featureManager.IsEnabledAsync(FeatureFlags.Names.MyNewFeature))
            return Result.Failure("This feature is not currently available.");

        // ... handler logic
    }
}
```

### Step 3: Gate the Frontend

#### Page-level gating

Use the `requireFeatureFlag` HOC to prevent a page from rendering when the flag is off. This triggers a **404** and prevents any API calls from being made:

```tsx
import { authorizePage, requireFeatureFlag } from '@/src/components/hoc'

const MyPage = () => { ... }

// Apply both permission and feature flag checks
// Feature flag wraps the outside — checked first
export default requireFeatureFlag(
  authorizePage(MyPage, 'Permission', 'Permissions.MyFeature.View'),
  'my-new-feature',
)
```

#### Conditional rendering

Use the `useFeatureFlag` hook for conditional rendering within components:

```tsx
import { useFeatureFlag } from "@/src/hooks";

const MyComponent = () => {
  const myFeatureEnabled = useFeatureFlag("my-new-feature");

  if (!myFeatureEnabled) return null;

  return <MyFeatureUI />;
};
```

#### Menu visibility

Gate menu items by passing feature flag state into menu builder functions:

```tsx
const buildMenuItems = (featureFlags: { myNewFeature: boolean }) => [
  ...(featureFlags.myNewFeature
    ? [menuItem("My Feature", "my-feature", "/my-feature")]
    : []),
];

const useAppMenuItems = () => {
  const myNewFeature = useFeatureFlag("my-new-feature");
  return buildMenuItems({ myNewFeature });
};
```

### Step 4: Deploy and Enable

1. Deploy the code — the seeder creates the flag as **disabled**
2. An admin navigates to **Settings > Feature Management > Feature Flags**
3. Uses the row action menu to **Enable** the flag
4. The feature is now live for all users

## Architecture

### Backend

- **Domain entity**: `Wayd.Common.Domain/FeatureManagement/FeatureFlag.cs`
- **Flag definitions**: `Wayd.Common.Domain/FeatureManagement/FeatureFlags.cs`
- **Seeder**: `Wayd.Infrastructure/Persistence/Initialization/FeatureFlagSeeder.cs`
- **Definition provider**: `Wayd.Infrastructure/FeatureManagement/DatabaseFeatureDefinitionProvider.cs` — implements `IFeatureDefinitionProvider` with 30-second caching
- **API controllers**: `Wayd.Web.Api/Controllers/FeatureManagement/` — admin CRUD and client endpoint for enabled flags

### Frontend

- **Feature flag hook**: `hooks/use-feature-flag.ts` — polls enabled flags every 60 seconds
- **Page HOC**: `components/hoc/require-feature-flag.tsx` — triggers 404 when flag is off
- **Admin UI**: `app/settings/feature-management/feature-flags/` — grid with toggle/archive actions and detail drawer
- **RTK Query slices**: `store/features/feature-flags-api.ts` (client) and `store/features/admin/feature-flags-api.ts` (admin)

### Observability

Feature flag evaluations are traced via OpenTelemetry using the `Microsoft.FeatureManagement` ActivitySource. Each feature definition has telemetry enabled, so flag evaluations appear in traces when running with Aspire or an OTLP endpoint.

## Current Feature Flags

| Name             | Display Name   | Description                                                          |
| ---------------- | -------------- | -------------------------------------------------------------------- |
| `planning-poker` | Planning Poker | Controls visibility of Planning Poker and Estimation Scales features |

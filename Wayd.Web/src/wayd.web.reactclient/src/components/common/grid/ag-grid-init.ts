import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community'

// Register all community features for AG Grid.
// This module is imported by the shared grid component to ensure
// registration happens before any grid renders.
// Guard ensures registration runs only once per runtime, even if this
// module is evaluated from multiple chunks.
let registered = false
if (!registered) {
  ModuleRegistry.registerModules([AllCommunityModule])
  registered = true
}

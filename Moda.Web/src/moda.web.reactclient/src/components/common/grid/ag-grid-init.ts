import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community'

// Register all community features for AG Grid.
// This module is imported by the shared grid component to ensure
// registration happens before any grid renders.
ModuleRegistry.registerModules([AllCommunityModule])

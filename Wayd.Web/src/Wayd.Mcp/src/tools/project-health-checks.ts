import type { McpToolDefinition } from '../types.js';

export const definitions: [string, McpToolDefinition][] = [

  ['Projects_GetProjectHealthChecks', {
    name: 'Projects_GetProjectHealthChecks',
    description: `Get the full health check history for a project, ordered newest first.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid","description":"Project ID."}},"required":["id"]},
    method: 'get',
    pathTemplate: '/api/ppm/projects/{id}/health-checks',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_GetProjectHealthCheck', {
    name: 'Projects_GetProjectHealthCheck',
    description: `Get a single project health check by ID.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid","description":"Project ID."},"healthCheckId":{"type":"string","format":"uuid"}},"required":["id","healthCheckId"]},
    method: 'get',
    pathTemplate: '/api/ppm/projects/{id}/health-checks/{healthCheckId}',
    executionParameters: [{"name":"id","in":"path"},{"name":"healthCheckId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_CreateProjectHealthCheck', {
    name: 'Projects_CreateProjectHealthCheck',
    description: `Log a new health check on a project. Creating a new check automatically expires the previously active check (only one non-expired check can exist at a time). Caller must be the project, parent portfolio, or parent program owner or manager.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid","description":"Project ID."},"requestBody":{"type":"object","properties":{"status":{"type":"string","enum":["Healthy","AtRisk","Unhealthy"],"description":"Health status."},"expiration":{"type":"string","format":"date-time","description":"ISO 8601 UTC datetime when this health check expires. Must be in the future."},"note":{"type":["string","null"],"maxLength":1024}},"required":["status","expiration"]}},"required":["id","requestBody"]},
    method: 'post',
    pathTemplate: '/api/ppm/projects/{id}/health-checks',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: 'application/json',
    securityRequirements: [{"ApiKey":[]}],
  }],

];

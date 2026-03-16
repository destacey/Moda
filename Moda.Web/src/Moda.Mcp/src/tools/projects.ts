import type { McpToolDefinition } from '../types.js';

export const definitions: [string, McpToolDefinition][] = [

  ['Projects_GetProjects', {
    name: 'Projects_GetProjects',
    description: `Get a list of projects.`,
    inputSchema: {"type":"object","properties":{"status":{"type":["array","null"],"items":{"type":"number","format":"int32"}},"portfolioId":{"type":["string","null"],"format":"uuid"}}},
    method: 'get',
    pathTemplate: '/api/ppm/projects',
    executionParameters: [{"name":"status","in":"query"},{"name":"portfolioId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_GetProject', {
    name: 'Projects_GetProject',
    description: `Get project details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: 'get',
    pathTemplate: '/api/ppm/projects/{idOrKey}',
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_GetStatuses', {
    name: 'Projects_GetStatuses',
    description: `Get a list of all project statuses.`,
    inputSchema: {"type":"object","properties":{}},
    method: 'get',
    pathTemplate: '/api/ppm/projects/statuses',
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_GetWorkItems', {
    name: 'Projects_GetWorkItems',
    description: `Get work items for a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"}},"required":["id"]},
    method: 'get',
    pathTemplate: '/api/ppm/projects/{id}/work-items',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

];

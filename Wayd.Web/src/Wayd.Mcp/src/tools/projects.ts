import type { McpToolDefinition } from '../types.js';

export const definitions: [string, McpToolDefinition][] = [

  ['Projects_GetProjects', {
    name: 'Projects_GetProjects',
    description: `Get a list of projects.`,
    inputSchema: {"type":"object","properties":{"status":{"type":["array","null"],"items":{"type":"number","format":"int32"}},"portfolioId":{"type":["string","null"],"format":"uuid"},"role":{"type":["array","null"],"items":{"type":"number","format":"int32"},"description":"Project role filter. 1=Sponsor, 2=Owner, 3=Manager, 4=Member."}}},
    method: 'get',
    pathTemplate: '/api/ppm/projects',
    executionParameters: [{"name":"status","in":"query"},{"name":"portfolioId","in":"query"},{"name":"role","in":"query"}],
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

  ['Projects_GetProjectTeam', {
    name: 'Projects_GetProjectTeam',
    description: `Get the team members for a project.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: 'get',
    pathTemplate: '/api/ppm/projects/{idOrKey}/team',
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_GetProjectPhases', {
    name: 'Projects_GetProjectPhases',
    description: `Get phases for a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"}},"required":["id"]},
    method: 'get',
    pathTemplate: '/api/ppm/projects/{id}/phases',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_GetProjectPhase', {
    name: 'Projects_GetProjectPhase',
    description: `Get project phase details.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"},"phaseId":{"type":"string","format":"uuid"}},"required":["id","phaseId"]},
    method: 'get',
    pathTemplate: '/api/ppm/projects/{id}/phases/{phaseId}',
    executionParameters: [{"name":"id","in":"path"},{"name":"phaseId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_GetProjectPlanTree', {
    name: 'Projects_GetProjectPlanTree',
    description: `Get a unified plan tree with phases as top-level nodes and tasks nested within. Returns both phase nodes and task nodes with WBS codes.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: 'get',
    pathTemplate: '/api/ppm/projects/{idOrKey}/plan-tree',
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_GetProjectPlanSummary', {
    name: 'Projects_GetProjectPlanSummary',
    description: `Get summary metrics for a project's plan, computed from leaf tasks. Includes overdue, due this week, upcoming, and total task counts.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"employeeId":{"type":["string","null"],"format":"uuid"}},"required":["idOrKey"]},
    method: 'get',
    pathTemplate: '/api/ppm/projects/{idOrKey}/plan-summary',
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"employeeId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

];

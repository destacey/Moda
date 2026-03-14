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

  ['Projects_Create', {
    name: 'Projects_Create',
    description: `Create a project.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","properties":{"name":{"type":"string","maxLength":128},"description":{"type":"string","maxLength":2048},"key":{"type":"string","description":"Unique project key: 2-20 uppercase alphanumeric characters"},"expenditureCategoryId":{"type":"number","format":"int32"},"start":{"type":["string","null"],"format":"date","description":"ISO date string (YYYY-MM-DD)"},"end":{"type":["string","null"],"format":"date","description":"ISO date string (YYYY-MM-DD)"},"portfolioId":{"type":"string","format":"uuid"},"programId":{"type":["string","null"],"format":"uuid"},"sponsorIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"ownerIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"managerIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"memberIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"strategicThemeIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}}},"required":["name","description","key","expenditureCategoryId","portfolioId"]}},"required":["requestBody"]},
    method: 'post',
    pathTemplate: '/api/ppm/projects',
    executionParameters: [],
    requestBodyContentType: 'application/json',
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_Update', {
    name: 'Projects_Update',
    description: `Update a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"},"requestBody":{"type":"object","properties":{"id":{"type":"string","format":"uuid"},"name":{"type":"string","maxLength":128},"description":{"type":"string","maxLength":2048},"expenditureCategoryId":{"type":"number","format":"int32"},"start":{"type":["string","null"],"format":"date","description":"ISO date string (YYYY-MM-DD)"},"end":{"type":["string","null"],"format":"date","description":"ISO date string (YYYY-MM-DD)"},"sponsorIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"ownerIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"managerIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"memberIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"strategicThemeIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}}},"required":["id","name","description","expenditureCategoryId"]}},"required":["id","requestBody"]},
    method: 'put',
    pathTemplate: '/api/ppm/projects/{id}',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: 'application/json',
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_ChangeProgram', {
    name: 'Projects_ChangeProgram',
    description: `Change a project's program. Set programId to null to remove the program.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"},"requestBody":{"type":"object","properties":{"programId":{"type":["string","null"],"format":"uuid"}}}},"required":["id","requestBody"]},
    method: 'put',
    pathTemplate: '/api/ppm/projects/{id}/program',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: 'application/json',
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_ChangeKey', {
    name: 'Projects_ChangeKey',
    description: `Change a project's key.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"},"requestBody":{"type":"object","properties":{"key":{"type":"string","description":"2-20 uppercase alphanumeric characters"}},"required":["key"]}},"required":["id","requestBody"]},
    method: 'put',
    pathTemplate: '/api/ppm/projects/{id}/key',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: 'application/json',
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_Approve', {
    name: 'Projects_Approve',
    description: `Approve a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"}},"required":["id"]},
    method: 'post',
    pathTemplate: '/api/ppm/projects/{id}/approve',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_Activate', {
    name: 'Projects_Activate',
    description: `Activate a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"}},"required":["id"]},
    method: 'post',
    pathTemplate: '/api/ppm/projects/{id}/activate',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Projects_Complete', {
    name: 'Projects_Complete',
    description: `Complete a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"}},"required":["id"]},
    method: 'post',
    pathTemplate: '/api/ppm/projects/{id}/complete',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

];

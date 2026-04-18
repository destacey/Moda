import type { McpToolDefinition } from '../types.js';

export const definitions: [string, McpToolDefinition][] = [

  ['Teams_GetTeams', {
    name: 'Teams_GetTeams',
    description: `Get a list of teams.`,
    inputSchema: {"type":"object","properties":{"includeInactive":{"type":"boolean"}}},
    method: 'get',
    pathTemplate: '/api/organization/teams',
    executionParameters: [{"name":"includeInactive","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

  ['Teams_GetTeam', {
    name: 'Teams_GetTeam',
    description: `Get team details.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"}},"required":["id"]},
    method: 'get',
    pathTemplate: '/api/organization/teams/{id}',
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}],
  }],

];

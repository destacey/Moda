#!/usr/bin/env node
/**
 * MCP Server generated from OpenAPI spec for moda-mcp vv1
 * Generated on: 2026-02-11T03:03:25.115Z
 */

// Load environment variables from .env file
import dotenv from 'dotenv';
dotenv.config();

import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
  type Tool,
  type CallToolResult,
  type CallToolRequest
} from "@modelcontextprotocol/sdk/types.js";

import { z, ZodError } from 'zod';
import { jsonSchemaToZod } from 'json-schema-to-zod';
import axios, { type AxiosRequestConfig, type AxiosError } from 'axios';
import { createRequire } from 'module';
import { parseArgs } from 'util';

const _require = createRequire(import.meta.url);
const { version: packageVersion } = _require('../package.json') as { version: string };

/**
 * Type definition for JSON objects
 */
type JsonObject = Record<string, any>;

/**
 * Interface for MCP Tool Definition
 */
interface McpToolDefinition {
    name: string;
    description: string;
    inputSchema: any;
    method: string;
    pathTemplate: string;
    executionParameters: { name: string, in: string }[];
    requestBodyContentType?: string;
    securityRequirements: any[];
}

/**
 * Server configuration
 */
const { values: cliArgs } = parseArgs({
  options: {
    'api-key':  { type: 'string' },
    'base-url': { type: 'string' },
  },
  strict: false, // ignore unknown args passed by MCP clients
});

export const SERVER_NAME = "moda-mcp";
export const SERVER_VERSION = packageVersion;
export const API_BASE_URL = (cliArgs['base-url'] as string | undefined) || process.env.MODA_API_BASE_URL || '';
export const MODA_API_KEY  = (cliArgs['api-key']  as string | undefined) || process.env.MODA_API_KEY  || '';
console.error(`API_BASE_URL: ${API_BASE_URL}`);

/**
 * MCP Server instance
 */
const server = new Server(
    { name: SERVER_NAME, version: SERVER_VERSION },
    { capabilities: { tools: {} } }
);

/**
 * Map of tool definitions by name
 */
const toolDefinitionMap: Map<string, McpToolDefinition> = new Map([

  // Portfolios - Read

  ["Portfolios_GetPortfolios", {
    name: "Portfolios_GetPortfolios",
    description: `Get a list of project portfolios.`,
    inputSchema: {"type":"object","properties":{"status":{"type":["array","null"],"items":{"type":"number","format":"int32"}}}},
    method: "get",
    pathTemplate: "/api/ppm/portfolios",
    executionParameters: [{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Portfolios_GetPortfolio", {
    name: "Portfolios_GetPortfolio",
    description: `Get project portfolio details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/portfolios/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Portfolios_GetPortfolioOptions", {
    name: "Portfolios_GetPortfolioOptions",
    description: `Get a lightweight list of project portfolio options (id and name) for use in lookups.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/portfolios/options",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Portfolios_GetPortfolioStatuses", {
    name: "Portfolios_GetPortfolioStatuses",
    description: `Get a list of all project portfolio statuses.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/portfolios/statuses",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Portfolios_GetPortfolioPrograms", {
    name: "Portfolios_GetPortfolioPrograms",
    description: `Get a list of programs for a portfolio. Optionally filter by status.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"status":{"type":["array","null"],"items":{"type":"number","format":"int32"}}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/portfolios/{idOrKey}/programs",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Portfolios_GetPortfolioProjects", {
    name: "Portfolios_GetPortfolioProjects",
    description: `Get a list of projects for a portfolio. Optionally filter by status.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"status":{"type":["array","null"],"items":{"type":"number","format":"int32"}}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/portfolios/{idOrKey}/projects",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  // Programs - Read

  ["Programs_GetPrograms", {
    name: "Programs_GetPrograms",
    description: `Get a list of programs. Optionally filter by status and/or portfolioId.`,
    inputSchema: {"type":"object","properties":{"status":{"type":["array","null"],"items":{"type":"number","format":"int32"}},"portfolioId":{"type":["string","null"],"format":"uuid"}}},
    method: "get",
    pathTemplate: "/api/ppm/programs",
    executionParameters: [{"name":"status","in":"query"},{"name":"portfolioId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Programs_GetProgram", {
    name: "Programs_GetProgram",
    description: `Get program details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/programs/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Programs_GetProgramStatuses", {
    name: "Programs_GetProgramStatuses",
    description: `Get a list of all program statuses.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/programs/statuses",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Programs_GetProgramProjects", {
    name: "Programs_GetProgramProjects",
    description: `Get a list of projects for a program. Optionally filter by status.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"status":{"type":["array","null"],"items":{"type":"number","format":"int32"}}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/programs/{idOrKey}/projects",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  // Projects - Read & Write

  ["Projects_GetProjects", {
    name: "Projects_GetProjects",
    description: `Get a list of projects.`,
    inputSchema: {"type":"object","properties":{"status":{"type":["array","null"],"items":{"type":"number","format":"int32"}},"portfolioId":{"type":["string","null"],"format":"uuid"}}},
    method: "get",
    pathTemplate: "/api/ppm/projects",
    executionParameters: [{"name":"status","in":"query"},{"name":"portfolioId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Projects_GetProject", {
    name: "Projects_GetProject",
    description: `Get project details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/projects/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Projects_GetStatuses", {
    name: "Projects_GetStatuses",
    description: `Get a list of all project statuses.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/projects/statuses",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Projects_GetWorkItems", {
    name: "Projects_GetWorkItems",
    description: `Get work items for a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/ppm/projects/{id}/work-items",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Projects_Create", {
    name: "Projects_Create",
    description: `Create a project.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","properties":{"name":{"type":"string","maxLength":128},"description":{"type":"string","maxLength":2048},"key":{"type":"string","description":"Unique project key: 2-20 uppercase alphanumeric characters"},"expenditureCategoryId":{"type":"number","format":"int32"},"start":{"type":["string","null"],"format":"date","description":"ISO date string (YYYY-MM-DD)"},"end":{"type":["string","null"],"format":"date","description":"ISO date string (YYYY-MM-DD)"},"portfolioId":{"type":"string","format":"uuid"},"programId":{"type":["string","null"],"format":"uuid"},"sponsorIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"ownerIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"managerIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"memberIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"strategicThemeIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}}},"required":["name","description","key","expenditureCategoryId","portfolioId"]}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/projects",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Projects_Update", {
    name: "Projects_Update",
    description: `Update a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"},"requestBody":{"type":"object","properties":{"id":{"type":"string","format":"uuid"},"name":{"type":"string","maxLength":128},"description":{"type":"string","maxLength":2048},"expenditureCategoryId":{"type":"number","format":"int32"},"start":{"type":["string","null"],"format":"date","description":"ISO date string (YYYY-MM-DD)"},"end":{"type":["string","null"],"format":"date","description":"ISO date string (YYYY-MM-DD)"},"sponsorIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"ownerIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"managerIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"memberIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}},"strategicThemeIds":{"type":["array","null"],"items":{"type":"string","format":"uuid"}}},"required":["id","name","description","expenditureCategoryId"]}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/projects/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Projects_ChangeProgram", {
    name: "Projects_ChangeProgram",
    description: `Change a project's program. Set programId to null to remove the program.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"},"requestBody":{"type":"object","properties":{"programId":{"type":["string","null"],"format":"uuid"}}}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/projects/{id}/program",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Projects_ChangeKey", {
    name: "Projects_ChangeKey",
    description: `Change a project's key.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"},"requestBody":{"type":"object","properties":{"key":{"type":"string","description":"2-20 uppercase alphanumeric characters"}},"required":["key"]}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/projects/{id}/key",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Projects_Approve", {
    name: "Projects_Approve",
    description: `Approve a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/projects/{id}/approve",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Projects_Activate", {
    name: "Projects_Activate",
    description: `Activate a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/projects/{id}/activate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Projects_Complete", {
    name: "Projects_Complete",
    description: `Complete a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"uuid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/projects/{id}/complete",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  // Roadmaps - Read

  ["Roadmaps_GetRoadmaps", {
    name: "Roadmaps_GetRoadmaps",
    description: `Get a list of roadmaps.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/roadmaps",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Roadmaps_GetRoadmap", {
    name: "Roadmaps_GetRoadmap",
    description: `Get roadmap details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/roadmaps/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Roadmaps_GetItems", {
    name: "Roadmaps_GetItems",
    description: `Get all items (activities, timeboxes, milestones) for a roadmap.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/roadmaps/{idOrKey}/items",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Roadmaps_GetActivities", {
    name: "Roadmaps_GetActivities",
    description: `Get roadmap activities.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/roadmaps/{idOrKey}/items/activities",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Roadmaps_GetItem", {
    name: "Roadmaps_GetItem",
    description: `Get roadmap item details.`,
    inputSchema: {"type":"object","properties":{"roadmapIdOrKey":{"type":"string"},"itemId":{"type":"string","format":"uuid"}},"required":["roadmapIdOrKey","itemId"]},
    method: "get",
    pathTemplate: "/api/planning/roadmaps/{roadmapIdOrKey}/items/{itemId}",
    executionParameters: [{"name":"roadmapIdOrKey","in":"path"},{"name":"itemId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["Roadmaps_GetVisibilityOptions", {
    name: "Roadmaps_GetVisibilityOptions",
    description: `Get a list of all roadmap visibility options.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/roadmaps/visibility-options",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  // Planning Intervals (PIs) - Read

  ["PlanningIntervals_GetList", {
    name: "PlanningIntervals_GetList",
    description: `Get a list of planning intervals.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetPlanningInterval", {
    name: "PlanningIntervals_GetPlanningInterval",
    description: `Get planning interval details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetCalendar", {
    name: "PlanningIntervals_GetCalendar",
    description: `Get the PI calendar.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/calendar",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetPredictability", {
    name: "PlanningIntervals_GetPredictability",
    description: `Get the PI predictability for all teams.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/predictability",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetTeams", {
    name: "PlanningIntervals_GetTeams",
    description: `Get a list of teams participating in a planning interval.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/teams",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetTeamPredictability", {
    name: "PlanningIntervals_GetTeamPredictability",
    description: `Get the PI predictability for a specific team.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"teamId":{"type":"string","format":"uuid"}},"required":["idOrKey","teamId"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/teams/{teamId}/predictability",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"teamId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetIterations", {
    name: "PlanningIntervals_GetIterations",
    description: `Get a list of iterations for a planning interval.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/iterations",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetIteration", {
    name: "PlanningIntervals_GetIteration",
    description: `Get a specific planning interval iteration.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"iterationIdOrKey":{"type":"string"}},"required":["idOrKey","iterationIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/iterations/{iterationIdOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"iterationIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetIterationCategories", {
    name: "PlanningIntervals_GetIterationCategories",
    description: `Get a list of iteration categories.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/iteration-categories",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetIterationSprints", {
    name: "PlanningIntervals_GetIterationSprints",
    description: `Get iteration sprint mappings for a Planning Interval. Optionally filter by iterationId.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"iterationId":{"type":["string","null"],"format":"uuid"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/iterations/sprints",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"iterationId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetIterationMetrics", {
    name: "PlanningIntervals_GetIterationMetrics",
    description: `Get metrics for a PI iteration aggregated across all mapped sprints.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"iterationIdOrKey":{"type":"string"}},"required":["idOrKey","iterationIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/iterations/{iterationIdOrKey}/metrics",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"iterationIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetIterationBacklog", {
    name: "PlanningIntervals_GetIterationBacklog",
    description: `Get combined backlog for a PI iteration from all mapped sprints.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"iterationIdOrKey":{"type":"string"}},"required":["idOrKey","iterationIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/iterations/{iterationIdOrKey}/backlog",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"iterationIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetObjectives", {
    name: "PlanningIntervals_GetObjectives",
    description: `Get a list of planning interval objectives. Optionally filter by teamId.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"teamId":{"type":["string","null"],"format":"uuid"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/objectives",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"teamId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetObjective", {
    name: "PlanningIntervals_GetObjective",
    description: `Get a planning interval objective.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"objectiveIdOrKey":{"type":"string"}},"required":["idOrKey","objectiveIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/objectives/{objectiveIdOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"objectiveIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetObjectiveStatuses", {
    name: "PlanningIntervals_GetObjectiveStatuses",
    description: `Get a list of all PI objective statuses.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/objective-statuses",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetObjectivesHealthReport", {
    name: "PlanningIntervals_GetObjectivesHealthReport",
    description: `Get a health report for planning interval objectives. Optionally filter by teamId.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"teamId":{"type":["string","null"],"format":"uuid"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/objectives/health-report",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"teamId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetObjectiveWorkItems", {
    name: "PlanningIntervals_GetObjectiveWorkItems",
    description: `Get work items linked to a planning interval objective.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"objectiveIdOrKey":{"type":"string"}},"required":["idOrKey","objectiveIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/objectives/{objectiveIdOrKey}/work-items",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"objectiveIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetObjectiveWorkItemMetrics", {
    name: "PlanningIntervals_GetObjectiveWorkItemMetrics",
    description: `Get daily metrics for work items linked to a planning interval objective.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"objectiveIdOrKey":{"type":"string"}},"required":["idOrKey","objectiveIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/objectives/{objectiveIdOrKey}/work-items/metrics",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"objectiveIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }],

  ["PlanningIntervals_GetRisks", {
    name: "PlanningIntervals_GetRisks",
    description: `Get planning interval risks. The default value for includeClosed is false. Optionally filter by teamId.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"includeClosed":{"type":["boolean","null"]},"teamId":{"type":["string","null"],"format":"uuid"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/risks",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"includeClosed","in":"query"},{"name":"teamId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"ApiKey":[]}]
  }]
]);

/**
 * Security schemes from the OpenAPI spec
 */
const securitySchemes =   {
    "ApiKey": {
      "type": "apiKey",
      "description": "Personal Access Token - Enter your PAT directly without any prefix",
      "name": "x-api-key",
      "in": "header"
    }
  };


server.setRequestHandler(ListToolsRequestSchema, async () => {
  const toolsForClient: Tool[] = Array.from(toolDefinitionMap.values()).map(def => ({
    name: def.name,
    description: def.description,
    inputSchema: def.inputSchema
  }));
  return { tools: toolsForClient };
});


server.setRequestHandler(CallToolRequestSchema, async (request: CallToolRequest): Promise<CallToolResult> => {
  const { name: toolName, arguments: toolArgs } = request.params;
  const toolDefinition = toolDefinitionMap.get(toolName);
  if (!toolDefinition) {
    console.error(`Error: Unknown tool requested: ${toolName}`);
    return { content: [{ type: "text", text: `Error: Unknown tool requested: ${toolName}` }] };
  }
  return await executeApiTool(toolName, toolDefinition, toolArgs ?? {}, securitySchemes);
});




/**
 * Executes an API tool with the provided arguments
 * 
 * @param toolName Name of the tool to execute
 * @param definition Tool definition
 * @param toolArgs Arguments provided by the user
 * @param allSecuritySchemes Security schemes from the OpenAPI spec
 * @returns Call tool result
 */
async function executeApiTool(
    toolName: string,
    definition: McpToolDefinition,
    toolArgs: JsonObject,
    allSecuritySchemes: Record<string, any>
): Promise<CallToolResult> {
  try {
    // Validate arguments against the input schema
    let validatedArgs: JsonObject;
    try {
        const zodSchema = getZodSchemaFromJsonSchema(definition.inputSchema, toolName);
        const argsToParse = (typeof toolArgs === 'object' && toolArgs !== null) ? toolArgs : {};
        validatedArgs = zodSchema.parse(argsToParse);
    } catch (error: unknown) {
        if (error instanceof ZodError) {
            const validationErrorMessage = `Invalid arguments for tool '${toolName}': ${error.errors.map(e => `${e.path.join('.')} (${e.code}): ${e.message}`).join(', ')}`;
            return { content: [{ type: 'text', text: validationErrorMessage }] };
        } else {
             const errorMessage = error instanceof Error ? error.message : String(error);
             return { content: [{ type: 'text', text: `Internal error during validation setup: ${errorMessage}` }] };
        }
    }

    // Prepare URL, query parameters, headers, and request body
    let urlPath = definition.pathTemplate;
    const queryParams: Record<string, any> = {};
    const headers: Record<string, string> = { 'Accept': 'application/json' };
    let requestBodyData: any = undefined;

    // Apply parameters to the URL path, query, or headers
    definition.executionParameters.forEach((param) => {
        const value = validatedArgs[param.name];
        if (typeof value !== 'undefined' && value !== null) {
            if (param.in === 'path') {
                urlPath = urlPath.replace(`{${param.name}}`, encodeURIComponent(String(value)));
            }
            else if (param.in === 'query') {
                queryParams[param.name] = value;
            }
            else if (param.in === 'header') {
                headers[param.name.toLowerCase()] = String(value);
            }
        }
    });

    // Ensure all path parameters are resolved
    if (urlPath.includes('{')) {
        throw new Error(`Failed to resolve path parameters: ${urlPath}`);
    }
    
    // Construct the full URL
    const requestUrl = API_BASE_URL ? `${API_BASE_URL}${urlPath}` : urlPath;

    // Handle request body if needed
    if (definition.requestBodyContentType && typeof validatedArgs['requestBody'] !== 'undefined') {
        requestBodyData = validatedArgs['requestBody'];
        headers['content-type'] = definition.requestBodyContentType;
    }


    // Apply API key security if available
    for (const req of (definition.securityRequirements ?? [])) {
        for (const [schemeName] of Object.entries(req)) {
            const scheme = allSecuritySchemes[schemeName];
            if (scheme?.type !== 'apiKey') continue;

            const apiKey = MODA_API_KEY;
            if (!apiKey) continue;

            if (scheme.in === 'header') {
                headers[scheme.name.toLowerCase()] = apiKey;
                console.error(`Applied API key '${schemeName}' in header '${scheme.name}'`);
            } else if (scheme.in === 'query') {
                queryParams[scheme.name] = apiKey;
                console.error(`Applied API key '${schemeName}' in query parameter '${scheme.name}'`);
            } else if (scheme.in === 'cookie') {
                headers['cookie'] = `${scheme.name}=${apiKey}${headers['cookie'] ? `; ${headers['cookie']}` : ''}`;
                console.error(`Applied API key '${schemeName}' in cookie '${scheme.name}'`);
            }
        }
    }
    

    // Prepare the axios request configuration
    const config: AxiosRequestConfig = {
      method: definition.method.toUpperCase(), 
      url: requestUrl, 
      params: queryParams, 
      headers: headers,
      ...(requestBodyData !== undefined && { data: requestBodyData }),
    };

    // Log request info to stderr (doesn't affect MCP output)
    console.error(`Executing tool "${toolName}": ${config.method} ${config.url}`);
    
    // Execute the request
    const response = await axios(config);

    // Process and format the response
    let responseText = '';
    const contentType = response.headers['content-type']?.toLowerCase() || '';
    
    // Handle JSON responses
    if (contentType.includes('application/json') && typeof response.data === 'object' && response.data !== null) {
         try { 
             responseText = JSON.stringify(response.data, null, 2); 
         } catch (e) { 
             responseText = "[Stringify Error]"; 
         }
    } 
    // Handle string responses
    else if (typeof response.data === 'string') { 
         responseText = response.data; 
    }
    // Handle other response types
    else if (response.data !== undefined && response.data !== null) { 
         responseText = String(response.data); 
    }
    // Handle empty responses
    else { 
         responseText = `(Status: ${response.status} - No body content)`; 
    }
    
    // Return formatted response
    return { 
        content: [ 
            { 
                type: "text", 
                text: `API Response (Status: ${response.status}):\n${responseText}` 
            } 
        ], 
    };

  } catch (error: unknown) {
    // Handle errors during execution
    let errorMessage: string;
    
    // Format Axios errors specially
    if (axios.isAxiosError(error)) { 
        errorMessage = formatApiError(error); 
    }
    // Handle standard errors
    else if (error instanceof Error) { 
        errorMessage = error.message; 
    }
    // Handle unexpected error types
    else { 
        errorMessage = 'Unexpected error: ' + String(error); 
    }
    
    // Log error to stderr
    console.error(`Error during execution of tool '${toolName}':`, errorMessage);
    
    // Return error message to client
    return { content: [{ type: "text", text: errorMessage }] };
  }
}


/**
 * Main function to start the server
 */
async function main() {
// Set up stdio transport
  try {
    const transport = new StdioServerTransport();
    await server.connect(transport);
    console.error(`${SERVER_NAME} MCP Server (v${SERVER_VERSION}) running on stdio${API_BASE_URL ? `, proxying API at ${API_BASE_URL}` : ''}`);
  } catch (error) {
    console.error("Error during server startup:", error);
    process.exit(1);
  }
}

/**
 * Cleanup function for graceful shutdown
 */
async function cleanup() {
    console.error("Shutting down MCP server...");
    process.exit(0);
}

// Register signal handlers
process.on('SIGINT', cleanup);
process.on('SIGTERM', cleanup);

// Start the server
main().catch((error) => {
  console.error("Fatal error in main execution:", error);
  process.exit(1);
});

/**
 * Formats API errors for better readability
 * 
 * @param error Axios error
 * @returns Formatted error message
 */
function formatApiError(error: AxiosError): string {
    let message = 'API request failed.';
    if (error.response) {
        message = `API Error: Status ${error.response.status} (${error.response.statusText || 'Status text not available'}). `;
        const responseData = error.response.data;
        const MAX_LEN = 200;
        if (typeof responseData === 'string') { 
            message += `Response: ${responseData.substring(0, MAX_LEN)}${responseData.length > MAX_LEN ? '...' : ''}`; 
        }
        else if (responseData) { 
            try { 
                const jsonString = JSON.stringify(responseData); 
                message += `Response: ${jsonString.substring(0, MAX_LEN)}${jsonString.length > MAX_LEN ? '...' : ''}`; 
            } catch { 
                message += 'Response: [Could not serialize data]'; 
            } 
        }
        else { 
            message += 'No response body received.'; 
        }
    } else if (error.request) {
        message = 'API Network Error: No response received from server.';
        if (error.code) message += ` (Code: ${error.code})`;
    } else { 
        message += `API Request Setup Error: ${error.message}`; 
    }
    return message;
}

/**
 * Converts a JSON Schema to a Zod schema for runtime validation
 * 
 * @param jsonSchema JSON Schema
 * @param toolName Tool name for error reporting
 * @returns Zod schema
 */
function getZodSchemaFromJsonSchema(jsonSchema: any, toolName: string): z.ZodTypeAny {
    if (typeof jsonSchema !== 'object' || jsonSchema === null) { 
        return z.object({}).passthrough(); 
    }
    try {
        const zodSchemaString = jsonSchemaToZod(jsonSchema);
        const zodSchema = eval(zodSchemaString);
        if (typeof zodSchema?.parse !== 'function') { 
            throw new Error('Eval did not produce a valid Zod schema.'); 
        }
        return zodSchema as z.ZodTypeAny;
    } catch (err: any) {
        console.error(`Failed to generate/evaluate Zod schema for '${toolName}':`, err);
        return z.object({}).passthrough();
    }
}

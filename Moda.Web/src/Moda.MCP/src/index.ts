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
export const SERVER_NAME = "moda-mcp";
export const SERVER_VERSION = "v1";
export const API_BASE_URL = process.env.API_BASE_URL || "";
console.error("API_BASE_URL is set to:", API_BASE_URL);

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

  ["GetStartup", {
    name: "GetStartup",
    description: `Executes GET /startup`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/startup",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Permissions_GetList", {
    name: "Permissions_GetList",
    description: `Get a list of all permissions.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/user-management/permissions",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PersonalAccessTokens_GetMyTokens", {
    name: "PersonalAccessTokens_GetMyTokens",
    description: `Get all personal access tokens for the current user.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/user-management/personal-access-tokens",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PersonalAccessTokens_Create", {
    name: "PersonalAccessTokens_Create",
    description: `Returns the plaintext token - this is the ONLY time it will be visible!`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","expiresAt"],"properties":{"name":{"type":"string","description":"The user-friendly name for this token.","maxLength":100,"minLength":1},"expiresAt":{"type":"string","description":"The expiration date for the token. Must be in the future and within 2 years from now.","format":"date-time"}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/user-management/personal-access-tokens",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PersonalAccessTokens_GetById", {
    name: "PersonalAccessTokens_GetById",
    description: `Get a personal access token by ID.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/user-management/personal-access-tokens/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PersonalAccessTokens_Update", {
    name: "PersonalAccessTokens_Update",
    description: `Update a personal access token's name and/or expiration date.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["name","expiresAt"],"properties":{"name":{"type":"string","description":"The user-friendly name for this token.","maxLength":100,"minLength":1},"expiresAt":{"type":"string","description":"The expiration date for the token. Must be in the future and within 2 years from now.","format":"date-time"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/user-management/personal-access-tokens/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PersonalAccessTokens_Delete", {
    name: "PersonalAccessTokens_Delete",
    description: `This removes the token from the database entirely.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/user-management/personal-access-tokens/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PersonalAccessTokens_Revoke", {
    name: "PersonalAccessTokens_Revoke",
    description: `Revoked tokens are kept for audit purposes.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "put",
    pathTemplate: "/api/user-management/personal-access-tokens/{id}/revoke",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Profile_Get", {
    name: "Profile_Get",
    description: `Get profile details of currently logged in user.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/user-management/profiles",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Profile_Update", {
    name: "Profile_Update",
    description: `Update profile details of currently logged in user.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["id","firstName","lastName","email"],"properties":{"id":{"type":"string","minLength":1},"firstName":{"type":"string","maxLength":100,"minLength":1},"lastName":{"type":"string","maxLength":100,"minLength":1},"email":{"type":"string","minLength":1,"pattern":"^[^@]+@[^@]+$"},"phoneNumber":{"type":["string","null"]}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "put",
    pathTemplate: "/api/user-management/profiles",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Profile_GetPermissions", {
    name: "Profile_GetPermissions",
    description: `Get permissions of currently logged in user.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/user-management/profiles/permissions",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Profile_GetInternalEmployeeId", {
    name: "Profile_GetInternalEmployeeId",
    description: `Get internal employee id of currently logged in user.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/user-management/profiles/internal-employee-id",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Profile_GetLogs", {
    name: "Profile_GetLogs",
    description: `Get audit logs of currently logged in user.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/user-management/profiles/logs",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roles_GetList", {
    name: "Roles_GetList",
    description: `Get a list of all roles.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/user-management/roles",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roles_CreateOrUpdate", {
    name: "Roles_CreateOrUpdate",
    description: `Create or update a role.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name"],"properties":{"id":{"type":["string","null"]},"name":{"type":"string","maxLength":256,"minLength":1},"description":{"type":["string","null"]}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/user-management/roles",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roles_GetById", {
    name: "Roles_GetById",
    description: `Get role details.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/user-management/roles/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roles_Delete", {
    name: "Roles_Delete",
    description: `Delete a role.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/user-management/roles/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roles_GetByIdWithPermissions", {
    name: "Roles_GetByIdWithPermissions",
    description: `Get role details with its permissions.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/user-management/roles/{id}/permissions",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roles_UpdatePermissions", {
    name: "Roles_UpdatePermissions",
    description: `Update a role's permissions.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"},"requestBody":{"type":"object","additionalProperties":false,"required":["roleId","permissions"],"properties":{"roleId":{"type":"string","minLength":1},"permissions":{"type":"array","items":{"type":"string"}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/user-management/roles/{id}/permissions",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roles_GetUsers", {
    name: "Roles_GetUsers",
    description: `Get list of all users with this role.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/user-management/roles/{id}/users",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roles_GetUsersCount", {
    name: "Roles_GetUsersCount",
    description: `Get a count of all users with this role.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/user-management/roles/{id}/users-count",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Users_GetUsers", {
    name: "Users_GetUsers",
    description: `Get list of all users.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/user-management/users",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Users_GetUser", {
    name: "Users_GetUser",
    description: `Get a user's details.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/user-management/users/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Users_GetRoles", {
    name: "Users_GetRoles",
    description: `Get a user's roles.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"},"includeUnassigned":{"type":"boolean","default":false}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/user-management/users/{id}/roles",
    executionParameters: [{"name":"id","in":"path"},{"name":"includeUnassigned","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Users_ManageUserRoles", {
    name: "Users_ManageUserRoles",
    description: `Update a user's assigned roles.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"},"requestBody":{"type":"object","additionalProperties":false,"required":["userId","roleNames"],"properties":{"userId":{"type":"string"},"roleNames":{"type":"array","items":{"type":"string"}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/user-management/users/{id}/roles",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Users_ManageRoleUsers", {
    name: "Users_ManageRoleUsers",
    description: `Add or remove users from a role.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["roleId","userIdsToAdd","userIdsToRemove"],"properties":{"roleId":{"type":"string","minLength":1},"userIdsToAdd":{"type":"array","items":{"type":"string"}},"userIdsToRemove":{"type":"array","items":{"type":"string"}}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/user-management/users/manage-role",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Users_ToggleStatus", {
    name: "Users_ToggleStatus",
    description: `Toggle a user's active status.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"},"requestBody":{"type":"object","additionalProperties":false,"required":["userId","activateUser"],"properties":{"userId":{"type":"string"},"activateUser":{"type":"boolean"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/user-management/users/{id}/toggle-status",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicThemes_GetStrategicThemes", {
    name: "StrategicThemes_GetStrategicThemes",
    description: `Get a list of strategic themes.`,
    inputSchema: {"type":"object","properties":{"state":{"type":["number","null"],"format":"int32"}}},
    method: "get",
    pathTemplate: "/api/strategic-management/strategic-themes",
    executionParameters: [{"name":"state","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicThemes_Create", {
    name: "StrategicThemes_Create",
    description: `Create a strategic theme.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","description"],"properties":{"name":{"type":"string","description":"The name of the strategic theme, highlighting its focus or priority.","maxLength":64,"minLength":1},"description":{"type":"string","description":"A detailed description of the strategic theme and its importance.","maxLength":1024,"minLength":1}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/strategic-management/strategic-themes",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicThemes_GetStrategicTheme", {
    name: "StrategicThemes_GetStrategicTheme",
    description: `Get strategic themes details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/strategic-management/strategic-themes/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicThemes_Update", {
    name: "StrategicThemes_Update",
    description: `Update a strategic theme.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","description"],"properties":{"id":{"type":"string","description":"The unique identifier of the strategic theme.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the strategic theme, highlighting its focus or priority.","maxLength":64,"minLength":1},"description":{"type":"string","description":"A detailed description of the strategic theme and its importance.","maxLength":1024,"minLength":1}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/strategic-management/strategic-themes/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicThemes_Delete", {
    name: "StrategicThemes_Delete",
    description: `Delete a strategic theme.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/strategic-management/strategic-themes/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicThemes_Activate", {
    name: "StrategicThemes_Activate",
    description: `Activate a strategic theme.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/strategic-management/strategic-themes/{id}/activate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicThemes_Archive", {
    name: "StrategicThemes_Archive",
    description: `Archive a strategic theme.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/strategic-management/strategic-themes/{id}/archive",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicThemes_GetStrategicThemeOptions", {
    name: "StrategicThemes_GetStrategicThemeOptions",
    description: `Get a list of strategic theme options.`,
    inputSchema: {"type":"object","properties":{"includeArchived":{"type":["boolean","null"]}}},
    method: "get",
    pathTemplate: "/api/strategic-management/strategic-themes/options",
    executionParameters: [{"name":"includeArchived","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicThemes_GetStateOptions", {
    name: "StrategicThemes_GetStateOptions",
    description: `Get a list of all strategic theme states.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/strategic-management/strategic-themes/states",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Strategies_GetStrategies", {
    name: "Strategies_GetStrategies",
    description: `Get a list of strategies.`,
    inputSchema: {"type":"object","properties":{"status":{"type":["number","null"],"format":"int32"}}},
    method: "get",
    pathTemplate: "/api/strategic-management/strategies",
    executionParameters: [{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Strategies_Create", {
    name: "Strategies_Create",
    description: `Create a strategy.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","description","statusId"],"properties":{"name":{"type":"string","description":"The concise statement describing the strategy and its purpose or focus area.","maxLength":1024,"minLength":1},"description":{"type":"string","description":"A concise statement describing the strategy of the organization.","maxLength":3072,"minLength":0},"statusId":{"type":"number","description":"The current status id of the strategy.","format":"int32"},"start":{"type":["string","null"],"description":"The start date of when the strategy became active.","format":"date"},"end":{"type":["string","null"],"description":"The end date of when the strategy became archived.","format":"date"}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/strategic-management/strategies",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Strategies_GetStrategy", {
    name: "Strategies_GetStrategy",
    description: `Get strategy details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/strategic-management/strategies/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Strategies_Update", {
    name: "Strategies_Update",
    description: `Update a strategy.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","description","statusId"],"properties":{"id":{"type":"string","description":"The unique identifier of the strategy.","format":"guid","minLength":1},"name":{"type":"string","description":"The concise statement describing the strategy and its purpose or focus area.","maxLength":1024,"minLength":1},"description":{"type":"string","description":"A concise statement describing the strategy of the organization.","maxLength":3072,"minLength":0},"statusId":{"type":"number","description":"The current status id of the strategy.","format":"int32"},"start":{"type":["string","null"],"description":"The start date of when the strategy became active.","format":"date"},"end":{"type":["string","null"],"description":"The end date of when the strategy became archived.","format":"date"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/strategic-management/strategies/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Strategies_Delete", {
    name: "Strategies_Delete",
    description: `Delete a strategy.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/strategic-management/strategies/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Strategies_GetStatusOptions", {
    name: "Strategies_GetStatusOptions",
    description: `Get a list of all strategy statuses.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/strategic-management/strategies/statuses",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Visions_GetVisions", {
    name: "Visions_GetVisions",
    description: `Get a list of visions.`,
    inputSchema: {"type":"object","properties":{"state":{"type":["number","null"],"format":"int32"}}},
    method: "get",
    pathTemplate: "/api/strategic-management/visions",
    executionParameters: [{"name":"state","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Visions_Create", {
    name: "Visions_Create",
    description: `Create a vision.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["description"],"properties":{"description":{"type":"string","description":"A concise statement describing the vision of the organization.","maxLength":3072,"minLength":1}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/strategic-management/visions",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Visions_GetVision", {
    name: "Visions_GetVision",
    description: `Get vision details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/strategic-management/visions/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Visions_Update", {
    name: "Visions_Update",
    description: `Update a vision.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","description"],"properties":{"id":{"type":"string","description":"The unique identifier of the vision.","format":"guid","minLength":1},"description":{"type":"string","description":"A concise statement describing the vision of the organization.","maxLength":3072,"minLength":1}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/strategic-management/visions/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Visions_Delete", {
    name: "Visions_Delete",
    description: `Delete a vision.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/strategic-management/visions/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Visions_Activate", {
    name: "Visions_Activate",
    description: `Activate a vision.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/strategic-management/visions/{id}/activate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Visions_Archive", {
    name: "Visions_Archive",
    description: `Archive a vision.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/strategic-management/visions/{id}/archive",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Visions_GetStateOptions", {
    name: "Visions_GetStateOptions",
    description: `Get a list of all vision states.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/strategic-management/visions/states",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ExpenditureCategories_GetExpenditureCategories", {
    name: "ExpenditureCategories_GetExpenditureCategories",
    description: `Get a list of expenditure categories.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/expenditure-categories",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ExpenditureCategories_Create", {
    name: "ExpenditureCategories_Create",
    description: `Create an expenditure category.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","description","isCapitalizable","requiresDepreciation"],"properties":{"name":{"type":"string","description":"The name of the expenditure category (e.g., \"Opex\", \"Capex\", \"Hybrid\", etc.).","maxLength":64,"minLength":1},"description":{"type":"string","description":"Detailed description of what qualifies under this expenditure category.","maxLength":1024,"minLength":1},"isCapitalizable":{"type":"boolean","description":"Defines whether the expenditure is treated as capitalizable."},"requiresDepreciation":{"type":"boolean","description":"Defines whether the expenditure requires asset depreciation tracking."},"accountingCode":{"type":["string","null"],"description":"Reporting codes or financial classifications for the expenditure category.","maxLength":64,"minLength":0}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/expenditure-categories",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ExpenditureCategories_GetExpenditureCategory", {
    name: "ExpenditureCategories_GetExpenditureCategory",
    description: `Get expenditure category details.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/ppm/expenditure-categories/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ExpenditureCategories_Update", {
    name: "ExpenditureCategories_Update",
    description: `Update an expenditure category.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","description","isCapitalizable","requiresDepreciation"],"properties":{"id":{"type":"number","format":"int32"},"name":{"type":"string","description":"The name of the expenditure category (e.g., \"Opex\", \"Capex\", \"Hybrid\", etc.).","maxLength":64,"minLength":1},"description":{"type":"string","description":"Detailed description of what qualifies under this expenditure category.","maxLength":1024,"minLength":1},"isCapitalizable":{"type":"boolean","description":"Defines whether the expenditure is treated as capitalizable."},"requiresDepreciation":{"type":"boolean","description":"Defines whether the expenditure requires asset depreciation tracking."},"accountingCode":{"type":["string","null"],"description":"Reporting codes or financial classifications for the expenditure category.","maxLength":64,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/expenditure-categories/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ExpenditureCategories_Delete", {
    name: "ExpenditureCategories_Delete",
    description: `Delete an expenditure category.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/ppm/expenditure-categories/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ExpenditureCategories_Activate", {
    name: "ExpenditureCategories_Activate",
    description: `Activate an expenditure category.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/expenditure-categories/{id}/activate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ExpenditureCategories_Archive", {
    name: "ExpenditureCategories_Archive",
    description: `Archive an expenditure category.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/expenditure-categories/{id}/archive",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ExpenditureCategories_GetExpenditureCategoryOptions", {
    name: "ExpenditureCategories_GetExpenditureCategoryOptions",
    description: `Get a list of expenditure categories options.`,
    inputSchema: {"type":"object","properties":{"includeArchived":{"type":["boolean","null"]}}},
    method: "get",
    pathTemplate: "/api/ppm/expenditure-categories/options",
    executionParameters: [{"name":"includeArchived","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_GetPortfolios", {
    name: "Portfolios_GetPortfolios",
    description: `Get a list of project portfolios.`,
    inputSchema: {"type":"object","properties":{"status":{"type":["number","null"],"format":"int32"}}},
    method: "get",
    pathTemplate: "/api/ppm/portfolios",
    executionParameters: [{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_Create", {
    name: "Portfolios_Create",
    description: `Create a portfolio.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","description"],"properties":{"name":{"type":"string","description":"The name of the portfolio.","maxLength":128,"minLength":1},"description":{"type":"string","description":"A detailed description of the portfolio’s purpose.","maxLength":1024,"minLength":1},"sponsorIds":{"type":["array","null"],"description":"The sponsors of the portfolio.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"ownerIds":{"type":["array","null"],"description":"The owners of the portfolio.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"managerIds":{"type":["array","null"],"description":"The managers of the portfolio.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/portfolios",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_GetPortfolio", {
    name: "Portfolios_GetPortfolio",
    description: `Get project portfolio details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/portfolios/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_Update", {
    name: "Portfolios_Update",
    description: `Update a portfolio.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","description"],"properties":{"id":{"type":"string","description":"The unique identifier of the portfolio.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the portfolio.","maxLength":128,"minLength":1},"description":{"type":"string","description":"A detailed description of the portfolio’s purpose.","maxLength":1024,"minLength":1},"sponsorIds":{"type":["array","null"],"description":"The sponsors of the portfolio.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"ownerIds":{"type":["array","null"],"description":"The owners of the portfolio.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"managerIds":{"type":["array","null"],"description":"The managers of the portfolio.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/portfolios/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_Delete", {
    name: "Portfolios_Delete",
    description: `Delete a portfolio.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/ppm/portfolios/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_Activate", {
    name: "Portfolios_Activate",
    description: `Activate a project portfolio.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/portfolios/{id}/activate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_Close", {
    name: "Portfolios_Close",
    description: `Close a project portfolio.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/portfolios/{id}/close",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_Archive", {
    name: "Portfolios_Archive",
    description: `Archive a project portfolio.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/portfolios/{id}/archive",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_GetPrograms", {
    name: "Portfolios_GetPrograms",
    description: `Get a list of programs for the portfolio.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"status":{"type":["number","null"],"format":"int32"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/portfolios/{idOrKey}/programs",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_GetProjects", {
    name: "Portfolios_GetProjects",
    description: `Get a list of projects for the portfolio.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/portfolios/{idOrKey}/projects",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_GetStrategicInitiatives", {
    name: "Portfolios_GetStrategicInitiatives",
    description: `Get a list of strategic initiatives for the portfolio.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"status":{"type":["number","null"],"format":"int32"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/portfolios/{idOrKey}/strategic-initiatives",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Portfolios_GetPortfolioOptions", {
    name: "Portfolios_GetPortfolioOptions",
    description: `Get a list of project portfolio options.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/portfolios/options",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Programs_GetPrograms", {
    name: "Programs_GetPrograms",
    description: `Get a list of programs.`,
    inputSchema: {"type":"object","properties":{"status":{"type":["number","null"],"format":"int32"}}},
    method: "get",
    pathTemplate: "/api/ppm/programs",
    executionParameters: [{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Programs_Create", {
    name: "Programs_Create",
    description: `Create a program.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","description","portfolioId"],"properties":{"name":{"type":"string","description":"The name of the program.","maxLength":128,"minLength":1},"description":{"type":"string","description":"A detailed description of the program's purpose.","maxLength":2048,"minLength":1},"start":{"type":["string","null"],"description":"The Program start date.","format":"date"},"end":{"type":["string","null"],"description":"The Program end date.","format":"date"},"portfolioId":{"type":"string","description":"The ID of the portfolio to which this program belongs.","format":"guid","minLength":1},"sponsorIds":{"type":["array","null"],"description":"The sponsors of the program.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"ownerIds":{"type":["array","null"],"description":"The owners of the program.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"managerIds":{"type":["array","null"],"description":"The managers of the program.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"strategicThemeIds":{"type":["array","null"],"description":"The strategic themes associated with this program.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/programs",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Programs_GetProgram", {
    name: "Programs_GetProgram",
    description: `Get program details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/programs/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Programs_Update", {
    name: "Programs_Update",
    description: `Update a program.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","description"],"properties":{"id":{"type":"string","description":"The unique identifier of the program.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the program.","maxLength":128,"minLength":1},"description":{"type":"string","description":"A detailed description of the program's purpose.","maxLength":2048,"minLength":1},"start":{"type":["string","null"],"description":"The Program start date.","format":"date"},"end":{"type":["string","null"],"description":"The Program end date.","format":"date"},"sponsorIds":{"type":["array","null"],"description":"The sponsors of the program.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"ownerIds":{"type":["array","null"],"description":"The owners of the program.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"managerIds":{"type":["array","null"],"description":"The managers of the program.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"strategicThemeIds":{"type":["array","null"],"description":"The strategic themes associated with this program.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/programs/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Programs_Delete", {
    name: "Programs_Delete",
    description: `Delete a program.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/ppm/programs/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Programs_Activate", {
    name: "Programs_Activate",
    description: `Activate a program.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/programs/{id}/activate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Programs_Complete", {
    name: "Programs_Complete",
    description: `Complete a program.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/programs/{id}/complete",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Programs_Cancel", {
    name: "Programs_Cancel",
    description: `Cancel a program.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/programs/{id}/cancel",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Programs_GetProjects", {
    name: "Programs_GetProjects",
    description: `Get a list of projects.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"status":{"type":["number","null"],"format":"int32"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/programs/{idOrKey}/projects",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_GetProjects", {
    name: "Projects_GetProjects",
    description: `Get a list of projects.`,
    inputSchema: {"type":"object","properties":{"status":{"type":["number","null"],"format":"int32"}}},
    method: "get",
    pathTemplate: "/api/ppm/projects",
    executionParameters: [{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_Create", {
    name: "Projects_Create",
    description: `Create a project.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","description","key","expenditureCategoryId","portfolioId"],"properties":{"name":{"type":"string","description":"The name of the project.","maxLength":128,"minLength":1},"description":{"type":"string","description":"A detailed description of the project's purpose.","maxLength":2048,"minLength":1},"key":{"type":"string","description":"The unique key for the project (2-20 uppercase alphanumeric characters).","pattern":"^([A-Z0-9]){2,20}$"},"expenditureCategoryId":{"type":"number","description":"The ID of the expenditure category associated with the project.","format":"int32","minimum":0,"exclusiveMinimum":true},"start":{"type":["string","null"],"description":"The Project start date.","format":"date"},"end":{"type":["string","null"],"description":"The Project end date.","format":"date"},"portfolioId":{"type":"string","description":"The ID of the portfolio to which this project belongs.","format":"guid","minLength":1},"programId":{"type":["string","null"],"description":"The ID of the program to which this project belongs (optional).","format":"guid"},"sponsorIds":{"type":["array","null"],"description":"The sponsors of the project.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"ownerIds":{"type":["array","null"],"description":"The owners of the project.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"managerIds":{"type":["array","null"],"description":"The managers of the project.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"strategicThemeIds":{"type":["array","null"],"description":"The strategic themes associated with this project.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/projects",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_GetProject", {
    name: "Projects_GetProject",
    description: `Get project details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/projects/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_Update", {
    name: "Projects_Update",
    description: `Update a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","description","expenditureCategoryId"],"properties":{"id":{"type":"string","description":"The unique identifier of the project.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the project.","maxLength":128,"minLength":1},"description":{"type":"string","description":"A detailed description of the project’s purpose.","maxLength":2048,"minLength":1},"expenditureCategoryId":{"type":"number","description":"The ID of the expenditure category associated with the project.","format":"int32","minimum":0,"exclusiveMinimum":true},"start":{"type":["string","null"],"description":"The Project start date.","format":"date"},"end":{"type":["string","null"],"description":"The Project end date.","format":"date"},"sponsorIds":{"type":["array","null"],"description":"The sponsors of the project.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"ownerIds":{"type":["array","null"],"description":"The owners of the project.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"managerIds":{"type":["array","null"],"description":"The managers of the project.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"strategicThemeIds":{"type":["array","null"],"description":"The strategic themes associated with this project.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/projects/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_Delete", {
    name: "Projects_Delete",
    description: `Delete a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/ppm/projects/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_ChangeProgram", {
    name: "Projects_ChangeProgram",
    description: `Change a project's program.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"properties":{"programId":{"type":["string","null"],"description":"The new ProgramId to assign to the Project. If null the Program will be removed.","format":"guid"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/projects/{id}/program",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_ChangeKey", {
    name: "Projects_ChangeKey",
    description: `Change a project's key.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["key"],"properties":{"key":{"type":"string","description":"The new key to assign to the Project (2-20 uppercase alphanumeric characters).","pattern":"^([A-Z0-9]){2,20}$"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/projects/{id}/key",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_Activate", {
    name: "Projects_Activate",
    description: `Activate a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/projects/{id}/activate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_Complete", {
    name: "Projects_Complete",
    description: `Complete a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/projects/{id}/complete",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_Cancel", {
    name: "Projects_Cancel",
    description: `Cancel a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/projects/{id}/cancel",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Projects_GetProjectWorkItems", {
    name: "Projects_GetProjectWorkItems",
    description: `Get work items for a project.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/ppm/projects/{id}/work-items",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_GetProjectTasks", {
    name: "ProjectTasks_GetProjectTasks",
    description: `Get a list of project tasks.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"},"status":{"type":["number","null"],"format":"int32"},"parentId":{"type":["string","null"],"format":"guid"}},"required":["projectIdOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks",
    executionParameters: [{"name":"projectIdOrKey","in":"path"},{"name":"status","in":"query"},{"name":"parentId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_CreateProjectTask", {
    name: "ProjectTasks_CreateProjectTask",
    description: `Create a project task.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"},"requestBody":{"type":"object","additionalProperties":false,"required":["name","typeId","statusId","priorityId","progress","plannedDate"],"properties":{"name":{"type":"string","description":"The name of the task.","maxLength":128,"minLength":1},"description":{"type":["string","null"],"description":"A detailed description of the task (optional).","maxLength":2048,"minLength":0},"typeId":{"type":"number","description":"The type of task (Task or Milestone).","format":"int32"},"statusId":{"type":"number","description":"The current status of the task.","format":"int32"},"priorityId":{"type":"number","description":"The priority level of the task.","format":"int32"},"assigneeIds":{"type":["array","null"],"description":"The assignees of the project task.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"progress":{"type":"number","description":"The progress of the task (optional). Ranges from 0.0 to 100.0. Milestones can not update progress directly.","format":"decimal"},"parentId":{"type":["string","null"],"description":"The ID of the parent task (optional).","format":"guid"},"plannedStart":{"type":["string","null"],"description":"The planned start date for the task (for tasks, not milestones).","format":"date"},"plannedEnd":{"type":["string","null"],"description":"The planned end date for the task (for tasks, not milestones).","format":"date"},"plannedDate":{"type":"string","description":"The planned date for a milestone (for milestones only).","format":"date"},"estimatedEffortHours":{"type":["number","null"],"description":"The estimated effort in hours (optional).","format":"decimal","minimum":0,"exclusiveMinimum":true}},"description":"The JSON request body."}},"required":["projectIdOrKey","requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks",
    executionParameters: [{"name":"projectIdOrKey","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_GetProjectTaskTree", {
    name: "ProjectTasks_GetProjectTaskTree",
    description: `Get a hierarchical tree of project tasks with WBS codes.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"}},"required":["projectIdOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks/tree",
    executionParameters: [{"name":"projectIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_GetProjectTask", {
    name: "ProjectTasks_GetProjectTask",
    description: `Get project task details.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"},"taskIdOrKey":{"type":"string"}},"required":["projectIdOrKey","taskIdOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks/{taskIdOrKey}",
    executionParameters: [{"name":"projectIdOrKey","in":"path"},{"name":"taskIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_UpdateProjectTask", {
    name: "ProjectTasks_UpdateProjectTask",
    description: `Update a project task.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"},"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","statusId","priorityId"],"properties":{"id":{"type":"string","description":"The ID of the task.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the task.","maxLength":128,"minLength":1},"description":{"type":["string","null"],"description":"A detailed description of the task (optional).","maxLength":2048,"minLength":0},"statusId":{"type":"number","description":"The current status of the task.","format":"int32"},"priorityId":{"type":"number","description":"The priority level of the task.","format":"int32"},"assigneeIds":{"type":["array","null"],"description":"The assignees of the project task.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"progress":{"type":["number","null"],"description":"The progress of the task (optional). Ranges from 0.0 to 100.0. Milestones can not update progress directly.","format":"decimal"},"parentId":{"type":["string","null"],"description":"The ID of the parent task (optional).","format":"guid"},"plannedStart":{"type":["string","null"],"description":"The planned start date for the task.","format":"date"},"plannedEnd":{"type":["string","null"],"description":"The planned end date for the task.","format":"date"},"plannedDate":{"type":["string","null"],"description":"The planned date for a milestone.","format":"date"},"estimatedEffortHours":{"type":["number","null"],"description":"The estimated effort in hours (optional).","format":"decimal","minimum":0,"exclusiveMinimum":true}},"description":"The JSON request body."}},"required":["projectIdOrKey","id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks/{id}",
    executionParameters: [{"name":"projectIdOrKey","in":"path"},{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_DeleteProjectTask", {
    name: "ProjectTasks_DeleteProjectTask",
    description: `Delete a project task.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"},"id":{"type":"string","format":"guid"}},"required":["projectIdOrKey","id"]},
    method: "delete",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks/{id}",
    executionParameters: [{"name":"projectIdOrKey","in":"path"},{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_PatchProjectTask", {
    name: "ProjectTasks_PatchProjectTask",
    description: `Applies a JSON Patch document to update specific fields.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"},"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["operations"],"properties":{"operations":{"type":["array","null"],"items":{"allOf":[{"allOf":[{"type":"object","additionalProperties":false,"required":["path","op","from"],"properties":{"path":{"type":"string","nullable":true},"op":{"type":"string","nullable":true},"from":{"type":"string","nullable":true}}},{"type":"object","additionalProperties":false,"properties":{"value":{"nullable":true}}}]},{"type":"object","additionalProperties":false}]}}},"description":"The JSON request body."}},"required":["projectIdOrKey","id","requestBody"]},
    method: "patch",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks/{id}",
    executionParameters: [{"name":"projectIdOrKey","in":"path"},{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_UpdateProjectTaskPlacement", {
    name: "ProjectTasks_UpdateProjectTaskPlacement",
    description: `Update a project task's placement within a parent.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"},"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["taskId"],"properties":{"taskId":{"type":"string","description":"The ID of the task.","format":"guid","minLength":1},"parentId":{"type":["string","null"],"description":"The ID of the new parent task. If null, the task will be moved to the root level.","format":"guid"},"order":{"type":["number","null"],"description":"The new order/position of the task within its parent.","format":"int32","minimum":0,"exclusiveMinimum":true}},"description":"The JSON request body."}},"required":["projectIdOrKey","id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks/{id}/placement",
    executionParameters: [{"name":"projectIdOrKey","in":"path"},{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_GetCriticalPath", {
    name: "ProjectTasks_GetCriticalPath",
    description: `Returns an ordered list of task IDs on the critical path.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"}},"required":["projectIdOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks/critical-path",
    executionParameters: [{"name":"projectIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_AddTaskDependency", {
    name: "ProjectTasks_AddTaskDependency",
    description: `Creates a finish-to-start dependency where the specified task is the predecessor.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"},"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["predecessorId","successorId"],"properties":{"predecessorId":{"type":"string","description":"The ID of the predecessor task (the task that must complete first).","format":"guid","minLength":1},"successorId":{"type":"string","description":"The ID of the successor task (the task that depends on the predecessor).","format":"guid","minLength":1}},"description":"The JSON request body."}},"required":["projectIdOrKey","id","requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks/{id}/dependencies",
    executionParameters: [{"name":"projectIdOrKey","in":"path"},{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_RemoveTaskDependency", {
    name: "ProjectTasks_RemoveTaskDependency",
    description: `Removes the finish-to-start dependency between the predecessor and successor tasks.`,
    inputSchema: {"type":"object","properties":{"projectIdOrKey":{"type":"string"},"id":{"type":"string","format":"guid"},"successorId":{"type":"string","format":"guid"}},"required":["projectIdOrKey","id","successorId"]},
    method: "delete",
    pathTemplate: "/api/ppm/projects/{projectIdOrKey}/tasks/{id}/dependencies/{successorId}",
    executionParameters: [{"name":"projectIdOrKey","in":"path"},{"name":"id","in":"path"},{"name":"successorId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_GetTaskTypes", {
    name: "ProjectTasks_GetTaskTypes",
    description: `Get a list of all task types.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/projects/tasks/types",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_GetTaskStatuses", {
    name: "ProjectTasks_GetTaskStatuses",
    description: `Get a list of all task statuses.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/projects/tasks/statuses",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["ProjectTasks_GetTaskPriorities", {
    name: "ProjectTasks_GetTaskPriorities",
    description: `Get a list of all task priorities.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/projects/tasks/priorities",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_GetStrategicInitiatives", {
    name: "StrategicInitiatives_GetStrategicInitiatives",
    description: `Get a list of strategic initiatives.`,
    inputSchema: {"type":"object","properties":{"status":{"type":["number","null"],"format":"int32"}}},
    method: "get",
    pathTemplate: "/api/ppm/strategic-initiatives",
    executionParameters: [{"name":"status","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_Create", {
    name: "StrategicInitiatives_Create",
    description: `Create a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","description","start","end","portfolioId"],"properties":{"name":{"type":"string","description":"The name of the strategic initiative.","maxLength":128,"minLength":1},"description":{"type":"string","description":"A detailed explanation of what the strategic initiative aims to achieve.","maxLength":2048,"minLength":1},"start":{"type":"string","description":"The start date of the strategic initiative.","format":"date"},"end":{"type":"string","description":"The end date of the strategic initiative.","format":"date"},"portfolioId":{"type":"string","description":"The ID of the portfolio to which this strategic initiative belongs.","format":"guid","minLength":1},"sponsorIds":{"type":["array","null"],"description":"The sponsors of the strategic initiative.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"ownerIds":{"type":["array","null"],"description":"The Owners of the strategic initiative.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/strategic-initiatives",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_GetStrategicInitiative", {
    name: "StrategicInitiatives_GetStrategicInitiative",
    description: `Get strategic initiative details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/strategic-initiatives/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_Update", {
    name: "StrategicInitiatives_Update",
    description: `Update a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","description","start","end"],"properties":{"id":{"type":"string","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the strategic initiative.","maxLength":128,"minLength":1},"description":{"type":"string","description":"A detailed explanation of what the strategic initiative aims to achieve.","maxLength":2048,"minLength":1},"start":{"type":"string","description":"The start date of the strategic initiative.","format":"date"},"end":{"type":"string","description":"The end date of the strategic initiative.","format":"date"},"sponsorIds":{"type":["array","null"],"description":"The sponsors of the strategic initiative.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}},"ownerIds":{"type":["array","null"],"description":"The Owners of the strategic initiative.","items":{"type":"string","format":"guid","nullable":false,"example":"00000000-0000-0000-0000-000000000000"}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_Delete", {
    name: "StrategicInitiatives_Delete",
    description: `Delete a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_Approve", {
    name: "StrategicInitiatives_Approve",
    description: `Approve a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/approve",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_Activate", {
    name: "StrategicInitiatives_Activate",
    description: `Activate a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/activate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_Complete", {
    name: "StrategicInitiatives_Complete",
    description: `Complete a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/complete",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_Cancel", {
    name: "StrategicInitiatives_Cancel",
    description: `Cancel a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/cancel",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_GetKpis", {
    name: "StrategicInitiatives_GetKpis",
    description: `Get a list of KPIs for a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/kpis",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_CreateKpi", {
    name: "StrategicInitiatives_CreateKpi",
    description: `Create a KPI for a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["strategicInitiativeId","name","description","targetValue","unitId","targetDirectionId"],"properties":{"strategicInitiativeId":{"type":"string","description":"The ID of the strategic initiative to which this KPI belongs.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the KPI.","maxLength":64,"minLength":1},"description":{"type":"string","description":"A description of what the KPI measures.","maxLength":512,"minLength":1},"targetValue":{"type":"number","description":"The target value that defines success for the KPI.","format":"double","minLength":1},"unitId":{"type":"number","description":"The ID of the unit of measurement for the KPI.","format":"int32"},"targetDirectionId":{"type":"number","description":"The ID of the target direction for the KPI.","format":"int32"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/kpis",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_GetKpi", {
    name: "StrategicInitiatives_GetKpi",
    description: `Get a KPI for a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"},"kpiId":{"type":"string"}},"required":["id","kpiId"]},
    method: "get",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/kpis/{kpiId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"kpiId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_UpdateKpi", {
    name: "StrategicInitiatives_UpdateKpi",
    description: `Update a KPI for a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"kpiId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["strategicInitiativeId","kpiId","name","description","targetValue","unitId","targetDirectionId"],"properties":{"strategicInitiativeId":{"type":"string","description":"The ID of the strategic initiative to which this KPI belongs.","format":"guid","minLength":1},"kpiId":{"type":"string","description":"The ID of the KPI.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the KPI.","maxLength":64,"minLength":1},"description":{"type":"string","description":"A description of what the KPI measures.","maxLength":512,"minLength":1},"targetValue":{"type":"number","description":"The target value that defines success for the KPI.","format":"double","minLength":1},"unitId":{"type":"number","description":"The ID of the unit of measurement for the KPI.","format":"int32"},"targetDirectionId":{"type":"number","description":"The ID of the target direction for the KPI.","format":"int32"}},"description":"The JSON request body."}},"required":["id","kpiId","requestBody"]},
    method: "put",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/kpis/{kpiId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"kpiId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_DeleteKpi", {
    name: "StrategicInitiatives_DeleteKpi",
    description: `Delete a KPI for a strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"kpiId":{"type":"string","format":"guid"}},"required":["id","kpiId"]},
    method: "delete",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/kpis/{kpiId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"kpiId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_AddKpiMeasurement", {
    name: "StrategicInitiatives_AddKpiMeasurement",
    description: `Add a measurement to the strategic initiative KPI.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"kpiId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["strategicInitiativeId","kpiId","actualValue","measurementDate"],"properties":{"strategicInitiativeId":{"type":"string","description":"The ID of the strategic initiative to which this KPI belongs.","format":"guid","minLength":1},"kpiId":{"type":"string","description":"The ID of the KPI.","format":"guid","minLength":1},"actualValue":{"type":"number","description":"The actual measured value for the KPI at this check-in.","format":"double","minLength":1},"measurementDate":{"type":"string","description":"The date and time (in UTC) when the measurement was taken.","format":"date-time","minLength":1},"note":{"type":["string","null"],"description":"Optional note providing context for the measurement.","maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","kpiId","requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/kpis/{kpiId}/measurements",
    executionParameters: [{"name":"id","in":"path"},{"name":"kpiId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_GetKpiUnits", {
    name: "StrategicInitiatives_GetKpiUnits",
    description: `Get a list of KPI units.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/strategic-initiatives/kpi-units",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_GetKpiTargetDirections", {
    name: "StrategicInitiatives_GetKpiTargetDirections",
    description: `Get a list of KPI target directions.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/ppm/strategic-initiatives/kpi-target-directions",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_GetProjects", {
    name: "StrategicInitiatives_GetProjects",
    description: `Get a list of projects for the strategic initiative.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/ppm/strategic-initiatives/{idOrKey}/projects",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["StrategicInitiatives_ManageProjects", {
    name: "StrategicInitiatives_ManageProjects",
    description: `Manage projects for the strategic initiative.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","projectIds"],"properties":{"id":{"type":"string","description":"The ID of the strategic initiative to manage projects for.","format":"guid","minLength":1},"projectIds":{"type":"array","description":"The list of project IDs to be associated with the strategic initiative.","items":{"type":"string","format":"guid"}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/ppm/strategic-initiatives/{id}/projects",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetList", {
    name: "PlanningIntervals_GetList",
    description: `Get a list of planning intervals.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_Create", {
    name: "PlanningIntervals_Create",
    description: `Create a planning interval.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","start","end","iterationWeeks"],"properties":{"name":{"type":"string","description":"Gets the team name.","maxLength":128,"minLength":1},"description":{"type":["string","null"],"description":"Gets the team description.","maxLength":2048,"minLength":0},"start":{"type":"string","description":"Gets or sets the start.","format":"date"},"end":{"type":"string","description":"Gets or sets the end.","format":"date"},"iterationWeeks":{"type":"number","description":"Gets or sets the length of iterations in weeks.","format":"int32","minimum":0,"exclusiveMinimum":true},"iterationPrefix":{"type":["string","null"],"description":"Gets or sets the iteration prefix."}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/planning-intervals",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetPlanningInterval", {
    name: "PlanningIntervals_GetPlanningInterval",
    description: `Get planning interval details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetCalendar", {
    name: "PlanningIntervals_GetCalendar",
    description: `Get the PI calendar.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/calendar",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetPredictability", {
    name: "PlanningIntervals_GetPredictability",
    description: `Get the PI predictability for all teams.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/predictability",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_Update", {
    name: "PlanningIntervals_Update",
    description: `Update a planning interval.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","objectivesLocked"],"properties":{"id":{"type":"string","description":"Gets or sets the identifier.","format":"guid"},"name":{"type":"string","description":"Gets the team name.","maxLength":128,"minLength":1},"description":{"type":["string","null"],"description":"Gets the team description.","maxLength":2048,"minLength":0},"objectivesLocked":{"type":"boolean","description":"Gets or sets the objectives locked."}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/planning/planning-intervals/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetTeams", {
    name: "PlanningIntervals_GetTeams",
    description: `Get a list of planning interval teams.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/teams",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetTeamPredictability", {
    name: "PlanningIntervals_GetTeamPredictability",
    description: `Get the PI predictability for a team.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"teamId":{"type":"string","format":"guid"}},"required":["idOrKey","teamId"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/teams/{teamId}/predictability",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"teamId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_ManageDates", {
    name: "PlanningIntervals_ManageDates",
    description: `Manage planning interval dates and iterations.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","start","end","iterations"],"properties":{"id":{"type":"string","format":"guid","minLength":1},"start":{"type":"string","description":"Gets or sets the start.","format":"date"},"end":{"type":"string","description":"Gets or sets the end.","format":"date"},"iterations":{"type":"array","description":"The iterations for the Planning Interval.","items":{"type":"object","additionalProperties":false,"required":["name","categoryId","start","end"],"properties":{"iterationId":{"type":["string","null"],"format":"guid"},"name":{"type":"string","description":"The name of the iteration.","maxLength":128,"minLength":1},"categoryId":{"type":"number","description":"The category of iteration.","format":"int32"},"start":{"type":"string","description":"Gets or sets the start.","format":"date"},"end":{"type":"string","description":"Gets or sets the end.","format":"date"}}}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/planning-intervals/{id}/dates",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_ManageTeams", {
    name: "PlanningIntervals_ManageTeams",
    description: `Manage planning interval teams.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","teamIds"],"properties":{"id":{"type":"string","description":"The ID of the planning interval to manage teams for.","format":"guid"},"teamIds":{"type":"array","description":"The list of team IDs to be associated with the planning interval.","items":{"type":"string","format":"guid"}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/planning-intervals/{id}/teams",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetIterations", {
    name: "PlanningIntervals_GetIterations",
    description: `Get a list of planning interval iterations.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/iterations",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetIteration", {
    name: "PlanningIntervals_GetIteration",
    description: `Get a specific planning interval iteration.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"iterationIdOrKey":{"type":"string"}},"required":["idOrKey","iterationIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/iterations/{iterationIdOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"iterationIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetIterationCategories", {
    name: "PlanningIntervals_GetIterationCategories",
    description: `Get a list of iteration categories.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/iteration-categories",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetIterationSprints", {
    name: "PlanningIntervals_GetIterationSprints",
    description: `Retrieves all sprint-to-iteration mappings, with optional filtering by iteration.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"iterationId":{"type":["string","null"],"format":"guid"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/iterations/sprints",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"iterationId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_MapTeamSprints", {
    name: "PlanningIntervals_MapTeamSprints",
    description: `This is a sync/replace operation that sets the complete desired state for the team's sprint mappings.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"teamId":{"type":"string","format":"guid"},"requestBody":{"type":"object","description":"Request model for mapping team sprints to Planning Interval iterations.","additionalProperties":false,"required":["id","teamId","iterationSprintMappings"],"properties":{"id":{"type":"string","description":"The ID of the planning interval.","format":"guid","minLength":1},"teamId":{"type":"string","description":"The ID of the team whose sprints are being synchronized.","format":"guid","minLength":1},"iterationSprintMappings":{"type":"object","description":"Dictionary representing the complete desired state where key is iteration ID and value is sprint ID.\nThis is a sync/replace operation - any team sprints currently mapped but not included will be unmapped.\n- Non-null value: Maps the sprint to the iteration.\n- Null value: Explicitly unmaps any sprint from that iteration for this team.\n- Omitted iteration: No change to that iteration's mappings.","additionalProperties":{"type":"string","format":"guid","nullable":true,"example":"00000000-0000-0000-0000-000000000000"}}}}},"required":["id","teamId","requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/planning-intervals/{id}/teams/{teamId}/sprints",
    executionParameters: [{"name":"id","in":"path"},{"name":"teamId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetIterationMetrics", {
    name: "PlanningIntervals_GetIterationMetrics",
    description: `Get metrics for a PI iteration aggregated across all mapped sprints.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"iterationIdOrKey":{"type":"string"}},"required":["idOrKey","iterationIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/iterations/{iterationIdOrKey}/metrics",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"iterationIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetIterationBacklog", {
    name: "PlanningIntervals_GetIterationBacklog",
    description: `Get combined backlog for a PI iteration from all mapped sprints.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"iterationIdOrKey":{"type":"string"}},"required":["idOrKey","iterationIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/iterations/{iterationIdOrKey}/backlog",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"iterationIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetObjectives", {
    name: "PlanningIntervals_GetObjectives",
    description: `Get a list of planning interval teams.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"teamId":{"type":["string","null"],"format":"guid"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/objectives",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"teamId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetObjective", {
    name: "PlanningIntervals_GetObjective",
    description: `Get a planning interval objective.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"objectiveIdOrKey":{"type":"string"}},"required":["idOrKey","objectiveIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/objectives/{objectiveIdOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"objectiveIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_CreateObjective", {
    name: "PlanningIntervals_CreateObjective",
    description: `Create a planning interval objective.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["planningIntervalId","teamId","name","isStretch"],"properties":{"planningIntervalId":{"type":"string","format":"guid"},"teamId":{"type":"string","format":"guid","minLength":1},"name":{"type":"string","maxLength":256,"minLength":1},"description":{"type":["string","null"],"maxLength":1024,"minLength":0},"startDate":{"type":["string","null"],"format":"date"},"targetDate":{"type":["string","null"],"format":"date"},"isStretch":{"type":"boolean"},"order":{"type":["number","null"],"format":"int32"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/planning-intervals/{id}/objectives",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_UpdateObjective", {
    name: "PlanningIntervals_UpdateObjective",
    description: `Update a planning interval objective.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"objectiveId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["planningIntervalId","objectiveId","name","statusId","progress","isStretch"],"properties":{"planningIntervalId":{"type":"string","format":"guid"},"objectiveId":{"type":"string","format":"guid"},"name":{"type":"string","maxLength":256,"minLength":1},"description":{"type":["string","null"],"maxLength":1024,"minLength":0},"statusId":{"type":"number","format":"int32"},"progress":{"type":"number","format":"double","maximum":100,"minimum":0},"startDate":{"type":["string","null"],"format":"date"},"targetDate":{"type":["string","null"],"format":"date"},"isStretch":{"type":"boolean"}},"description":"The JSON request body."}},"required":["id","objectiveId","requestBody"]},
    method: "put",
    pathTemplate: "/api/planning/planning-intervals/{id}/objectives/{objectiveId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"objectiveId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_DeleteObjective", {
    name: "PlanningIntervals_DeleteObjective",
    description: `Delete a planning interval objective.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"objectiveId":{"type":"string","format":"guid"}},"required":["id","objectiveId"]},
    method: "delete",
    pathTemplate: "/api/planning/planning-intervals/{id}/objectives/{objectiveId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"objectiveId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_UpdateObjectivesOrder", {
    name: "PlanningIntervals_UpdateObjectivesOrder",
    description: `Update the order of planning interval objectives.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["planningIntervalId","objectives"],"properties":{"planningIntervalId":{"type":"string","format":"guid"},"objectives":{"type":"object","additionalProperties":{"type":"integer","format":"int32","nullable":true}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/planning/planning-intervals/{id}/objectives/order",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetObjectivesHealthReport", {
    name: "PlanningIntervals_GetObjectivesHealthReport",
    description: `Get a health report for planning interval objectives.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"teamId":{"type":["string","null"],"format":"guid"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/objectives/health-report",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"teamId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetObjectiveWorkItems", {
    name: "PlanningIntervals_GetObjectiveWorkItems",
    description: `Get work items for an objective.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"objectiveIdOrKey":{"type":"string"}},"required":["idOrKey","objectiveIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/objectives/{objectiveIdOrKey}/work-items",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"objectiveIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetObjectiveWorkItemMetrics", {
    name: "PlanningIntervals_GetObjectiveWorkItemMetrics",
    description: `Get metrics for the work items linked to an objective.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"objectiveIdOrKey":{"type":"string"}},"required":["idOrKey","objectiveIdOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/objectives/{objectiveIdOrKey}/work-items/metrics",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"objectiveIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_ManageObjectiveWorkItems", {
    name: "PlanningIntervals_ManageObjectiveWorkItems",
    description: `Manage objective work items.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"objectiveId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["planningIntervalId","objectiveId","workItemIds"],"properties":{"planningIntervalId":{"type":"string","format":"guid"},"objectiveId":{"type":"string","format":"guid"},"workItemIds":{"type":"array","items":{"type":"string","format":"guid"}}},"description":"The JSON request body."}},"required":["id","objectiveId","requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/planning-intervals/{id}/objectives/{objectiveId}/work-items",
    executionParameters: [{"name":"id","in":"path"},{"name":"objectiveId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_ImportObjectives", {
    name: "PlanningIntervals_ImportObjectives",
    description: `Import objectives for a planning interval from a csv file.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"string","description":"Request body (content type: multipart/form-data)"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/planning/planning-intervals/{id}/objectives/import",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "multipart/form-data",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetObjectiveStatuses", {
    name: "PlanningIntervals_GetObjectiveStatuses",
    description: `Get a list of all PI objective statuses.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/objective-statuses",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["PlanningIntervals_GetRisks", {
    name: "PlanningIntervals_GetRisks",
    description: `Get planning interval risks. The default value for includeClosed is false.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"includeClosed":{"type":["boolean","null"]},"teamId":{"type":["string","null"],"format":"guid"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/planning-intervals/{idOrKey}/risks",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"includeClosed","in":"query"},{"name":"teamId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Risks_GetList", {
    name: "Risks_GetList",
    description: `Get a list of risks.`,
    inputSchema: {"type":"object","properties":{"includeClosed":{"type":"boolean","default":false}}},
    method: "get",
    pathTemplate: "/api/planning/risks",
    executionParameters: [{"name":"includeClosed","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Risks_Create", {
    name: "Risks_Create",
    description: `Create a risk.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["teamId","summary","categoryId","impactId","likelihoodId"],"properties":{"teamId":{"type":"string","format":"guid","minLength":1},"summary":{"type":"string","maxLength":256,"minLength":1},"description":{"type":["string","null"],"maxLength":1024,"minLength":0},"categoryId":{"type":"number","format":"int32"},"impactId":{"type":"number","format":"int32"},"likelihoodId":{"type":"number","format":"int32"},"assigneeId":{"type":["string","null"],"format":"guid"},"followUpDate":{"type":["string","null"],"format":"date"},"response":{"type":["string","null"],"maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/risks",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Risks_GetRisk", {
    name: "Risks_GetRisk",
    description: `Get risk details using the Id or key.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/risks/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Risks_GetMyRisks", {
    name: "Risks_GetMyRisks",
    description: `Get a list of open risks assigned to me.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/risks/me",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Risks_Update", {
    name: "Risks_Update",
    description: `Update a risk.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["riskId","teamId","summary","statusId","categoryId","impactId","likelihoodId"],"properties":{"riskId":{"type":"string","format":"guid"},"teamId":{"type":"string","format":"guid","minLength":1},"summary":{"type":"string","maxLength":256,"minLength":1},"description":{"type":["string","null"],"maxLength":1024,"minLength":0},"statusId":{"type":"number","format":"int32"},"categoryId":{"type":"number","format":"int32"},"impactId":{"type":"number","format":"int32"},"likelihoodId":{"type":"number","format":"int32"},"assigneeId":{"type":["string","null"],"format":"guid"},"followUpDate":{"type":["string","null"],"format":"date"},"response":{"type":["string","null"],"maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/planning/risks/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Risks_Import", {
    name: "Risks_Import",
    description: `Import risks from a csv file.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"string","description":"Request body (content type: multipart/form-data)"}}},
    method: "post",
    pathTemplate: "/api/planning/risks/import",
    executionParameters: [],
    requestBodyContentType: "multipart/form-data",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Risks_GetStatuses", {
    name: "Risks_GetStatuses",
    description: `Get a list of all risk statuses.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/risks/statuses",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Risks_GetCategories", {
    name: "Risks_GetCategories",
    description: `Get a list of all risk categories.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/risks/categories",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Risks_GetGrades", {
    name: "Risks_GetGrades",
    description: `Get a list of all risk grades.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/risks/grades",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_GetRoadmaps", {
    name: "Roadmaps_GetRoadmaps",
    description: `Get a list of roadmaps.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/roadmaps",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_Create", {
    name: "Roadmaps_Create",
    description: `Create a roadmap.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","start","end","roadmapManagerIds","visibilityId"],"properties":{"name":{"type":"string","description":"The name of the Roadmap.","maxLength":128,"minLength":1},"description":{"type":["string","null"],"description":"The description of the Roadmap.","maxLength":2048,"minLength":0},"start":{"type":"string","description":"The Roadmap start date.","format":"date"},"end":{"type":"string","description":"The Roadmap end date.","format":"date"},"roadmapManagerIds":{"type":"array","description":"The managers of the Roadmap.","minLength":1,"items":{"type":"string","format":"guid"}},"visibilityId":{"type":"number","description":"The visibility id for the Roadmap. If the Roadmap is public, all users can see the Roadmap. Otherwise, only the Roadmap Managers can see the Roadmap.","format":"int32"}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/roadmaps",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_GetRoadmap", {
    name: "Roadmaps_GetRoadmap",
    description: `Get roadmap details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/roadmaps/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_Copy", {
    name: "Roadmaps_Copy",
    description: `Copy an existing roadmap.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["sourceRoadmapId","name","roadmapManagerIds","visibilityId"],"properties":{"sourceRoadmapId":{"type":"string","description":"The Id of the source Roadmap to copy.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the new Roadmap.","maxLength":128,"minLength":1},"roadmapManagerIds":{"type":"array","description":"The managers of the new Roadmap.","minLength":1,"items":{"type":"string","format":"guid"}},"visibilityId":{"type":"number","description":"The visibility id for the new Roadmap. If the Roadmap is public, all users can see the Roadmap. Otherwise, only the Roadmap Managers can see the Roadmap.","format":"int32"}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/roadmaps/copy",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_Update", {
    name: "Roadmaps_Update",
    description: `Update a roadmap.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","start","end","roadmapManagerIds","visibilityId"],"properties":{"id":{"type":"string","description":"The unique identifier of the Roadmap.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the Roadmap.","maxLength":128,"minLength":1},"description":{"type":["string","null"],"description":"The description of the Roadmap.","maxLength":2048,"minLength":0},"start":{"type":"string","description":"The Roadmap start date.","format":"date"},"end":{"type":"string","description":"The Roadmap end date.","format":"date"},"roadmapManagerIds":{"type":"array","description":"The managers of the Roadmap.","minLength":1,"items":{"type":"string","format":"guid"}},"visibilityId":{"type":"number","description":"The visibility id for the Roadmap. If the Roadmap is public, all users can see the Roadmap. Otherwise, only the Roadmap Managers can see the Roadmap.","format":"int32"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/planning/roadmaps/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_Delete", {
    name: "Roadmaps_Delete",
    description: `Delete a roadmap.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/planning/roadmaps/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_GetItems", {
    name: "Roadmaps_GetItems",
    description: `Get roadmap items`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/roadmaps/{idOrKey}/items",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_GetActivities", {
    name: "Roadmaps_GetActivities",
    description: `Get roadmap activities`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/roadmaps/{idOrKey}/items/activities",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_GetItem", {
    name: "Roadmaps_GetItem",
    description: `Get roadmap item details`,
    inputSchema: {"type":"object","properties":{"roadmapIdOrKey":{"type":"string"},"itemId":{"type":"string","format":"guid"}},"required":["roadmapIdOrKey","itemId"]},
    method: "get",
    pathTemplate: "/api/planning/roadmaps/{roadmapIdOrKey}/items/{itemId}",
    executionParameters: [{"name":"roadmapIdOrKey","in":"path"},{"name":"itemId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_CreateItem", {
    name: "Roadmaps_CreateItem",
    description: `Create a roadmap item of type: Activity, Timebox, Milestone.`,
    inputSchema: {"type":"object","properties":{"roadmapId":{"type":"string","format":"guid"},"requestBody":{"type":"object","discriminator":{"propertyName":"$type","mapping":{"activity":"#/components/schemas/CreateRoadmapActivityRequest","milestone":"#/components/schemas/CreateRoadmapMilestoneRequest","timebox":"#/components/schemas/CreateRoadmapTimeboxRequest"}},"x-abstract":true,"additionalProperties":false,"required":["$type","roadmapId","name"],"properties":{"roadmapId":{"type":"string","description":"The Roadmap Id the Roadmap Item belongs to.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the Roadmap Item.","maxLength":128,"minLength":1},"description":{"type":["string","null"],"description":"The description of the Roadmap Item.","maxLength":2048,"minLength":0},"parentId":{"type":["string","null"],"description":"The parent Roadmap Item Id. This is used to connect Roadmap Items together.","format":"guid"},"color":{"type":["string","null"],"description":"The color of the Roadmap Item. This is used to display the Roadmap Item in the UI."},"$type":{"type":"string"}},"description":"The JSON request body."}},"required":["roadmapId","requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/roadmaps/{roadmapId}/items",
    executionParameters: [{"name":"roadmapId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_UpdateItem", {
    name: "Roadmaps_UpdateItem",
    description: `Update a roadmap item of type: Activity, Timebox, Milestone.`,
    inputSchema: {"type":"object","properties":{"roadmapId":{"type":"string","format":"guid"},"itemId":{"type":"string","format":"guid"},"requestBody":{"type":"object","discriminator":{"propertyName":"$type","mapping":{"activity":"#/components/schemas/UpdateRoadmapActivityRequest","milestone":"#/components/schemas/UpdateRoadmapMilestoneRequest","timebox":"#/components/schemas/UpdateRoadmapTimeboxRequest"}},"x-abstract":true,"additionalProperties":false,"required":["$type","roadmapId","itemId","name"],"properties":{"roadmapId":{"type":"string","description":"The Roadmap Id the Roadmap Item belongs to.","format":"guid","minLength":1},"itemId":{"type":"string","description":"The Roadmap Item Id.","format":"guid","minLength":1},"name":{"type":"string","description":"The name of the Roadmap Item.","maxLength":128,"minLength":1},"description":{"type":["string","null"],"description":"The description of the Roadmap Item.","maxLength":2048,"minLength":0},"parentId":{"type":["string","null"],"description":"The parent Roadmap Item Id. This is used to connect Roadmap Items together.","format":"guid"},"color":{"type":["string","null"],"description":"The color of the Roadmap Item. This is used to display the Roadmap Item in the UI."},"$type":{"type":"string"}},"description":"The JSON request body."}},"required":["roadmapId","itemId","requestBody"]},
    method: "put",
    pathTemplate: "/api/planning/roadmaps/{roadmapId}/items/{itemId}",
    executionParameters: [{"name":"roadmapId","in":"path"},{"name":"itemId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_DeleteItem", {
    name: "Roadmaps_DeleteItem",
    description: `Delete a roadmap item.`,
    inputSchema: {"type":"object","properties":{"roadmapId":{"type":"string","format":"guid"},"itemId":{"type":"string","format":"guid"}},"required":["roadmapId","itemId"]},
    method: "delete",
    pathTemplate: "/api/planning/roadmaps/{roadmapId}/items/{itemId}",
    executionParameters: [{"name":"roadmapId","in":"path"},{"name":"itemId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_UpdateItemDates", {
    name: "Roadmaps_UpdateItemDates",
    description: `Update the date(s) on a roadmap item of type: Activity, Timebox, Milestone.`,
    inputSchema: {"type":"object","properties":{"roadmapId":{"type":"string","format":"guid"},"itemId":{"type":"string","format":"guid"},"requestBody":{"type":"object","discriminator":{"propertyName":"$type","mapping":{"activity":"#/components/schemas/UpdateRoadmapActivityDatesRequest","milestone":"#/components/schemas/UpdateRoadmapMilestoneDatesRequest","timebox":"#/components/schemas/UpdateRoadmapTimeboxDatesRequest"}},"x-abstract":true,"additionalProperties":false,"required":["$type","roadmapId","itemId"],"properties":{"roadmapId":{"type":"string","description":"The Roadmap Id the Roadmap Item belongs to.","format":"guid","minLength":1},"itemId":{"type":"string","description":"The Roadmap Item Id.","format":"guid","minLength":1},"$type":{"type":"string"}},"description":"The JSON request body."}},"required":["roadmapId","itemId","requestBody"]},
    method: "put",
    pathTemplate: "/api/planning/roadmaps/{roadmapId}/items/{itemId}/dates",
    executionParameters: [{"name":"roadmapId","in":"path"},{"name":"itemId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_ReorganizeActivity", {
    name: "Roadmaps_ReorganizeActivity",
    description: `Reorganize a roadmap activity.`,
    inputSchema: {"type":"object","properties":{"roadmapId":{"type":"string","format":"guid"},"activityId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["roadmapId","activityId","order"],"properties":{"roadmapId":{"type":"string","format":"guid","minLength":1},"parentActivityId":{"type":["string","null"],"format":"guid"},"activityId":{"type":"string","format":"guid","minLength":1},"order":{"type":"number","format":"int32","minimum":0,"exclusiveMinimum":true}},"description":"The JSON request body."}},"required":["roadmapId","activityId","requestBody"]},
    method: "post",
    pathTemplate: "/api/planning/roadmaps/{roadmapId}/items/{activityId}/reorganize",
    executionParameters: [{"name":"roadmapId","in":"path"},{"name":"activityId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Roadmaps_GetVisibilityOptions", {
    name: "Roadmaps_GetVisibilityOptions",
    description: `Get a list of all visibility.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/planning/roadmaps/visibility-options",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Sprints_GetSprints", {
    name: "Sprints_GetSprints",
    description: `Get a list of sprints.`,
    inputSchema: {"type":"object","properties":{"teamId":{"type":["string","null"],"format":"guid"}}},
    method: "get",
    pathTemplate: "/api/planning/sprints",
    executionParameters: [{"name":"teamId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Sprints_GetSprint", {
    name: "Sprints_GetSprint",
    description: `Get sprint details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/sprints/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Sprints_GetSprintBacklog", {
    name: "Sprints_GetSprintBacklog",
    description: `Get sprint backlog items.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/sprints/{idOrKey}/backlog",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Sprints_GetSprintMetrics", {
    name: "Sprints_GetSprintMetrics",
    description: `Get sprint work item metrics.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/planning/sprints/{idOrKey}/metrics",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamTypes_GetList", {
    name: "TeamTypes_GetList",
    description: `Get a list of all team types.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/organization/team-types",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkProcesses_GetList", {
    name: "WorkProcesses_GetList",
    description: `Get a list of work processes.`,
    inputSchema: {"type":"object","properties":{"includeInactive":{"type":"boolean","default":false}}},
    method: "get",
    pathTemplate: "/api/work/work-processes",
    executionParameters: [{"name":"includeInactive","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkProcesses_Get", {
    name: "WorkProcesses_Get",
    description: `Get work process details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/work/work-processes/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkProcesses_Activate", {
    name: "WorkProcesses_Activate",
    description: `Activate a work process.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/work/work-processes/{id}/activate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkProcesses_Deactivate", {
    name: "WorkProcesses_Deactivate",
    description: `Deactivate a work process.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/work/work-processes/{id}/deactivate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkProcesses_GetSchemes", {
    name: "WorkProcesses_GetSchemes",
    description: `Get work process schemes.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/work/work-processes/{id}/schemes",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_GetList", {
    name: "Workspaces_GetList",
    description: `Get a list of workspaces.`,
    inputSchema: {"type":"object","properties":{"includeInactive":{"type":"boolean","default":false}}},
    method: "get",
    pathTemplate: "/api/work/workspaces",
    executionParameters: [{"name":"includeInactive","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_Get", {
    name: "Workspaces_Get",
    description: `Get workspace details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/work/workspaces/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_SetExternalUrlTemplates", {
    name: "Workspaces_SetExternalUrlTemplates",
    description: `Set the external view work item URL template for a workspace.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"properties":{"externalViewWorkItemUrlTemplate":{"type":["string","null"],"maxLength":256,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/work/workspaces/{id}/external-url-templates",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_GetWorkItems", {
    name: "Workspaces_GetWorkItems",
    description: `Get work items for a workspace.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/work/workspaces/{idOrKey}/work-items",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_GetWorkItem", {
    name: "Workspaces_GetWorkItem",
    description: `Get work item details.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"workItemKey":{"type":"string"}},"required":["idOrKey","workItemKey"]},
    method: "get",
    pathTemplate: "/api/work/workspaces/{idOrKey}/work-items/{workItemKey}",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"workItemKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_GetWorkItemProjectInfo", {
    name: "Workspaces_GetWorkItemProjectInfo",
    description: `Get a work item's project info.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"workItemKey":{"type":"string"}},"required":["idOrKey","workItemKey"]},
    method: "get",
    pathTemplate: "/api/work/workspaces/{idOrKey}/work-items/{workItemKey}/project-info",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"workItemKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_UpdateWorkItemProject", {
    name: "Workspaces_UpdateWorkItemProject",
    description: `Update the project for a work item.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"workItemId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["workItemId"],"properties":{"workItemId":{"type":"string","format":"guid","minLength":1},"projectId":{"type":["string","null"],"format":"guid"}},"description":"The JSON request body."}},"required":["id","workItemId","requestBody"]},
    method: "put",
    pathTemplate: "/api/work/workspaces/{id}/work-items/{workItemId}/update-project",
    executionParameters: [{"name":"id","in":"path"},{"name":"workItemId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_GetChildWorkItems", {
    name: "Workspaces_GetChildWorkItems",
    description: `Get a work item's child work items.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"workItemKey":{"type":"string"}},"required":["idOrKey","workItemKey"]},
    method: "get",
    pathTemplate: "/api/work/workspaces/{idOrKey}/work-items/{workItemKey}/children",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"workItemKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_GetWorkItemDependencies", {
    name: "Workspaces_GetWorkItemDependencies",
    description: `Get a work item's dependencies.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"workItemKey":{"type":"string"}},"required":["idOrKey","workItemKey"]},
    method: "get",
    pathTemplate: "/api/work/workspaces/{idOrKey}/work-items/{workItemKey}/dependencies",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"workItemKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_GetMetrics", {
    name: "Workspaces_GetMetrics",
    description: `Get metrics for a work item.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"},"workItemKey":{"type":"string"}},"required":["idOrKey","workItemKey"]},
    method: "get",
    pathTemplate: "/api/work/workspaces/{idOrKey}/work-items/{workItemKey}/metrics",
    executionParameters: [{"name":"idOrKey","in":"path"},{"name":"workItemKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Workspaces_SearchWorkItems", {
    name: "Workspaces_SearchWorkItems",
    description: `Search for a work item using its key or title.`,
    inputSchema: {"type":"object","properties":{"query":{"type":"string"},"top":{"type":"number","format":"int32","default":50}}},
    method: "get",
    pathTemplate: "/api/work/workspaces/work-items/search",
    executionParameters: [{"name":"query","in":"query"},{"name":"top","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkStatusCategories_GetList", {
    name: "WorkStatusCategories_GetList",
    description: `Get a list of all work status categories.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/work/work-status-categories",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkStatuses_GetList", {
    name: "WorkStatuses_GetList",
    description: `Get a list of all work statuss.`,
    inputSchema: {"type":"object","properties":{"includeInactive":{"type":"boolean","default":false}}},
    method: "get",
    pathTemplate: "/api/work/work-statuses",
    executionParameters: [{"name":"includeInactive","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkStatuses_Create", {
    name: "WorkStatuses_Create",
    description: `Create a work status.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name"],"properties":{"name":{"type":"string","description":"The name of the work status.  The name cannot be changed.","maxLength":64,"minLength":1},"description":{"type":["string","null"],"description":"The description of the work status.","maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/work/work-statuses",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkStatuses_GetById", {
    name: "WorkStatuses_GetById",
    description: `Get work status details using the id.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/work/work-statuses/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkStatuses_Update", {
    name: "WorkStatuses_Update",
    description: `Update a work status.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"},"requestBody":{"type":"object","additionalProperties":false,"required":["id"],"properties":{"id":{"type":"number","format":"int32"},"description":{"type":["string","null"],"description":"The description of the work status.","maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/work/work-statuses/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkTypeLevels_GetList", {
    name: "WorkTypeLevels_GetList",
    description: `Get a list of all work type levels.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/work/work-type-levels",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkTypeLevels_Create", {
    name: "WorkTypeLevels_Create",
    description: `Create a work type level.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name"],"properties":{"name":{"type":"string","description":"The name of the work type level.  The name cannot be changed.","maxLength":128,"minLength":1},"description":{"type":["string","null"],"description":"The description of the work type level.","maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/work/work-type-levels",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkTypeLevels_GetById", {
    name: "WorkTypeLevels_GetById",
    description: `Get work type level details using the id.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/work/work-type-levels/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkTypeLevels_Update", {
    name: "WorkTypeLevels_Update",
    description: `Update a work type level.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name"],"properties":{"id":{"type":"number","format":"int32"},"name":{"type":"string","description":"The name of the work type level.  The name cannot be changed.","maxLength":128,"minLength":1},"description":{"type":["string","null"],"description":"The description of the work type level.","maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/work/work-type-levels/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkTypeLevels_UpdateOrder", {
    name: "WorkTypeLevels_UpdateOrder",
    description: `Update the order of portfolio tier work type levels.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["levels"],"properties":{"levels":{"type":"object","additionalProperties":{"type":"integer","format":"int32"}}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "put",
    pathTemplate: "/order",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkTypes_GetList", {
    name: "WorkTypes_GetList",
    description: `Get a list of all work types.`,
    inputSchema: {"type":"object","properties":{"includeInactive":{"type":"boolean","default":false}}},
    method: "get",
    pathTemplate: "/api/work/work-types",
    executionParameters: [{"name":"includeInactive","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkTypes_Create", {
    name: "WorkTypes_Create",
    description: `Create a work type.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","levelId"],"properties":{"name":{"type":"string","description":"The name of the work type.  The name cannot be changed.","maxLength":64,"minLength":1},"description":{"type":["string","null"],"description":"The description of the work type.","maxLength":1024,"minLength":0},"levelId":{"type":"number","description":"The work type level identifier.","format":"int32","minimum":0,"exclusiveMinimum":true}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/work/work-types",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkTypes_GetById", {
    name: "WorkTypes_GetById",
    description: `Get work type details using the id.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/work/work-types/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkTypes_Update", {
    name: "WorkTypes_Update",
    description: `Update a work type.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","levelId"],"properties":{"id":{"type":"number","format":"int32"},"description":{"type":["string","null"],"description":"The description of the work type.","maxLength":1024,"minLength":0},"levelId":{"type":"number","description":"The work type level identifier.","format":"int32","minimum":0,"exclusiveMinimum":true}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/work/work-types/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["WorkTypeTiers_GetList", {
    name: "WorkTypeTiers_GetList",
    description: `Get a list of all work type tiers.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/work/work-type-tiers",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Employees_GetList", {
    name: "Employees_GetList",
    description: `Get a list of all employees.`,
    inputSchema: {"type":"object","properties":{"includeInactive":{"type":"boolean","default":false}}},
    method: "get",
    pathTemplate: "/api/organization/employees",
    executionParameters: [{"name":"includeInactive","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Employees_Create", {
    name: "Employees_Create",
    description: `Create an employee.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["firstName","lastName","employeeNumber","email"],"properties":{"firstName":{"type":"string","description":"Gets the first name.","maxLength":100,"minLength":1},"middleName":{"type":["string","null"],"description":"Gets the middle name.","maxLength":100,"minLength":0},"lastName":{"type":"string","description":"Gets the last name.","maxLength":100,"minLength":1},"suffix":{"type":["string","null"],"description":"Gets the suffix.","maxLength":50,"minLength":0},"title":{"type":["string","null"],"description":"Gets the employee's personal title.","maxLength":50,"minLength":0},"employeeNumber":{"type":"string","description":"Gets the employee number.","maxLength":256,"minLength":1},"hireDate":{"type":["string","null"],"description":"Gets the hire date.","format":"date-time"},"email":{"type":"string","description":"Gets the email.","maxLength":256,"minLength":1},"jobTitle":{"type":["string","null"],"description":"Gets the job title.","maxLength":256,"minLength":0},"department":{"type":["string","null"],"description":"Gets the department.","maxLength":256,"minLength":0},"officeLocation":{"type":["string","null"],"description":"Gets the office location.","maxLength":256,"minLength":0},"managerId":{"type":["string","null"],"description":"Gets the manager identifier.","format":"guid"}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/organization/employees",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Employees_GetEmployee", {
    name: "Employees_GetEmployee",
    description: `Get employee details using the Id or key.`,
    inputSchema: {"type":"object","properties":{"idOrKey":{"type":"string"}},"required":["idOrKey"]},
    method: "get",
    pathTemplate: "/api/organization/employees/{idOrKey}",
    executionParameters: [{"name":"idOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Employees_Update", {
    name: "Employees_Update",
    description: `Update an employee.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","firstName","lastName","employeeNumber","email"],"properties":{"id":{"type":"string","description":"Gets or sets the identifier.","format":"guid"},"firstName":{"type":"string","description":"Gets the first name.","maxLength":100,"minLength":1},"middleName":{"type":["string","null"],"description":"Gets the middle name.","maxLength":100,"minLength":0},"lastName":{"type":"string","description":"Gets the last name.","maxLength":100,"minLength":1},"suffix":{"type":["string","null"],"description":"Gets the suffix.","maxLength":50,"minLength":0},"title":{"type":["string","null"],"description":"Gets the employee's personal title.","maxLength":50,"minLength":0},"employeeNumber":{"type":"string","description":"Gets the employee number.","maxLength":256,"minLength":1},"hireDate":{"type":["string","null"],"description":"Gets the hire date.","format":"date-time"},"email":{"type":"string","description":"Gets the email.","maxLength":256,"minLength":1},"jobTitle":{"type":["string","null"],"description":"Gets the job title.","maxLength":256,"minLength":0},"department":{"type":["string","null"],"description":"Gets the department.","maxLength":256,"minLength":0},"officeLocation":{"type":["string","null"],"description":"Gets the office location.","maxLength":256,"minLength":0},"managerId":{"type":["string","null"],"description":"Gets the manager identifier.","format":"guid"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/organization/employees/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Employees_Delete", {
    name: "Employees_Delete",
    description: `Delete an employee.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/organization/employees/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Employees_GetDirectReports", {
    name: "Employees_GetDirectReports",
    description: `Get a list of direct reports for an employee.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/employees/{id}/direct-reports",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Employees_RemoveInvalid", {
    name: "Employees_RemoveInvalid",
    description: `Remove invalid employee record from employee list.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/organization/employees/{id}/remove-invalid",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetList", {
    name: "Teams_GetList",
    description: `Get a list of teams.`,
    inputSchema: {"type":"object","properties":{"includeInactive":{"type":"boolean","default":false}}},
    method: "get",
    pathTemplate: "/api/organization/teams",
    executionParameters: [{"name":"includeInactive","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_Create", {
    name: "Teams_Create",
    description: `Create a team.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","code","activeDate"],"properties":{"name":{"type":"string","description":"Gets the team name.","maxLength":128,"minLength":1},"code":{"type":"string","description":"Gets the code.","maxLength":10,"minLength":1},"description":{"type":["string","null"],"description":"Gets the team description.","maxLength":1024,"minLength":0},"activeDate":{"type":"string","description":"The active date for the team.","format":"date","minLength":1}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/organization/teams",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetById", {
    name: "Teams_GetById",
    description: `Get team details using the key.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_Update", {
    name: "Teams_Update",
    description: `Update a team.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","code"],"properties":{"id":{"type":"string","format":"guid"},"name":{"type":"string","description":"Gets the team name.","maxLength":128,"minLength":1},"code":{"type":"string","description":"Gets the code.","maxLength":10,"minLength":1},"description":{"type":["string","null"],"description":"Gets the team description.","maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/organization/teams/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_Deactivate", {
    name: "Teams_Deactivate",
    description: `Deactivate a team.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","inactiveDate"],"properties":{"id":{"type":"string","format":"guid","minLength":1},"inactiveDate":{"type":"string","format":"date","minLength":1}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/organization/teams/{id}/deactivate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetTeamMemberships", {
    name: "Teams_GetTeamMemberships",
    description: `Get parent team memberships.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}/team-memberships",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_AddTeamMembership", {
    name: "Teams_AddTeamMembership",
    description: `Add a parent team membership.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["teamId","parentTeamId","start"],"properties":{"teamId":{"type":"string","format":"guid","minLength":1},"parentTeamId":{"type":"string","format":"guid","minLength":1},"start":{"type":"string","format":"date"},"end":{"type":["string","null"],"format":"date"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/organization/teams/{id}/team-memberships",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_UpdateTeamMembership", {
    name: "Teams_UpdateTeamMembership",
    description: `Update a team membership.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"teamMembershipId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["teamId","teamMembershipId","start"],"properties":{"teamId":{"type":"string","format":"guid","minLength":1},"teamMembershipId":{"type":"string","format":"guid","minLength":1},"start":{"type":"string","format":"date"},"end":{"type":["string","null"],"format":"date"}},"description":"The JSON request body."}},"required":["id","teamMembershipId","requestBody"]},
    method: "put",
    pathTemplate: "/api/organization/teams/{id}/team-memberships/{teamMembershipId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"teamMembershipId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_RemoveTeamMembership", {
    name: "Teams_RemoveTeamMembership",
    description: `Remove a parent team membership.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"teamMembershipId":{"type":"string","format":"guid"}},"required":["id","teamMembershipId"]},
    method: "delete",
    pathTemplate: "/api/organization/teams/{id}/team-memberships/{teamMembershipId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"teamMembershipId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetTeamBacklog", {
    name: "Teams_GetTeamBacklog",
    description: `Get the backlog for a team.`,
    inputSchema: {"type":"object","properties":{"idOrCode":{"type":"string"}},"required":["idOrCode"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{idOrCode}/backlog",
    executionParameters: [{"name":"idOrCode","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetTeamWorkItems", {
    name: "Teams_GetTeamWorkItems",
    description: `Get the work items for a team.`,
    inputSchema: {"type":"object","properties":{"idOrCode":{"type":"string"},"statusCategories":{"type":["array","null"],"items":{"type":"string","description":"","x-enumNames":["Proposed","Active","Done","Removed"],"enum":["Proposed","Active","Done","Removed"]}},"doneFrom":{"type":["string","null"],"format":"date-time"},"doneTo":{"type":["string","null"],"format":"date-time"}},"required":["idOrCode"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{idOrCode}/work-items",
    executionParameters: [{"name":"idOrCode","in":"path"},{"name":"statusCategories","in":"query"},{"name":"doneFrom","in":"query"},{"name":"doneTo","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetTeamDependencies", {
    name: "Teams_GetTeamDependencies",
    description: `Get the active dependencies for a team.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}/dependencies",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetRisks", {
    name: "Teams_GetRisks",
    description: `Get team risks.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"includeClosed":{"type":"boolean","default":false}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}/risks",
    executionParameters: [{"name":"id","in":"path"},{"name":"includeClosed","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_CreateRisk", {
    name: "Teams_CreateRisk",
    description: `Create a risk for a team.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["teamId","summary","categoryId","impactId","likelihoodId"],"properties":{"teamId":{"type":"string","format":"guid","minLength":1},"summary":{"type":"string","maxLength":256,"minLength":1},"description":{"type":["string","null"],"maxLength":1024,"minLength":0},"categoryId":{"type":"number","format":"int32"},"impactId":{"type":"number","format":"int32"},"likelihoodId":{"type":"number","format":"int32"},"assigneeId":{"type":["string","null"],"format":"guid"},"followUpDate":{"type":["string","null"],"format":"date"},"response":{"type":["string","null"],"maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/organization/teams/{id}/risks",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetRiskById", {
    name: "Teams_GetRiskById",
    description: `Get a team risk by Id.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"riskIdOrKey":{"type":"string"}},"required":["id","riskIdOrKey"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}/risks/{riskIdOrKey}",
    executionParameters: [{"name":"id","in":"path"},{"name":"riskIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_UpdateRisk", {
    name: "Teams_UpdateRisk",
    description: `Update a team risk.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"riskId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["riskId","teamId","summary","statusId","categoryId","impactId","likelihoodId"],"properties":{"riskId":{"type":"string","format":"guid"},"teamId":{"type":"string","format":"guid","minLength":1},"summary":{"type":"string","maxLength":256,"minLength":1},"description":{"type":["string","null"],"maxLength":1024,"minLength":0},"statusId":{"type":"number","format":"int32"},"categoryId":{"type":"number","format":"int32"},"impactId":{"type":"number","format":"int32"},"likelihoodId":{"type":"number","format":"int32"},"assigneeId":{"type":["string","null"],"format":"guid"},"followUpDate":{"type":["string","null"],"format":"date"},"response":{"type":["string","null"],"maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","riskId","requestBody"]},
    method: "put",
    pathTemplate: "/api/organization/teams/{id}/risks/{riskId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"riskId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetOperatingModel", {
    name: "Teams_GetOperatingModel",
    description: `Get a specific operating model for a team.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"operatingModelId":{"type":"string","format":"guid"}},"required":["id","operatingModelId"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}/operating-models/{operatingModelId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"operatingModelId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_UpdateOperatingModel", {
    name: "Teams_UpdateOperatingModel",
    description: `Update an existing operating model for a team.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"operatingModelId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["methodology","sizingMethod"],"properties":{"methodology":{"description":"The methodology the team uses (e.g., Scrum, Kanban).","oneOf":[{"type":"string","description":"","x-enumNames":["Scrum","Kanban"],"enum":["Scrum","Kanban"]}]},"sizingMethod":{"description":"The sizing method the team uses (e.g., StoryPoints, Count).","oneOf":[{"type":"string","description":"","x-enumNames":["StoryPoints","Count"],"enum":["StoryPoints","Count"]}]}},"description":"The JSON request body."}},"required":["id","operatingModelId","requestBody"]},
    method: "put",
    pathTemplate: "/api/organization/teams/{id}/operating-models/{operatingModelId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"operatingModelId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_DeleteOperatingModel", {
    name: "Teams_DeleteOperatingModel",
    description: `Delete an operating model from a team.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"operatingModelId":{"type":"string","format":"guid"}},"required":["id","operatingModelId"]},
    method: "delete",
    pathTemplate: "/api/organization/teams/{id}/operating-models/{operatingModelId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"operatingModelId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetOperatingModels", {
    name: "Teams_GetOperatingModels",
    description: `Get the operating model history for a team.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}/operating-models",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_SetOperatingModel", {
    name: "Teams_SetOperatingModel",
    description: `Set a new operating model for a team.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["startDate","methodology","sizingMethod"],"properties":{"startDate":{"type":"string","description":"The start date for this operating model.","format":"date","minLength":1},"methodology":{"description":"The methodology the team uses (e.g., Scrum, Kanban).","oneOf":[{"type":"string","description":"","x-enumNames":["Scrum","Kanban"],"enum":["Scrum","Kanban"]}]},"sizingMethod":{"description":"The sizing method the team uses (e.g., StoryPoints, Count).","oneOf":[{"type":"string","description":"","x-enumNames":["StoryPoints","Count"],"enum":["StoryPoints","Count"]}]}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/organization/teams/{id}/operating-models",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetOperatingModelAsOf", {
    name: "Teams_GetOperatingModelAsOf",
    description: `Get the current operating model for a team, or the model effective on a specific date.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"asOfDate":{"type":["string","null"],"format":"date-time"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}/operating-models/as-of",
    executionParameters: [{"name":"id","in":"path"},{"name":"asOfDate","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetOperatingModelsForTeams", {
    name: "Teams_GetOperatingModelsForTeams",
    description: `Get operating models for multiple teams.`,
    inputSchema: {"type":"object","properties":{"teamIds":{"type":"array","items":{"type":"string","format":"guid"}},"asOfDate":{"type":["string","null"],"format":"date-time"}}},
    method: "get",
    pathTemplate: "/api/organization/teams/operating-models",
    executionParameters: [{"name":"teamIds","in":"query"},{"name":"asOfDate","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_HasEverBeenScrum", {
    name: "Teams_HasEverBeenScrum",
    description: `Check if a team has ever used the Scrum methodology.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}/has-ever-been-scrum",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetSprints", {
    name: "Teams_GetSprints",
    description: `Get the sprints for a team.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}/sprints",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetActiveSprint", {
    name: "Teams_GetActiveSprint",
    description: `Get the team's active sprint`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams/{id}/sprints/active",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Teams_GetFunctionalOrganizationChart", {
    name: "Teams_GetFunctionalOrganizationChart",
    description: `Get the functional organizaation chart for a given date.`,
    inputSchema: {"type":"object","properties":{"asOfDate":{"type":["string","null"],"format":"date-time"}}},
    method: "get",
    pathTemplate: "/api/organization/teams/functional-organization-chart",
    executionParameters: [{"name":"asOfDate","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_GetList", {
    name: "TeamsOfTeams_GetList",
    description: `Get a list of team of teams.`,
    inputSchema: {"type":"object","properties":{"includeInactive":{"type":"boolean","default":false}}},
    method: "get",
    pathTemplate: "/api/organization/teams-of-teams",
    executionParameters: [{"name":"includeInactive","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_Create", {
    name: "TeamsOfTeams_Create",
    description: `Create a team of teams.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","code","activeDate"],"properties":{"name":{"type":"string","description":"Gets the team name.","maxLength":128,"minLength":1},"code":{"type":"string","description":"Gets the code.","maxLength":10,"minLength":2},"description":{"type":["string","null"],"description":"Gets the team description.","maxLength":1024,"minLength":0},"activeDate":{"type":"string","description":"The active date for the team.","format":"date","minLength":1}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/organization/teams-of-teams",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_GetById", {
    name: "TeamsOfTeams_GetById",
    description: `Get team of teams details using the key.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"number","format":"int32"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams-of-teams/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_Update", {
    name: "TeamsOfTeams_Update",
    description: `Update a team of teams.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","code"],"properties":{"id":{"type":"string","format":"guid"},"name":{"type":"string","description":"Gets the team name.","maxLength":128,"minLength":1},"code":{"type":"string","description":"Gets the code.","maxLength":10,"minLength":1},"description":{"type":["string","null"],"description":"Gets the team description.","maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/organization/teams-of-teams/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_Deactivate", {
    name: "TeamsOfTeams_Deactivate",
    description: `Deactivate a team of teams.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","inactiveDate"],"properties":{"id":{"type":"string","format":"guid","minLength":1},"inactiveDate":{"type":"string","format":"date","minLength":1}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/organization/teams-of-teams/{id}/deactivate",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_GetTeamMemberships", {
    name: "TeamsOfTeams_GetTeamMemberships",
    description: `Get parent team memberships.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams-of-teams/{id}/team-memberships",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_AddTeamMembership", {
    name: "TeamsOfTeams_AddTeamMembership",
    description: `Add a parent team membership.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["teamId","parentTeamId","start"],"properties":{"teamId":{"type":"string","format":"guid","minLength":1},"parentTeamId":{"type":"string","format":"guid","minLength":1},"start":{"type":"string","format":"date"},"end":{"type":["string","null"],"format":"date"}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/organization/teams-of-teams/{id}/team-memberships",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_UpdateTeamMembership", {
    name: "TeamsOfTeams_UpdateTeamMembership",
    description: `Update a team membership.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"teamMembershipId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["teamId","teamMembershipId","start"],"properties":{"teamId":{"type":"string","format":"guid","minLength":1},"teamMembershipId":{"type":"string","format":"guid","minLength":1},"start":{"type":"string","format":"date"},"end":{"type":["string","null"],"format":"date"}},"description":"The JSON request body."}},"required":["id","teamMembershipId","requestBody"]},
    method: "put",
    pathTemplate: "/api/organization/teams-of-teams/{id}/team-memberships/{teamMembershipId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"teamMembershipId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_RemoveTeamMembership", {
    name: "TeamsOfTeams_RemoveTeamMembership",
    description: `Remove a parent team membership.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"teamMembershipId":{"type":"string","format":"guid"}},"required":["id","teamMembershipId"]},
    method: "delete",
    pathTemplate: "/api/organization/teams-of-teams/{id}/team-memberships/{teamMembershipId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"teamMembershipId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_GetRisks", {
    name: "TeamsOfTeams_GetRisks",
    description: `Get team risks.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"includeClosed":{"type":"boolean","default":false}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/organization/teams-of-teams/{id}/risks",
    executionParameters: [{"name":"id","in":"path"},{"name":"includeClosed","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_CreateRisk", {
    name: "TeamsOfTeams_CreateRisk",
    description: `Create a risk for a team of teams.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["teamId","summary","categoryId","impactId","likelihoodId"],"properties":{"teamId":{"type":"string","format":"guid","minLength":1},"summary":{"type":"string","maxLength":256,"minLength":1},"description":{"type":["string","null"],"maxLength":1024,"minLength":0},"categoryId":{"type":"number","format":"int32"},"impactId":{"type":"number","format":"int32"},"likelihoodId":{"type":"number","format":"int32"},"assigneeId":{"type":["string","null"],"format":"guid"},"followUpDate":{"type":["string","null"],"format":"date"},"response":{"type":["string","null"],"maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/organization/teams-of-teams/{id}/risks",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_GetRiskById", {
    name: "TeamsOfTeams_GetRiskById",
    description: `Get a team of teams risk by Id.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"riskIdOrKey":{"type":"string"}},"required":["id","riskIdOrKey"]},
    method: "get",
    pathTemplate: "/api/organization/teams-of-teams/{id}/risks/{riskIdOrKey}",
    executionParameters: [{"name":"id","in":"path"},{"name":"riskIdOrKey","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["TeamsOfTeams_UpdateRisk", {
    name: "TeamsOfTeams_UpdateRisk",
    description: `Update a team of teams risk.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"riskId":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["riskId","teamId","summary","statusId","categoryId","impactId","likelihoodId"],"properties":{"riskId":{"type":"string","format":"guid"},"teamId":{"type":"string","format":"guid","minLength":1},"summary":{"type":"string","maxLength":256,"minLength":1},"description":{"type":["string","null"],"maxLength":1024,"minLength":0},"statusId":{"type":"number","format":"int32"},"categoryId":{"type":"number","format":"int32"},"impactId":{"type":"number","format":"int32"},"likelihoodId":{"type":"number","format":"int32"},"assigneeId":{"type":["string","null"],"format":"guid"},"followUpDate":{"type":["string","null"],"format":"date"},"response":{"type":["string","null"],"maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","riskId","requestBody"]},
    method: "put",
    pathTemplate: "/api/organization/teams-of-teams/{id}/risks/{riskId}",
    executionParameters: [{"name":"id","in":"path"},{"name":"riskId","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Links_GetList", {
    name: "Links_GetList",
    description: `Get a list of links for a specific objectId.`,
    inputSchema: {"type":"object","properties":{"objectId":{"type":"string","format":"guid"}},"required":["objectId"]},
    method: "get",
    pathTemplate: "/api/links/{objectId}/list",
    executionParameters: [{"name":"objectId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Links_GetById", {
    name: "Links_GetById",
    description: `Get a link by id.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/links/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Links_Update", {
    name: "Links_Update",
    description: `Update a link.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","url"],"properties":{"id":{"type":"string","format":"guid","minLength":1},"name":{"type":"string","maxLength":128,"minLength":1},"url":{"type":"string","minLength":1}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/links/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Links_Delete", {
    name: "Links_Delete",
    description: `Delete a link.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/links/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Links_Create", {
    name: "Links_Create",
    description: `Create a link.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["objectId","name","url"],"properties":{"objectId":{"type":"string","format":"guid","minLength":1},"name":{"type":"string","maxLength":128,"minLength":1},"url":{"type":"string","minLength":1}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/links",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["HealthChecks_GetById", {
    name: "HealthChecks_GetById",
    description: `Get a health check by id.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/healthchecks/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["HealthChecks_Update", {
    name: "HealthChecks_Update",
    description: `Update a health report.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","contextId","statusId","expiration"],"properties":{"id":{"type":"string","format":"guid","minLength":1},"contextId":{"type":"number","format":"int32"},"statusId":{"type":"number","format":"int32"},"expiration":{"type":"string","format":"date-time","minLength":1},"note":{"type":["string","null"],"maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/healthchecks/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["HealthChecks_GetHealthReport", {
    name: "HealthChecks_GetHealthReport",
    description: `Get the health report for a specific objectId.`,
    inputSchema: {"type":"object","properties":{"objectId":{"type":"string","format":"guid"}},"required":["objectId"]},
    method: "get",
    pathTemplate: "/api/healthchecks/health-report/{objectId}",
    executionParameters: [{"name":"objectId","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["HealthChecks_Create", {
    name: "HealthChecks_Create",
    description: `Create a health report.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["objectId","contextId","statusId","expiration"],"properties":{"objectId":{"type":"string","format":"guid","minLength":1},"contextId":{"type":"number","format":"int32"},"statusId":{"type":"number","format":"int32"},"expiration":{"type":"string","format":"date-time","minLength":1},"note":{"type":["string","null"],"maxLength":1024,"minLength":0}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/healthchecks",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["HealthChecks_GetStatuses", {
    name: "HealthChecks_GetStatuses",
    description: `Get a list of health check statuses.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/healthchecks/statuses",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_GetList", {
    name: "AzureDevOpsBoardsConnections_GetList",
    description: `Get a list of all Azure DevOps Boards connections.`,
    inputSchema: {"type":"object","properties":{"includeDisabled":{"type":"boolean","default":false}}},
    method: "get",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections",
    executionParameters: [{"name":"includeDisabled","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_Create", {
    name: "AzureDevOpsBoardsConnections_Create",
    description: `Create an Azure DevOps Boards connection.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["name","organization","personalAccessToken"],"properties":{"name":{"type":"string","description":"The name of the connection.","maxLength":256,"minLength":1},"description":{"type":["string","null"],"description":"The description of the connection.","maxLength":1024,"minLength":0},"organization":{"type":"string","description":"The Azure DevOps Organization name.","maxLength":128,"minLength":1},"personalAccessToken":{"type":"string","description":"The personal access token that enables access to Azure DevOps Boards data.","maxLength":128,"minLength":1}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_GetById", {
    name: "AzureDevOpsBoardsConnections_GetById",
    description: `Get Azure DevOps Boards connection based on id.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_Update", {
    name: "AzureDevOpsBoardsConnections_Update",
    description: `Update an Azure DevOps Boards connection.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","name","organization","personalAccessToken"],"properties":{"id":{"type":"string","description":"Gets or sets the identifier.","format":"guid"},"name":{"type":"string","description":"Gets or sets the name of the connection.","maxLength":256,"minLength":1},"description":{"type":["string","null"],"description":"Gets or sets the description.","maxLength":1024,"minLength":0},"organization":{"type":"string","description":"Gets the organization.","maxLength":128,"minLength":1},"personalAccessToken":{"type":"string","description":"Gets the personal access token.","maxLength":128,"minLength":1}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "put",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_Delete", {
    name: "AzureDevOpsBoardsConnections_Delete",
    description: `Delete an Azure DevOps Boards connection.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "delete",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections/{id}",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_UpdateSyncState", {
    name: "AzureDevOpsBoardsConnections_UpdateSyncState",
    description: `Update an Azure DevOps Boards connection sync state.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"isSyncEnabled":{"type":"boolean"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections/{id}/sync-state",
    executionParameters: [{"name":"id","in":"path"},{"name":"isSyncEnabled","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_GetConnectionTeams", {
    name: "AzureDevOpsBoardsConnections_GetConnectionTeams",
    description: `Get Azure DevOps connection teams based on id.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"workspaceId":{"type":["string","null"],"format":"guid"}},"required":["id"]},
    method: "get",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections/{id}/teams",
    executionParameters: [{"name":"id","in":"path"},{"name":"workspaceId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_MapConnectionTeams", {
    name: "AzureDevOpsBoardsConnections_MapConnectionTeams",
    description: `Update Azure DevOps connection team mappings.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["connectionId","teamMappings"],"properties":{"connectionId":{"type":"string","description":"The unique identifer for the connection.","format":"guid","minLength":1},"teamMappings":{"type":"array","description":"List of team mappings.","items":{"type":"object","additionalProperties":false,"required":["workspaceId","teamId"],"properties":{"workspaceId":{"type":"string","description":"The unique identifier for the workspace in the Azure DevOps Boards system.","format":"guid","minLength":1},"teamId":{"type":"string","description":"The unique identifier for the team in the Azure DevOps Boards system.","format":"guid","minLength":1},"internalTeamId":{"type":["string","null"],"description":"The unique identifier for the team within Moda.","format":"guid"}}}}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections/{id}/teams",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_TestConfig", {
    name: "AzureDevOpsBoardsConnections_TestConfig",
    description: `Test Azure DevOps Boards connection configuration.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["organization","personalAccessToken"],"properties":{"organization":{"type":"string","description":"Gets the organization."},"personalAccessToken":{"type":"string","description":"Gets the personal access token."}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections/test",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_SyncOrganizationConfiguration", {
    name: "AzureDevOpsBoardsConnections_SyncOrganizationConfiguration",
    description: `Sync Azure DevOps processes and projects.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"}},"required":["id"]},
    method: "post",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections/{id}/sync-organization-configuration",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_InitWorkProcesssIntegration", {
    name: "AzureDevOpsBoardsConnections_InitWorkProcesssIntegration",
    description: `Initialize Azure DevOps project integration as a Moda workspace.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","externalId"],"properties":{"id":{"type":"string","description":"Connection Id.","format":"guid","minLength":1},"externalId":{"type":"string","description":"External identifier for the work process.","format":"guid","minLength":1}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections/{id}/init-work-process-integration",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["AzureDevOpsBoardsConnections_InitWorkspaceIntegration", {
    name: "AzureDevOpsBoardsConnections_InitWorkspaceIntegration",
    description: `Initialize Azure DevOps project integration as a Moda workspace.`,
    inputSchema: {"type":"object","properties":{"id":{"type":"string","format":"guid"},"requestBody":{"type":"object","additionalProperties":false,"required":["id","externalId","workspaceKey","workspaceName"],"properties":{"id":{"type":"string","description":"Connection Id.","format":"guid","minLength":1},"externalId":{"type":"string","description":"External identifier for the workspace.","format":"guid","minLength":1},"workspaceKey":{"type":"string","description":"The key for the workspace.","maxLength":20,"minLength":1},"workspaceName":{"type":"string","description":"The name for the workspace.","maxLength":64,"minLength":1},"externalViewWorkItemUrlTemplate":{"type":["string","null"],"description":"A url template for external work items.  This template plus the work item external id will create a url to view the work item in the external system.","maxLength":256,"minLength":0}},"description":"The JSON request body."}},"required":["id","requestBody"]},
    method: "post",
    pathTemplate: "/api/app-integrations/azure-devops-boards-connections/{id}/init-workspace-integration",
    executionParameters: [{"name":"id","in":"path"}],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["Connectors_GetList", {
    name: "Connectors_GetList",
    description: `Get a list of all connectors.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/app-integrations/connectors",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["BackgroundJobs_GetJobTypes", {
    name: "BackgroundJobs_GetJobTypes",
    description: `Get a list of all job types.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/admin/background-jobs/job-types",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["BackgroundJobs_GetRunningJobs", {
    name: "BackgroundJobs_GetRunningJobs",
    description: `Get a list of running jobs.`,
    inputSchema: {"type":"object","properties":{}},
    method: "get",
    pathTemplate: "/api/admin/background-jobs/running",
    executionParameters: [],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["BackgroundJobs_Run", {
    name: "BackgroundJobs_Run",
    description: `Run a background job.`,
    inputSchema: {"type":"object","properties":{"jobTypeId":{"type":"number","format":"int32"}}},
    method: "post",
    pathTemplate: "/api/admin/background-jobs/run",
    executionParameters: [{"name":"jobTypeId","in":"query"}],
    requestBodyContentType: undefined,
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
  ["BackgroundJobs_Create", {
    name: "BackgroundJobs_Create",
    description: `Create a recurring background job.`,
    inputSchema: {"type":"object","properties":{"requestBody":{"type":"object","additionalProperties":false,"required":["jobId","jobTypeId","cronExpression"],"properties":{"jobId":{"type":"string"},"jobTypeId":{"type":"number","format":"int32"},"cronExpression":{"type":"string"}},"description":"The JSON request body."}},"required":["requestBody"]},
    method: "post",
    pathTemplate: "/api/admin/background-jobs",
    executionParameters: [],
    requestBodyContentType: "application/json",
    securityRequirements: [{"Bearer":[]},{"ApiKey":[]}]
  }],
]);

/**
 * Security schemes from the OpenAPI spec
 */
const securitySchemes =   {
    "Bearer": {
      "type": "oauth2",
      "description": "OAuth2.0 Auth Code with PKCE",
      "flows": {
        "authorizationCode": {
          "authorizationUrl": "https://login.microsoftonline.com/f399216f-be6b-4062-8700-54952e44e7ef/oauth2/v2.0/authorize",
          "tokenUrl": "https://login.microsoftonline.com/f399216f-be6b-4062-8700-54952e44e7ef/oauth2/v2.0/token",
          "scopes": {
            "api://fdca5e6f-46a2-455c-b2f3-06a9a6877190/access_as_user": "access the api"
          }
        }
      }
    },
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
 * Type definition for cached OAuth tokens
 */
interface TokenCacheEntry {
    token: string;
    expiresAt: number;
}

/**
 * Declare global __oauthTokenCache property for TypeScript
 */
declare global {
    var __oauthTokenCache: Record<string, TokenCacheEntry> | undefined;
}

/**
 * Acquires an OAuth2 token using client credentials flow
 * 
 * @param schemeName Name of the security scheme
 * @param scheme OAuth2 security scheme
 * @returns Acquired token or null if unable to acquire
 */
async function acquireOAuth2Token(schemeName: string, scheme: any): Promise<string | null | undefined> {
    try {
        // Check if we have the necessary credentials
        const clientId = process.env[`OAUTH_CLIENT_ID_SCHEMENAME`];
        const clientSecret = process.env[`OAUTH_CLIENT_SECRET_SCHEMENAME`];
        const scopes = process.env[`OAUTH_SCOPES_SCHEMENAME`];
        
        if (!clientId || !clientSecret) {
            console.error(`Missing client credentials for OAuth2 scheme '${schemeName}'`);
            return null;
        }
        
        // Initialize token cache if needed
        if (typeof global.__oauthTokenCache === 'undefined') {
            global.__oauthTokenCache = {};
        }
        
        // Check if we have a cached token
        const cacheKey = `${schemeName}_${clientId}`;
        const cachedToken = global.__oauthTokenCache[cacheKey];
        const now = Date.now();
        
        if (cachedToken && cachedToken.expiresAt > now) {
            console.error(`Using cached OAuth2 token for '${schemeName}' (expires in ${Math.floor((cachedToken.expiresAt - now) / 1000)} seconds)`);
            return cachedToken.token;
        }
        
        // Determine token URL based on flow type
        let tokenUrl = '';
        if (scheme.flows?.clientCredentials?.tokenUrl) {
            tokenUrl = scheme.flows.clientCredentials.tokenUrl;
            console.error(`Using client credentials flow for '${schemeName}'`);
        } else if (scheme.flows?.password?.tokenUrl) {
            tokenUrl = scheme.flows.password.tokenUrl;
            console.error(`Using password flow for '${schemeName}'`);
        } else {
            console.error(`No supported OAuth2 flow found for '${schemeName}'`);
            return null;
        }
        
        // Prepare the token request
        let formData = new URLSearchParams();
        formData.append('grant_type', 'client_credentials');
        
        // Add scopes if specified
        if (scopes) {
            formData.append('scope', scopes);
        }
        
        console.error(`Requesting OAuth2 token from ${tokenUrl}`);
        
        // Make the token request
        const response = await axios({
            method: 'POST',
            url: tokenUrl,
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'Authorization': `Basic ${Buffer.from(`${clientId}:${clientSecret}`).toString('base64')}`
            },
            data: formData.toString()
        });
        
        // Process the response
        if (response.data?.access_token) {
            const token = response.data.access_token;
            const expiresIn = response.data.expires_in || 3600; // Default to 1 hour
            
            // Cache the token
            global.__oauthTokenCache[cacheKey] = {
                token,
                expiresAt: now + (expiresIn * 1000) - 60000 // Expire 1 minute early
            };
            
            console.error(`Successfully acquired OAuth2 token for '${schemeName}' (expires in ${expiresIn} seconds)`);
            return token;
        } else {
            console.error(`Failed to acquire OAuth2 token for '${schemeName}': No access_token in response`);
            return null;
        }
    } catch (error: unknown) {
        const errorMessage = error instanceof Error ? error.message : String(error);
        console.error(`Error acquiring OAuth2 token for '${schemeName}':`, errorMessage);
        return null;
    }
}


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


    // Apply security requirements if available
    // Security requirements use OR between array items and AND within each object
    const appliedSecurity = definition.securityRequirements?.find(req => {
        // Try each security requirement (combined with OR)
        return Object.entries(req).every(([schemeName, scopesArray]) => {
            const scheme = allSecuritySchemes[schemeName];
            if (!scheme) return false;
            
            // API Key security (header, query, cookie)
            if (scheme.type === 'apiKey') {
                return !!process.env[`API_KEY_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`];
            }
            
            // HTTP security (basic, bearer)
            if (scheme.type === 'http') {
                if (scheme.scheme?.toLowerCase() === 'bearer') {
                    return !!process.env[`BEARER_TOKEN_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`];
                }
                else if (scheme.scheme?.toLowerCase() === 'basic') {
                    return !!process.env[`BASIC_USERNAME_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`] && 
                           !!process.env[`BASIC_PASSWORD_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`];
                }
            }
            
            // OAuth2 security
            if (scheme.type === 'oauth2') {
                // Check for pre-existing token
                if (process.env[`OAUTH_TOKEN_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`]) {
                    return true;
                }
                
                // Check for client credentials for auto-acquisition
                if (process.env[`OAUTH_CLIENT_ID_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`] &&
                    process.env[`OAUTH_CLIENT_SECRET_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`]) {
                    // Verify we have a supported flow
                    if (scheme.flows?.clientCredentials || scheme.flows?.password) {
                        return true;
                    }
                }
                
                return false;
            }
            
            // OpenID Connect
            if (scheme.type === 'openIdConnect') {
                return !!process.env[`OPENID_TOKEN_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`];
            }
            
            return false;
        });
    });

    // If we found matching security scheme(s), apply them
    if (appliedSecurity) {
        // Apply each security scheme from this requirement (combined with AND)
        for (const [schemeName, scopesArray] of Object.entries(appliedSecurity)) {
            const scheme = allSecuritySchemes[schemeName];
            
            // API Key security
            if (scheme?.type === 'apiKey') {
                const apiKey = process.env[`API_KEY_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`];
                if (apiKey) {
                    if (scheme.in === 'header') {
                        headers[scheme.name.toLowerCase()] = apiKey;
                        console.error(`Applied API key '${schemeName}' in header '${scheme.name}'`);
                    }
                    else if (scheme.in === 'query') {
                        queryParams[scheme.name] = apiKey;
                        console.error(`Applied API key '${schemeName}' in query parameter '${scheme.name}'`);
                    }
                    else if (scheme.in === 'cookie') {
                        // Add the cookie, preserving other cookies if they exist
                        headers['cookie'] = `${scheme.name}=${apiKey}${headers['cookie'] ? `; ${headers['cookie']}` : ''}`;
                        console.error(`Applied API key '${schemeName}' in cookie '${scheme.name}'`);
                    }
                }
            } 
            // HTTP security (Bearer or Basic)
            else if (scheme?.type === 'http') {
                if (scheme.scheme?.toLowerCase() === 'bearer') {
                    const token = process.env[`BEARER_TOKEN_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`];
                    if (token) {
                        headers['authorization'] = `Bearer ${token}`;
                        console.error(`Applied Bearer token for '${schemeName}'`);
                    }
                } 
                else if (scheme.scheme?.toLowerCase() === 'basic') {
                    const username = process.env[`BASIC_USERNAME_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`];
                    const password = process.env[`BASIC_PASSWORD_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`];
                    if (username && password) {
                        headers['authorization'] = `Basic ${Buffer.from(`${username}:${password}`).toString('base64')}`;
                        console.error(`Applied Basic authentication for '${schemeName}'`);
                    }
                }
            }
            // OAuth2 security
            else if (scheme?.type === 'oauth2') {
                // First try to use a pre-provided token
                let token = process.env[`OAUTH_TOKEN_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`];
                
                // If no token but we have client credentials, try to acquire a token
                if (!token && (scheme.flows?.clientCredentials || scheme.flows?.password)) {
                    console.error(`Attempting to acquire OAuth token for '${schemeName}'`);
                    token = (await acquireOAuth2Token(schemeName, scheme)) ?? '';
                }
                
                // Apply token if available
                if (token) {
                    headers['authorization'] = `Bearer ${token}`;
                    console.error(`Applied OAuth2 token for '${schemeName}'`);
                    
                    // List the scopes that were requested, if any
                    const scopes = scopesArray as string[];
                    if (scopes && scopes.length > 0) {
                        console.error(`Requested scopes: ${scopes.join(', ')}`);
                    }
                }
            }
            // OpenID Connect
            else if (scheme?.type === 'openIdConnect') {
                const token = process.env[`OPENID_TOKEN_${schemeName.replace(/[^a-zA-Z0-9]/g, '_').toUpperCase()}`];
                if (token) {
                    headers['authorization'] = `Bearer ${token}`;
                    console.error(`Applied OpenID Connect token for '${schemeName}'`);
                    
                    // List the scopes that were requested, if any
                    const scopes = scopesArray as string[];
                    if (scopes && scopes.length > 0) {
                        console.error(`Requested scopes: ${scopes.join(', ')}`);
                    }
                }
            }
        }
    } 
    // Log warning if security is required but not available
    else if (definition.securityRequirements?.length > 0) {
        // First generate a more readable representation of the security requirements
        const securityRequirementsString = definition.securityRequirements
            .map(req => {
                const parts = Object.entries(req)
                    .map(([name, scopesArray]) => {
                        const scopes = scopesArray as string[];
                        if (scopes.length === 0) return name;
                        return `${name} (scopes: ${scopes.join(', ')})`;
                    })
                    .join(' AND ');
                return `[${parts}]`;
            })
            .join(' OR ');
            
        console.warn(`Tool '${toolName}' requires security: ${securityRequirementsString}, but no suitable credentials found.`);
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

/**
 * Type definition for JSON objects
 */
export type JsonObject = Record<string, any>;

/**
 * Interface for MCP Tool Definition
 */
export interface McpToolDefinition {
    name: string;
    description: string;
    inputSchema: any;
    method: string;
    pathTemplate: string;
    executionParameters: { name: string; in: string }[];
    requestBodyContentType?: string;
    securityRequirements: any[];
}

import type { McpToolDefinition } from '../types.js';
import { definitions as portfolios } from './portfolios.js';
import { definitions as programs } from './programs.js';
import { definitions as projects } from './projects.js';
import { definitions as roadmaps } from './roadmaps.js';
import { definitions as planningIntervals } from './planning-intervals.js';

export const toolDefinitionMap: Map<string, McpToolDefinition> = new Map([
  ...portfolios,
  ...programs,
  ...projects,
  ...roadmaps,
  ...planningIntervals,
]);

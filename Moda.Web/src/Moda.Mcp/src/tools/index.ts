import type { McpToolDefinition } from '../types.js';
import { definitions as portfolios } from './portfolios.js';
import { definitions as programs } from './programs.js';
import { definitions as projects } from './projects.js';
import { definitions as roadmaps } from './roadmaps.js';
import { definitions as planningIntervals } from './planning-intervals.js';
import { definitions as tasks } from './tasks.js';
import { definitions as teams } from './teams.js';
import { definitions as users } from './users.js';

export const toolDefinitionMap: Map<string, McpToolDefinition> = new Map([
  ...portfolios,
  ...programs,
  ...projects,
  ...roadmaps,
  ...planningIntervals,
  ...tasks,
  ...teams,
  ...users,
]);

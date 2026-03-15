import dotenv from 'dotenv';
dotenv.config();

import { createRequire } from 'module';
import { parseArgs } from 'util';

const _require = createRequire(import.meta.url);
const { version: packageVersion } = _require('../package.json') as { version: string };

const { values: cliArgs } = parseArgs({
  options: {
    'api-key':  { type: 'string' },
    'base-url': { type: 'string' },
  },
  strict: false, // ignore unknown args passed by MCP clients
});

export const SERVER_NAME    = 'moda-mcp';
export const SERVER_VERSION = packageVersion;
export const API_BASE_URL   = (cliArgs['base-url'] as string | undefined) || process.env.MODA_API_BASE_URL || '';
export const MODA_API_KEY   = (cliArgs['api-key']  as string | undefined) || process.env.MODA_API_KEY  || '';

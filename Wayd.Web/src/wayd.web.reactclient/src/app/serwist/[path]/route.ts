import { readFileSync } from 'node:fs'
import { join } from 'node:path'
import { createSerwistRoute } from '@serwist/turbopack'

// Use the Next.js build ID for stable cache revisions across cold starts.
// Falls back to a UUID only during development when no build exists.
let revision: string
try {
  revision = readFileSync(
    join(process.cwd(), '.next', 'BUILD_ID'),
    'utf-8',
  ).trim()
} catch {
  revision = crypto.randomUUID()
}

export const { dynamic, dynamicParams, revalidate, generateStaticParams, GET } =
  createSerwistRoute({
    additionalPrecacheEntries: [{ url: '/', revision }],
    swSrc: 'src/app/sw.ts',
    useNativeEsbuild: true,
  })


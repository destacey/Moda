FROM node:18-alpine AS base

# Install dependencies only when needed
FROM base AS deps
# Check https://github.com/nodejs/docker-node/tree/b4117f9333da4138b03a546ec926ef50a31506c3#nodealpine to understand why libc6-compat might be needed.
RUN apk add --no-cache libc6-compat
WORKDIR /app

# Install dependencies based on the preferred package manager
COPY Moda.Web/src/moda.web.reactclient/package.json Moda.Web/src/moda.web.reactclient/package-lock.json* ./
RUN npm ci

# Rebuild the source code only when needed
FROM base AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY Moda.Web/src/moda.web.reactclient/ .

# Next.js collects completely anonymous telemetry data about general usage.
# Learn more here: https://nextjs.org/telemetry
# Uncomment the following line in case you want to disable telemetry during the build.
ENV NEXT_TELEMETRY_DISABLED 1

#RUN yarn build

# If using npm comment out above and use below instead
RUN NEXT_PUBLIC_API_BASE_URL=APP_NEXT_PUBLIC_API_BASE_URL \
    NEXT_PUBLIC_API_SCOPE=APP_NEXT_PUBLIC_API_SCOPE \
    NEXT_PUBLIC_AZURE_AD_CLIENT_ID=APP_NEXT_PUBLIC_AZURE_AD_CLIENT_ID \
    NEXT_PUBLIC_AZURE_AD_TENANT_ID=APP_NEXT_PUBLIC_AZURE_AD_TENANT_ID \
    NEXT_PUBLIC_MICROSOFT_LOGON_AUTHORITY=APP_NEXT_PUBLIC_MICROSOFT_LOGON_AUTHORITY \
    npm run build

# Production image, copy all the files and run next
FROM base AS runner
WORKDIR /app

ENV NODE_ENV production
# Uncomment the following line in case you want to disable telemetry during runtime.
ENV NEXT_TELEMETRY_DISABLED 1

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder /app/public ./public
COPY --from=builder /app/entrypoint.sh ./
RUN chown nextjs:nodejs /app/entrypoint.sh

# Automatically leverage output traces to reduce image size
# https://nextjs.org/docs/advanced-features/output-file-tracing
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static
RUN chown -R nextjs:nodejs /app/.next

USER nextjs

EXPOSE 3000

ENV PORT 3000

ENTRYPOINT [ "/app/entrypoint.sh" ]

CMD ["node", "server.js"]
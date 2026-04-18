#!/bin/sh

echo "Check that we have env vars"
test -n "$NEXT_PUBLIC_API_BASE_URL"
test -n "$NEXT_PUBLIC_AZURE_AD_CLIENT_ID"
test -n "$NEXT_PUBLIC_AZURE_AD_TENANT_ID"
test -n "$NEXT_PUBLIC_MICROSOFT_LOGON_AUTHORITY"
test -n "$NEXT_PUBLIC_API_SCOPE"


find /app/.next \( -type d -name .git -prune \) -o -type f -print0 | xargs -0 sed -i "s#APP_NEXT_PUBLIC_API_BASE_URL#$NEXT_PUBLIC_API_BASE_URL#g"
find /app/.next \( -type d -name .git -prune \) -o -type f -print0 | xargs -0 sed -i "s#APP_NEXT_PUBLIC_AZURE_AD_CLIENT_ID#$NEXT_PUBLIC_AZURE_AD_CLIENT_ID#g"
find /app/.next \( -type d -name .git -prune \) -o -type f -print0 | xargs -0 sed -i "s#APP_NEXT_PUBLIC_AZURE_AD_TENANT_ID#$NEXT_PUBLIC_AZURE_AD_TENANT_ID#g"
find /app/.next \( -type d -name .git -prune \) -o -type f -print0 | xargs -0 sed -i "s#APP_NEXT_PUBLIC_MICROSOFT_LOGON_AUTHORITY#$NEXT_PUBLIC_MICROSOFT_LOGON_AUTHORITY#g"
find /app/.next \( -type d -name .git -prune \) -o -type f -print0 | xargs -0 sed -i "s#APP_NEXT_PUBLIC_API_SCOPE#$NEXT_PUBLIC_API_SCOPE#g"

echo "Starting Nextjs"
exec "$@"
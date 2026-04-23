'use client'

import { useEffect } from 'react'

/**
 * One-shot cleanup for browsers carrying pre-PR-3.2 state.
 *
 * Without this, returning users are stuck in a loop: the old service worker
 * serves the cached old app shell, which tries to hit `moda-api.*`/permissions
 * (an endpoint removed in PR 3.2) with MSAL-direct-to-API tokens, gets 401,
 * retries forever. Clearing site data manually works; users shouldn't have to.
 *
 * On first load per browser after this ships:
 *   1. Unregister every service worker (nukes the stale cached app shell).
 *   2. Delete every Cache Storage entry (nukes stale chunks + runtime-cached
 *      API responses the old SW populated).
 *   3. Remove pre-rename `moda.local.*` storage entries — harmless noise once
 *      `wayd.local.*` is the active key prefix, but worth clearing so DevTools
 *      doesn't suggest two parallel sessions.
 *   4. Mark the cleanup done (localStorage flag) and hard-reload so the fresh
 *      app boots without the old SW intercepting.
 *
 * Gated by a per-release marker so it runs at most once per browser per
 * release. Subsequent releases bump `RELEASE_MARKER_VALUE` to force another
 * pass; the marker key itself stays stable so we don't leak flags.
 */
const RELEASE_MARKER_KEY = 'wayd.release.cleanup'
const RELEASE_MARKER_VALUE = 'pr-3.2'

const LEGACY_STORAGE_KEYS = [
  'moda.local.token',
  'moda.local.refreshToken',
  'moda.local.tokenExpiry',
  'moda.local.mustChangePassword',
  'moda.local.rememberMe',
]

interface ReleaseCleanupProps {
  /**
   * Injection point for tests. Production path always uses `window.location.reload`;
   * tests pass a jest.fn so the test environment doesn't need to mock jsdom's
   * non-configurable `location`.
   */
  onReload?: () => void
}

export default function ReleaseCleanup({ onReload }: ReleaseCleanupProps = {}) {
  useEffect(() => {
    if (typeof window === 'undefined') return
    // The marker flip happens before the reload, so any remount — StrictMode
    // double-invoke in dev, or a revisit later in the same tab — is a no-op.
    if (localStorage.getItem(RELEASE_MARKER_KEY) === RELEASE_MARKER_VALUE) return

    const run = async () => {
      try {
        if ('serviceWorker' in navigator) {
          const registrations = await navigator.serviceWorker.getRegistrations()
          await Promise.all(registrations.map((r) => r.unregister()))
        }
        if ('caches' in window) {
          const keys = await caches.keys()
          await Promise.all(keys.map((k) => caches.delete(k)))
        }
        for (const key of LEGACY_STORAGE_KEYS) {
          localStorage.removeItem(key)
          sessionStorage.removeItem(key)
        }
      } catch (e) {
        console.warn(
          '[ReleaseCleanup] cleanup failed, continuing so the user is not stuck retrying forever.',
          e,
        )
      } finally {
        localStorage.setItem(RELEASE_MARKER_KEY, RELEASE_MARKER_VALUE)
        // Full reload so the new SW (if any) registers clean and the app
        // boots without the old bundle's handlers still subscribed.
        if (onReload) {
          onReload()
        } else {
          window.location.reload()
        }
      }
    }

    run()
  }, [onReload])

  return null
}

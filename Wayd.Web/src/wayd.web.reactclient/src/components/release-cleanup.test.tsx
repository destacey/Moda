import { render, waitFor } from '@testing-library/react'
import '@testing-library/jest-dom'
import ReleaseCleanup from './release-cleanup'

const RELEASE_MARKER_KEY = 'wayd.release.cleanup'
const RELEASE_MARKER_VALUE = 'pr-3.2.2'

// --- In-memory localStorage/sessionStorage mocks ---
//
// jest.setup.ts installs a global localStorage mock whose methods are jest.fn()
// no-ops — fine for most tests that only assert call shape, but inadequate
// here because ReleaseCleanup's branching depends on reading a key it wrote
// moments earlier. Swap in a real in-memory store for this test file.

function createInMemoryStorage() {
  const store = new Map<string, string>()
  return {
    getItem: jest.fn((k: string) => (store.has(k) ? store.get(k)! : null)),
    setItem: jest.fn((k: string, v: string) => {
      store.set(k, v)
    }),
    removeItem: jest.fn((k: string) => {
      store.delete(k)
    }),
    clear: jest.fn(() => {
      store.clear()
    }),
    get length() {
      return store.size
    },
    key: jest.fn((i: number) => Array.from(store.keys())[i] ?? null),
  }
}

// --- Mocks for browser APIs not in jsdom ---

function mockServiceWorker(registrations: Array<{ unregister: jest.Mock }> = []) {
  Object.defineProperty(navigator, 'serviceWorker', {
    configurable: true,
    value: {
      getRegistrations: jest.fn().mockResolvedValue(registrations),
    },
  })
}

function removeServiceWorker() {
  delete (navigator as unknown as { serviceWorker?: unknown }).serviceWorker
}

function mockCaches(keys: string[] = []) {
  const deleteFn = jest.fn().mockResolvedValue(true)
  Object.defineProperty(window, 'caches', {
    configurable: true,
    value: {
      keys: jest.fn().mockResolvedValue(keys),
      delete: deleteFn,
    },
  })
  return deleteFn
}

function removeCaches() {
  delete (window as unknown as { caches?: unknown }).caches
}

let mockLocalStorage: ReturnType<typeof createInMemoryStorage>
let mockSessionStorage: ReturnType<typeof createInMemoryStorage>

beforeEach(() => {
  mockLocalStorage = createInMemoryStorage()
  mockSessionStorage = createInMemoryStorage()
  Object.defineProperty(window, 'localStorage', {
    configurable: true,
    value: mockLocalStorage,
  })
  Object.defineProperty(window, 'sessionStorage', {
    configurable: true,
    value: mockSessionStorage,
  })
  jest.clearAllMocks()
})

afterEach(() => {
  removeServiceWorker()
  removeCaches()
})

describe('ReleaseCleanup', () => {
  it('unregisters service workers, clears caches, and reloads once on first load', async () => {
    const reload = jest.fn()
    const swUnregister = jest.fn().mockResolvedValue(true)
    mockServiceWorker([{ unregister: swUnregister }])
    const cacheDelete = mockCaches(['old-cache-1', 'old-cache-2'])

    render(<ReleaseCleanup onReload={reload} />)

    await waitFor(() => expect(reload).toHaveBeenCalled())

    expect(swUnregister).toHaveBeenCalledTimes(1)
    expect(cacheDelete).toHaveBeenCalledWith('old-cache-1')
    expect(cacheDelete).toHaveBeenCalledWith('old-cache-2')
    expect(mockLocalStorage.getItem(RELEASE_MARKER_KEY)).toBe(RELEASE_MARKER_VALUE)
  })

  it('removes legacy moda.local.* storage entries', async () => {
    const reload = jest.fn()
    mockServiceWorker()
    mockCaches()
    mockLocalStorage.setItem('moda.local.token', 'stale-token')
    mockLocalStorage.setItem('moda.local.refreshToken', 'stale-refresh')
    mockSessionStorage.setItem('moda.local.tokenExpiry', 'stale-expiry')

    render(<ReleaseCleanup onReload={reload} />)

    await waitFor(() => expect(reload).toHaveBeenCalled())

    expect(mockLocalStorage.getItem('moda.local.token')).toBeNull()
    expect(mockLocalStorage.getItem('moda.local.refreshToken')).toBeNull()
    expect(mockSessionStorage.getItem('moda.local.tokenExpiry')).toBeNull()
  })

  it('migrates modaTheme → appTheme, preserving the user preference', async () => {
    const reload = jest.fn()
    mockServiceWorker()
    mockCaches()
    mockLocalStorage.setItem('modaTheme', '"dark"')

    render(<ReleaseCleanup onReload={reload} />)

    await waitFor(() => expect(reload).toHaveBeenCalled())

    expect(mockLocalStorage.getItem('modaTheme')).toBeNull()
    expect(mockLocalStorage.getItem('appTheme')).toBe('"dark"')
  })

  it('migrates modaMenuCollapsed → appMenuCollapsed, preserving the user preference', async () => {
    const reload = jest.fn()
    mockServiceWorker()
    mockCaches()
    mockLocalStorage.setItem('modaMenuCollapsed', 'false')

    render(<ReleaseCleanup onReload={reload} />)

    await waitFor(() => expect(reload).toHaveBeenCalled())

    expect(mockLocalStorage.getItem('modaMenuCollapsed')).toBeNull()
    expect(mockLocalStorage.getItem('appMenuCollapsed')).toBe('false')
  })

  it('does not overwrite an already-populated destination key during migration', async () => {
    // Edge case: another device / tab already seeded the new key.
    // Preserve the new value, drop the old one.
    const reload = jest.fn()
    mockServiceWorker()
    mockCaches()
    mockLocalStorage.setItem('modaTheme', '"light"') // stale old value
    mockLocalStorage.setItem('appTheme', '"dark"') // fresh new value

    render(<ReleaseCleanup onReload={reload} />)

    await waitFor(() => expect(reload).toHaveBeenCalled())

    expect(mockLocalStorage.getItem('modaTheme')).toBeNull()
    expect(mockLocalStorage.getItem('appTheme')).toBe('"dark"')
  })

  it('leaves the destination key alone when the source key is absent', async () => {
    const reload = jest.fn()
    mockServiceWorker()
    mockCaches()
    // No modaTheme; user has never touched the theme on this browser.

    render(<ReleaseCleanup onReload={reload} />)

    await waitFor(() => expect(reload).toHaveBeenCalled())

    expect(mockLocalStorage.getItem('appTheme')).toBeNull()
  })

  it('is a no-op when the marker is already set (already cleaned this release)', async () => {
    mockLocalStorage.setItem(RELEASE_MARKER_KEY, RELEASE_MARKER_VALUE)
    const reload = jest.fn()
    const getRegistrations = jest.fn().mockResolvedValue([])
    Object.defineProperty(navigator, 'serviceWorker', {
      configurable: true,
      value: { getRegistrations },
    })

    render(<ReleaseCleanup onReload={reload} />)

    // Give any microtasks a tick to settle, but nothing should have fired.
    await new Promise((r) => setTimeout(r, 10))

    expect(reload).not.toHaveBeenCalled()
    expect(getRegistrations).not.toHaveBeenCalled()
  })

  it('still marks cleanup done and reloads if service worker APIs are unavailable', async () => {
    // No navigator.serviceWorker, no window.caches — older browsers, or SSR-
    // initialized environments that never got browser-only globals. The
    // cleanup should still mark itself complete and reload so the user isn't
    // permanently stuck.
    const reload = jest.fn()

    render(<ReleaseCleanup onReload={reload} />)

    await waitFor(() => expect(reload).toHaveBeenCalled())
    expect(mockLocalStorage.getItem(RELEASE_MARKER_KEY)).toBe(RELEASE_MARKER_VALUE)
  })

  it('recovers when service worker unregister rejects — still marks + reloads', async () => {
    const reload = jest.fn()
    const swUnregister = jest.fn().mockRejectedValue(new Error('sw failure'))
    mockServiceWorker([{ unregister: swUnregister }])
    mockCaches([])
    // Swallow the expected console.warn so test output stays clean.
    const warnSpy = jest.spyOn(console, 'warn').mockImplementation(() => {})

    render(<ReleaseCleanup onReload={reload} />)

    await waitFor(() => expect(reload).toHaveBeenCalled())
    expect(mockLocalStorage.getItem(RELEASE_MARKER_KEY)).toBe(RELEASE_MARKER_VALUE)
    expect(warnSpy).toHaveBeenCalled()
    warnSpy.mockRestore()
  })
})

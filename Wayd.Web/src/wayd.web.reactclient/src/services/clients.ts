import axios from 'axios'
import {
  AuthClient,
  BackgroundJobsClient,
  EmployeesClient,
  HealthChecksClient,
  LinksClient,
  PermissionsClient,
  ProfileClient,
  PlanningIntervalsClient,
  RisksClient,
  RolesClient,
  TeamsClient,
  TeamsOfTeamsClient,
  UsersClient,
  WorkStatusesClient,
  WorkTypesClient,
  WorkProcessesClient,
  WorkspacesClient,
  WorkTypeLevelsClient,
  WorkTypeTiersClient,
  RoadmapsClient,
  StrategicThemesClient,
  PortfoliosClient,
  ProgramsClient,
  ExpenditureCategoriesClient,
  ProjectsClient,
  ProjectTasksClient,
  StrategicInitiativesClient,
  ProjectLifecyclesClient,
  SprintsClient,
  EstimationScalesClient,
  PokerSessionsClient,
  ConnectionsClient,
  AzureDevOpsConnectionsClient,
  FeatureFlagsClient,
  PersonalAccessTokensClient,
  SearchClient,
  TokenResponse,
} from './wayd-api'

const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL

// Storage keys for the Wayd JWT session. The same keys are used for every
// login provider — local JWT and Entra-exchanged sessions are indistinguishable
// at the storage layer.
export const AUTH_TOKEN_KEY = 'wayd.local.token'
export const AUTH_REFRESH_TOKEN_KEY = 'wayd.local.refreshToken'
export const AUTH_TOKEN_EXPIRY_KEY = 'wayd.local.tokenExpiry'
export const AUTH_MUST_CHANGE_PASSWORD_KEY = 'wayd.local.mustChangePassword'
const AUTH_REMEMBER_KEY = 'wayd.local.rememberMe'

/**
 * Returns the storage backend for the auth session.
 * The routing flag in localStorage determines whether tokens live in
 * localStorage (remember me) or sessionStorage (ephemeral session).
 */
export function getAuthStorage(): Storage {
  if (typeof window === 'undefined') {
    throw new Error('getAuthStorage() cannot be called during SSR')
  }
  return localStorage.getItem(AUTH_REMEMBER_KEY) === 'false'
    ? sessionStorage
    : localStorage
}

/** Sets the remember-me routing flag (always in localStorage). */
export function setRememberMe(remember: boolean): void {
  localStorage.setItem(AUTH_REMEMBER_KEY, remember ? 'true' : 'false')
}

export function getAuthToken(): string | null {
  if (typeof window === 'undefined') return null
  return getAuthStorage().getItem(AUTH_TOKEN_KEY)
}

export function getAuthRefreshToken(): string | null {
  if (typeof window === 'undefined') return null
  return getAuthStorage().getItem(AUTH_REFRESH_TOKEN_KEY)
}

export function isAuthActive(): boolean {
  if (typeof window === 'undefined') return false
  // Check both storages since we don't know which was used until we read the flag.
  return !!(
    localStorage.getItem(AUTH_TOKEN_KEY) ||
    sessionStorage.getItem(AUTH_TOKEN_KEY)
  )
}

export function clearAuth(): void {
  // Clear from both storages to ensure clean state.
  for (const storage of [localStorage, sessionStorage]) {
    storage.removeItem(AUTH_TOKEN_KEY)
    storage.removeItem(AUTH_REFRESH_TOKEN_KEY)
    storage.removeItem(AUTH_TOKEN_EXPIRY_KEY)
    storage.removeItem(AUTH_MUST_CHANGE_PASSWORD_KEY)
  }
  localStorage.removeItem(AUTH_REMEMBER_KEY)
}

/** Persists a token response from /api/auth/login, /exchange, or /refresh-token. */
export function storeAuth(tokenResponse: {
  token: string
  refreshToken: string
  tokenExpiresAt: Date | string
  mustChangePassword: boolean
}): void {
  const storage = getAuthStorage()
  storage.setItem(AUTH_TOKEN_KEY, tokenResponse.token)
  storage.setItem(AUTH_REFRESH_TOKEN_KEY, tokenResponse.refreshToken)
  storage.setItem(
    AUTH_TOKEN_EXPIRY_KEY,
    new Date(tokenResponse.tokenExpiresAt).toISOString(),
  )
  if (tokenResponse.mustChangePassword) {
    storage.setItem(AUTH_MUST_CHANGE_PASSWORD_KEY, 'true')
  } else {
    storage.removeItem(AUTH_MUST_CHANGE_PASSWORD_KEY)
  }
}

// Unauthenticated axios client for login/refresh/exchange (no token interceptors).
const unauthAxiosClient = axios.create({
  baseURL: apiUrl,
  timeout: 30000,
})

// Auth client uses unauthenticated axios — login, refresh, exchange, and
// getProviders are all [AllowAnonymous] on the backend.
export const getAuthClient = () => new AuthClient('', unauthAxiosClient)

const axiosClient = axios.create({
  baseURL: apiUrl,
  timeout: 30000, // 30 second timeout
})

// Single-flight guard for refresh. When multiple requests 401 concurrently
// (typical after an idle period: every in-flight call fails at once), they all
// share one refresh call instead of each minting a new refresh token and racing
// the rotation. Without this, whichever response wrote to storage last wins and
// the others' rotated refresh tokens are orphaned — which breaks as soon as the
// backend adopts one-shot refresh semantics.
//
// Returns null when there are no tokens to refresh with. Callers distinguish
// this from a refresh-attempt failure: no tokens means "not a session" (don't
// wipe state, just reject), whereas a failure means "session invalidated by
// the server" (clear it).
let inFlightRefresh: Promise<TokenResponse> | null = null

function refreshOnce(): Promise<TokenResponse> | null {
  if (inFlightRefresh) return inFlightRefresh

  const currentToken = getAuthToken()
  const refreshToken = getAuthRefreshToken()
  if (!currentToken || !refreshToken) return null

  inFlightRefresh = getAuthClient()
    .refreshToken({ token: currentToken, refreshToken })
    .then((tokenResponse) => {
      storeAuth(tokenResponse)
      return tokenResponse
    })
    .finally(() => {
      inFlightRefresh = null
    })

  return inFlightRefresh
}

// Response interceptor: automatic Wayd refresh-token retry on 401.
// One path for every provider because every provider ends up with the same
// Wayd JWT shape in storage after login/exchange. No MSAL involvement — the
// Wayd refresh token is the sole mechanism for extending the session.
axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (
      error.response?.status === 401 &&
      originalRequest &&
      !(originalRequest as any)._retry
    ) {
      ;(originalRequest as any)._retry = true

      const refresh = refreshOnce()
      if (refresh) {
        try {
          const tokenResponse = await refresh
          originalRequest.headers = originalRequest.headers || {}
          originalRequest.headers.Authorization = `Bearer ${tokenResponse.token}`
          return axiosClient(originalRequest)
        } catch (refreshError) {
          // Server rejected the refresh — the session is invalid. Wipe storage
          // and force a hard navigation to /login. Without the redirect, the
          // app keeps rendering with no token: every subsequent request 401s,
          // pages load missing data, and the user only escapes by manually
          // refreshing. AuthGate reads isAuthActive() once and won't re-check
          // on its own, so the navigation is the trigger that resets it.
          console.error('Token refresh on 401 failed:', refreshError)
          clearAuth()
          if (typeof window !== 'undefined') {
            window.location.href = '/login'
          }
        }
      }
      // No tokens to refresh with: the caller was already unauthenticated.
      // Fall through to the default reject without touching storage — we
      // don't want to clobber the remember-me preference on every anonymous
      // 401 a page might trigger.
    }

    console.error('API Error:', error.message, error.config?.url)
    return Promise.reject(error)
  },
)

// Request interceptor: attach the Wayd JWT to outgoing requests.
axiosClient.interceptors.request.use(
  (config) => {
    const token = getAuthToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error),
)

// API Clients
export const getConnectionsClient = () => new ConnectionsClient('', axiosClient)

export const getAzureDevOpsConnectionsClient = () =>
  new AzureDevOpsConnectionsClient('', axiosClient)

export const getBackgroundJobsClient = () =>
  new BackgroundJobsClient('', axiosClient)

export const getHealthChecksClient = () =>
  new HealthChecksClient('', axiosClient)

export const getLinksClient = () => new LinksClient('', axiosClient)

// ORGANIZATION
export const getEmployeesClient = () => new EmployeesClient('', axiosClient)
export const getTeamsClient = () => new TeamsClient('', axiosClient)
export const getTeamsOfTeamsClient = () =>
  new TeamsOfTeamsClient('', axiosClient)

// PLANNING
export const getPlanningIntervalsClient = () =>
  new PlanningIntervalsClient('', axiosClient)
export const getRisksClient = () => new RisksClient('', axiosClient)
export const getRoadmapsClient = () => new RoadmapsClient('', axiosClient)
export const getSprintsClient = () => new SprintsClient('', axiosClient)
export const getEstimationScalesClient = () =>
  new EstimationScalesClient('', axiosClient)
export const getPokerSessionsClient = () =>
  new PokerSessionsClient('', axiosClient)

// PPM
export const getExpenditureCategoriesClient = () =>
  new ExpenditureCategoriesClient('', axiosClient)
export const getPortfoliosClient = () => new PortfoliosClient('', axiosClient)
export const getProgramsClient = () => new ProgramsClient('', axiosClient)
export const getProjectsClient = () => new ProjectsClient('', axiosClient)
export const getProjectTasksClient = () =>
  new ProjectTasksClient('', axiosClient)
export const getStrategicInitiativesClient = () =>
  new StrategicInitiativesClient('', axiosClient)
export const getProjectLifecyclesClient = () =>
  new ProjectLifecyclesClient('', axiosClient)

// SEARCH
export const getSearchClient = () => new SearchClient('', axiosClient)

// STRATEGIC MANAGEMENT
export const getStrategicThemesClient = () =>
  new StrategicThemesClient('', axiosClient)

// WORK MANAGEMENT
export const getWorkProcessesClient = () =>
  new WorkProcessesClient('', axiosClient)
export const getWorkspacesClient = () => new WorkspacesClient('', axiosClient)
export const getWorkStatusesClient = () =>
  new WorkStatusesClient('', axiosClient)
export const getWorkTypeLevelsClient = () =>
  new WorkTypeLevelsClient('', axiosClient)
export const getWorkTypesClient = () => new WorkTypesClient('', axiosClient)
export const getWorkTypeTiersClient = () =>
  new WorkTypeTiersClient('', axiosClient)

// FEATURE MANAGEMENT
export const getFeatureFlagsClient = () =>
  new FeatureFlagsClient('', axiosClient)

// USER MANAGEMENT
export const getPermissionsClient = () => new PermissionsClient('', axiosClient)
export const getProfileClient = () => new ProfileClient('', axiosClient)
export const getRolesClient = () => new RolesClient('', axiosClient)
export const getUsersClient = () => new UsersClient('', axiosClient)
export const getPersonalAccessTokensClient = () =>
  new PersonalAccessTokensClient('', axiosClient)

/**
 * Performs an authenticated fetch request.
 * Use this for custom API calls that need authentication but can't use the
 * generated clients (e.g., JSON Patch endpoints that NSwag doesn't generate).
 */
export async function authenticatedFetch(
  url: string,
  options: RequestInit = {},
): Promise<Response> {
  const token = getAuthToken()

  const headers = new Headers(options.headers)
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }
  if (!headers.has('Accept')) {
    headers.set('Accept', 'application/json')
  }

  const fullUrl = url.startsWith('http') ? url : `${apiUrl}${url}`

  return fetch(fullUrl, {
    ...options,
    headers,
    credentials: options.credentials || 'include',
  })
}

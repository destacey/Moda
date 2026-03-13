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
  SprintsClient,
  EstimationScalesClient,
  PokerSessionsClient,
  ConnectionsClient,
  AzureDevOpsConnectionsClient,
  FeatureFlagsClient,
  PersonalAccessTokensClient,
} from './moda-api'
import { tokenRequest } from '@/auth-config'
import { InteractionRequiredAuthError } from '@azure/msal-browser'
import { msalInstance } from '../components/contexts/auth'

const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL

// Local auth storage keys
export const LOCAL_AUTH_TOKEN_KEY = 'moda.local.token'
export const LOCAL_AUTH_REFRESH_TOKEN_KEY = 'moda.local.refreshToken'
export const LOCAL_AUTH_TOKEN_EXPIRY_KEY = 'moda.local.tokenExpiry'
export const LOCAL_AUTH_MUST_CHANGE_PASSWORD_KEY = 'moda.local.mustChangePassword'
const LOCAL_AUTH_REMEMBER_KEY = 'moda.local.rememberMe'

/**
 * Returns the storage backend for local auth tokens.
 * The routing flag in localStorage determines whether tokens live in
 * localStorage (remember me) or sessionStorage (ephemeral session).
 */
export function getAuthStorage(): Storage {
  if (typeof window === 'undefined') {
    throw new Error('getAuthStorage() cannot be called during SSR')
  }
  return localStorage.getItem(LOCAL_AUTH_REMEMBER_KEY) === 'false'
    ? sessionStorage
    : localStorage
}

/** Sets the remember-me routing flag (always in localStorage). */
export function setRememberMe(remember: boolean): void {
  localStorage.setItem(LOCAL_AUTH_REMEMBER_KEY, remember ? 'true' : 'false')
}

export function getLocalAuthToken(): string | null {
  if (typeof window === 'undefined') return null
  return getAuthStorage().getItem(LOCAL_AUTH_TOKEN_KEY)
}

export function isLocalAuthActive(): boolean {
  if (typeof window === 'undefined') return false
  // Check both storages since we don't know which was used until we read the flag
  return !!(
    localStorage.getItem(LOCAL_AUTH_TOKEN_KEY) ||
    sessionStorage.getItem(LOCAL_AUTH_TOKEN_KEY)
  )
}

export function clearLocalAuth(): void {
  // Clear from both storages to ensure clean state
  for (const storage of [localStorage, sessionStorage]) {
    storage.removeItem(LOCAL_AUTH_TOKEN_KEY)
    storage.removeItem(LOCAL_AUTH_REFRESH_TOKEN_KEY)
    storage.removeItem(LOCAL_AUTH_TOKEN_EXPIRY_KEY)
    storage.removeItem(LOCAL_AUTH_MUST_CHANGE_PASSWORD_KEY)
  }
  localStorage.removeItem(LOCAL_AUTH_REMEMBER_KEY)
}

// Unauthenticated axios client for login/refresh (no token interceptors)
const unauthAxiosClient = axios.create({
  baseURL: apiUrl,
  timeout: 30000,
  transformResponse: (data) => data,
})

// Auth client uses unauthenticated axios (login endpoints are [AllowAnonymous])
export const getAuthClient = () => new AuthClient('', unauthAxiosClient)

const axiosClient = axios.create({
  baseURL: apiUrl,
  timeout: 30000, // 30 second timeout
  // Ensuring that responses are processed correctly.
  transformResponse: (data) => data,
})

// Response interceptor with automatic 401 token refresh and retry.
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

      // Try local JWT refresh first
      const storage = getAuthStorage()
      const localRefreshToken = storage.getItem(LOCAL_AUTH_REFRESH_TOKEN_KEY)
      const localToken = getLocalAuthToken()
      if (localToken && localRefreshToken) {
        try {
          const authClient = getAuthClient()
          const tokenResponse = await authClient.refreshToken({
            token: localToken,
            refreshToken: localRefreshToken,
          })
          storage.setItem(LOCAL_AUTH_TOKEN_KEY, tokenResponse.token)
          storage.setItem(LOCAL_AUTH_REFRESH_TOKEN_KEY, tokenResponse.refreshToken)
          storage.setItem(LOCAL_AUTH_TOKEN_EXPIRY_KEY, new Date(tokenResponse.tokenExpiresAt).toISOString())
          originalRequest.headers = originalRequest.headers || {}
          originalRequest.headers.Authorization = `Bearer ${tokenResponse.token}`
          return axiosClient(originalRequest)
        } catch (refreshError) {
          console.error('Local token refresh on 401 failed:', refreshError)
          clearLocalAuth()
        }
      }

      // Fall back to MSAL refresh
      if (msalInstance) {
        try {
          const accounts = msalInstance.getAllAccounts()
          if (accounts.length > 0) {
            const response = await msalInstance.acquireTokenSilent({
              ...tokenRequest,
              account: accounts[0],
              forceRefresh: true,
            })
            originalRequest.headers = originalRequest.headers || {}
            originalRequest.headers.Authorization = `Bearer ${response.accessToken}`
            return axiosClient(originalRequest)
          }
        } catch (refreshError) {
          console.error('MSAL token refresh on 401 failed:', refreshError)
        }
      }
    }

    console.error('API Error:', error.message, error.config?.url)
    return Promise.reject(error)
  },
)

// Request interceptor: attach auth token (local JWT or MSAL) to outgoing requests.
axiosClient.interceptors.request.use(
  async (config) => {
    // Check for local JWT first
    const localToken = getLocalAuthToken()
    if (localToken) {
      config.headers.Authorization = `Bearer ${localToken}`
      return config
    }

    // Fall back to MSAL token acquisition
    let token: string | null = null
    try {
      const accounts = msalInstance?.getAllAccounts() ?? []

      if (accounts.length > 0) {
        const response = await msalInstance.acquireTokenSilent({
          ...tokenRequest,
          account: accounts[0],
        })
        token = response.accessToken
      }
    } catch (error: any) {
      if (error instanceof InteractionRequiredAuthError) {
        try {
          const response = await msalInstance.acquireTokenPopup(tokenRequest)
          token = response.accessToken
        } catch (popupError) {
          console.error('Token popup failed:', popupError)
        }
      } else {
        console.error('Token acquisition error:', error)
      }
    }

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
 * Performs an authenticated fetch request with automatic token acquisition.
 * Use this for custom API calls that need authentication but can't use the generated clients.
 *
 * @param url - The URL to fetch (relative to API base URL or absolute)
 * @param options - Standard fetch RequestInit options
 * @returns Promise<Response>
 *
 * @example
 * const response = await authenticatedFetch('/api/ppm/projects/123/tasks/456', {
 *   method: 'PATCH',
 *   headers: { 'Content-Type': 'application/json-patch+json' },
 *   body: JSON.stringify(patchOperations)
 * })
 */
export async function authenticatedFetch(
  url: string,
  options: RequestInit = {},
): Promise<Response> {
  // Acquire auth token — check local JWT first, then fall back to MSAL
  let token: string | null = getLocalAuthToken()

  if (!token) {
    try {
      const accounts = msalInstance?.getAllAccounts() ?? []
      if (accounts.length > 0) {
        const response = await msalInstance.acquireTokenSilent({
          ...tokenRequest,
          account: accounts[0],
        })
        token = response.accessToken
      }
    } catch (error: any) {
      if (error instanceof InteractionRequiredAuthError) {
        try {
          const response = await msalInstance.acquireTokenPopup(tokenRequest)
          token = response.accessToken
        } catch (popupError) {
          console.error('Token popup failed:', popupError)
        }
      } else {
        console.error('Token acquisition error:', error)
      }
    }
  }

  // Merge headers
  const headers = new Headers(options.headers)
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  // Add Accept header if not present
  if (!headers.has('Accept')) {
    headers.set('Accept', 'application/json')
  }

  // Prepend base URL if the URL is relative
  const fullUrl = url.startsWith('http') ? url : `${apiUrl}${url}`

  // Make the fetch call
  return fetch(fullUrl, {
    ...options,
    headers,
    credentials: options.credentials || 'include',
  })
}

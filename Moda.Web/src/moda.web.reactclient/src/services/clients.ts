import axios from 'axios'
import {
  AzureDevOpsBoardsConnectionsClient,
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
} from './moda-api'
import { tokenRequest } from '@/auth-config'
import { InteractionRequiredAuthError } from '@azure/msal-browser'
import { msalInstance } from '../components/contexts/auth'

const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL

const axiosClient = axios.create({
  baseURL: apiUrl,
  // Ensuring that responses are processed correctly.
  transformResponse: (data) => data,
})

// Use the shared MSAL instance to acquire tokens for outgoing requests.
axiosClient.interceptors.request.use(
  async (config) => {
    let token: string | null = null
    try {
      const response = await msalInstance.acquireTokenSilent(tokenRequest)
      token = response.accessToken
    } catch (error: any) {
      if (error instanceof InteractionRequiredAuthError) {
        const response = await msalInstance.acquireTokenPopup(tokenRequest)
        token = response.accessToken
      } else {
        throw error
      }
    }

    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    } else {
      throw new Error(
        'Unable to acquire token. User might not be authenticated.',
      )
    }
    return config
  },
  (error) => Promise.reject(error),
)

// API Clients
export const getAzureDevOpsBoardsConnectionsClient = () =>
  new AzureDevOpsBoardsConnectionsClient('', axiosClient)

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

// USER MANAGEMENT
export const getPermissionsClient = () => new PermissionsClient('', axiosClient)
export const getProfileClient = () => new ProfileClient('', axiosClient)
export const getRolesClient = () => new RolesClient('', axiosClient)
export const getUsersClient = () => new UsersClient('', axiosClient)

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
  // Acquire auth token
  let token: string | null = null
  try {
    const response = await msalInstance.acquireTokenSilent(tokenRequest)
    token = response.accessToken
  } catch (error: any) {
    if (error instanceof InteractionRequiredAuthError) {
      const response = await msalInstance.acquireTokenPopup(tokenRequest)
      token = response.accessToken
    } else {
      throw error
    }
  }

  if (!token) {
    throw new Error(
      'Unable to acquire token. User might not be authenticated.',
    )
  }

  // Merge headers with Authorization
  const headers = new Headers(options.headers)
  headers.set('Authorization', `Bearer ${token}`)

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

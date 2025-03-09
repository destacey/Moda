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
  ExpenditureCategoriesClient,
  ProjectsClient,
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

// PPM
export const getExpenditureCategoriesClient = () =>
  new ExpenditureCategoriesClient('', axiosClient)
export const getPortfoliosClient = () => new PortfoliosClient('', axiosClient)
export const getProjectsClient = () => new ProjectsClient('', axiosClient)

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

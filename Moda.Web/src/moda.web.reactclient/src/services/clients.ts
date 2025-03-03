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
import auth from './auth'

const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL

const createDefaultAxiosInstance = async (accessToken?: string) =>
  axios.create({
    baseURL: apiUrl,
    headers: {
      Authorization: `Bearer ${
        accessToken ?? (await auth.acquireToken())?.token
      }`,
    },
    //Removing the transformResponse will cause the response to be a string instead of an object
    transformResponse: (data) => data,
  })

export const getAzureDevOpsBoardsConnectionsClient = async (
  accessToken?: string,
) =>
  new AzureDevOpsBoardsConnectionsClient(
    '',
    await createDefaultAxiosInstance(accessToken),
  )

export const getBackgroundJobsClient = async (accessToken?: string) =>
  new BackgroundJobsClient('', await createDefaultAxiosInstance(accessToken))

export const getHealthChecksClient = async (accessToken?: string) =>
  new HealthChecksClient('', await createDefaultAxiosInstance(accessToken))

export const getLinksClient = async (accessToken?: string) =>
  new LinksClient('', await createDefaultAxiosInstance(accessToken))

// ORGANIZATION
export const getEmployeesClient = async (accessToken?: string) =>
  new EmployeesClient('', await createDefaultAxiosInstance(accessToken))
export const getTeamsClient = async (accessToken?: string) =>
  new TeamsClient('', await createDefaultAxiosInstance(accessToken))
export const getTeamsOfTeamsClient = async (accessToken?: string) =>
  new TeamsOfTeamsClient('', await createDefaultAxiosInstance(accessToken))

// PLANNING
export const getPlanningIntervalsClient = async (accessToken?: string) =>
  new PlanningIntervalsClient('', await createDefaultAxiosInstance(accessToken))
export const getRisksClient = async (accessToken?: string) =>
  new RisksClient('', await createDefaultAxiosInstance(accessToken))
export const getRoadmapsClient = async (accessToken?: string) =>
  new RoadmapsClient('', await createDefaultAxiosInstance(accessToken))

// PPM
export const getExpenditureCategoriesClient = async (accessToken?: string) =>
  new ExpenditureCategoriesClient(
    '',
    await createDefaultAxiosInstance(accessToken),
  )
export const getPortfoliosClient = async (accessToken?: string) =>
  new PortfoliosClient('', await createDefaultAxiosInstance(accessToken))
export const getProjectsClient = async (accessToken?: string) =>
  new ProjectsClient('', await createDefaultAxiosInstance(accessToken))

// STRATEGIC MANAGEMENT
export const getStrategicThemesClient = async (accessToken?: string) =>
  new StrategicThemesClient('', await createDefaultAxiosInstance(accessToken))

// WORK MANAGEMENT
export const getWorkProcessesClient = async (accessToken?: string) =>
  new WorkProcessesClient('', await createDefaultAxiosInstance(accessToken))
export const getWorkspacesClient = async (accessToken?: string) =>
  new WorkspacesClient('', await createDefaultAxiosInstance(accessToken))
export const getWorkStatusesClient = async (accessToken?: string) =>
  new WorkStatusesClient('', await createDefaultAxiosInstance(accessToken))
export const getWorkTypeLevelsClient = async (accessToken?: string) =>
  new WorkTypeLevelsClient('', await createDefaultAxiosInstance(accessToken))
export const getWorkTypesClient = async (accessToken?: string) =>
  new WorkTypesClient('', await createDefaultAxiosInstance(accessToken))
export const getWorkTypeTiersClient = async (accessToken?: string) =>
  new WorkTypeTiersClient('', await createDefaultAxiosInstance(accessToken))

// USER MANAGEMENT
export const getPermissionsClient = async (accessToken?: string) =>
  new PermissionsClient('', await createDefaultAxiosInstance(accessToken))
export const getProfileClient = async (accessToken?: string) =>
  new ProfileClient('', await createDefaultAxiosInstance(accessToken))
export const getRolesClient = async (accessToken?: string) =>
  new RolesClient('', await createDefaultAxiosInstance(accessToken))
export const getUsersClient = async (accessToken?: string) =>
  new UsersClient('', await createDefaultAxiosInstance(accessToken))

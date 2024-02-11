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
export const getEmployeesClient = async (accessToken?: string) =>
  new EmployeesClient('', await createDefaultAxiosInstance(accessToken))
export const getHealthChecksClient = async (accessToken?: string) =>
  new HealthChecksClient('', await createDefaultAxiosInstance(accessToken))
export const getLinksClient = async (accessToken?: string) =>
  new LinksClient('', await createDefaultAxiosInstance(accessToken))
export const getPermissionsClient = async (accessToken?: string) =>
  new PermissionsClient('', await createDefaultAxiosInstance(accessToken))
export const getProfileClient = async (accessToken?: string) =>
  new ProfileClient('', await createDefaultAxiosInstance(accessToken))
export const getPlanningIntervalsClient = async (accessToken?: string) =>
  new PlanningIntervalsClient('', await createDefaultAxiosInstance(accessToken))
export const getRisksClient = async (accessToken?: string) =>
  new RisksClient('', await createDefaultAxiosInstance(accessToken))
export const getRolesClient = async (accessToken?: string) =>
  new RolesClient('', await createDefaultAxiosInstance(accessToken))
export const getTeamsClient = async (accessToken?: string) =>
  new TeamsClient('', await createDefaultAxiosInstance(accessToken))
export const getTeamsOfTeamsClient = async (accessToken?: string) =>
  new TeamsOfTeamsClient('', await createDefaultAxiosInstance(accessToken))
export const getUsersClient = async (accessToken?: string) =>
  new UsersClient('', await createDefaultAxiosInstance(accessToken))
export const getWorkProcessesClient = async (accessToken?: string) =>
  new WorkProcessesClient('', await createDefaultAxiosInstance(accessToken))
export const getWorkStatusesClient = async (accessToken?: string) =>
  new WorkStatusesClient('', await createDefaultAxiosInstance(accessToken))
export const getWorkTypesClient = async (accessToken?: string) =>
  new WorkTypesClient('', await createDefaultAxiosInstance(accessToken))

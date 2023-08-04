import axios from 'axios'
import {
  BackgroundJobsClient,
  EmployeesClient,
  PermissionsClient,
  ProfileClient,
  ProgramIncrementsClient,
  RisksClient,
  RolesClient,
  TeamsClient,
  TeamsOfTeamsClient,
  UsersClient,
} from './moda-api'
import auth from './auth'

const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL

const createDefaultAxiosInstance = async (accessToken?: string) =>
  axios.create({
    baseURL: apiUrl,
    headers: {
      Authorization: `Bearer ${accessToken ?? (await auth.acquireToken())}`,
    },
    //Removing the transformResponse will cause the response to be a string instead of an object
    transformResponse: (data) => data,
  })

export const getBackgroundJobsClient = async (accessToken?: string) =>
  new BackgroundJobsClient('', await createDefaultAxiosInstance(accessToken))
export const getEmployeesClient = async (accessToken?: string) =>
  new EmployeesClient('', await createDefaultAxiosInstance(accessToken))
  export const getPermissionsClient = async (accessToken?: string) =>
    new PermissionsClient('', await createDefaultAxiosInstance(accessToken))
export const getProfileClient = async (accessToken?: string) =>
  new ProfileClient('', await createDefaultAxiosInstance(accessToken))
export const getProgramIncrementsClient = async (accessToken?: string) =>
  new ProgramIncrementsClient('', await createDefaultAxiosInstance(accessToken))
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

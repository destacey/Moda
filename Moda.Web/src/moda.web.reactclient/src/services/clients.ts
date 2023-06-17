import axios from "axios";
import { BackgroundJobsClient, EmployeesClient, ProfileClient, ProgramIncrementsClient, RisksClient, RolesClient, TeamsClient, TeamsOfTeamsClient, UsersClient } from "./moda-api";
import { acquireToken } from "./auth";

const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL;

const createDefaultAxiosInstance = async () => axios.create({
  baseURL: apiUrl,
  headers: {
    Authorization: `Bearer ${await acquireToken()}`
  },
  //Removing the transformResponse will cause the response to be a string instead of an object
  transformResponse: data => data
})

export const getBackgroundJobsClient = async () => new BackgroundJobsClient('', await createDefaultAxiosInstance())
export const getEmployeesClient = async () => new EmployeesClient('', await createDefaultAxiosInstance())
export const getProfileClient = async () => new ProfileClient('', await createDefaultAxiosInstance())
export const getProgramIncrementsClient = async () => new ProgramIncrementsClient('', await createDefaultAxiosInstance())
export const getRisksClient = async () => new RisksClient('', await createDefaultAxiosInstance())
export const getRolesClient = async () => new RolesClient('', await createDefaultAxiosInstance())
export const getTeamsClient = async () => new TeamsClient('', await createDefaultAxiosInstance())
export const getTeamsOfTeamsClient = async () => new TeamsOfTeamsClient('', await createDefaultAxiosInstance())
export const getUsersClient = async () => new UsersClient('', await createDefaultAxiosInstance())

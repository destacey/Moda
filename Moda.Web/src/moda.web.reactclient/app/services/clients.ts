import axios from "axios";
import { ProfileClient } from "./moda-api";
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

export const getProfileClient = async () => new ProfileClient('', await createDefaultAxiosInstance())
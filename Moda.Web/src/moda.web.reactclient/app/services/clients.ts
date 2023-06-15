import axios from "axios";
import { ProfileClient } from "./moda-api";
import { acquireToken } from "./auth";

const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL;

export const getProfileClient = async () => new ProfileClient('', axios.create({
  baseURL: apiUrl,
  headers: {
    Authorization: `Bearer ${await acquireToken()}`
  }
}))
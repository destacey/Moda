import axios from "axios";
import { ProfileClient } from "./moda-api";
import { acquireToken } from "./auth";


export const getProfileClient = async () => new ProfileClient('', axios.create({
  headers: {
    Authorization: `Bearer ${await acquireToken()}`
  }
}))
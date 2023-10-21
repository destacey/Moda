import { useQuery } from 'react-query'
import { QK } from './query-keys'
import {
  getEmployeesClient,
  getTeamsClient,
  getTeamsOfTeamsClient,
} from '../clients'

// TEAMS - RISKS
export const useGetTeamRisks = (
  id: string,
  includeClosed: boolean = false,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.TEAM_RISKS, id, includeClosed],
    queryFn: async () => (await getTeamsClient()).getRisks(id, includeClosed),
    staleTime: 10000,
    enabled: !!id && enabled,
  })
}

// TEAM OF TEAMS - RISKS
export const useGetTeamOfTeamsRisks = (
  id: string,
  includeClosed: boolean = false,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.TEAM_OF_TEAMS_RISKS, id, includeClosed],
    queryFn: async () =>
      (await getTeamsOfTeamsClient()).getRisks(id, includeClosed),
    staleTime: 10000,
    enabled: !!id && enabled,
  })
}

// EMPLOYEES
export const useGetEmployees = (includeInactive: boolean = false) => {
  return useQuery({
    queryKey: [QK.EMPLOYEES, includeInactive],
    queryFn: async () => (await getEmployeesClient()).getList(includeInactive),
    select: (data) =>
      data.sort((a, b) => a.displayName.localeCompare(b.displayName)),
    //staleTime: 60000,
  })
}

import { useQuery } from 'react-query'
import { QK } from './query-keys'
import {
  getEmployeesClient,
  getTeamsClient,
  getTeamsOfTeamsClient,
} from '../clients'
import _ from 'lodash'
import { OptionModel } from '@/src/app/components/types'

// TEAM MEMBERSHIPS

export const useGetTeamMemberships = (teamId: string, enabled: boolean) => {
  return useQuery({
    queryKey: [QK.TEAM_MEMBERSHIPS, teamId],
    queryFn: async () => (await getTeamsClient()).getTeamMemberships(teamId),
    staleTime: 10000,
    enabled: !!teamId && enabled,
  })
}

export const useGetTeamOfTeamsMemberships = (
  teamId: string,
  enabled: boolean,
) => {
  return useQuery({
    queryKey: [QK.TEAM_MEMBERSHIPS, teamId],
    queryFn: async () =>
      (await getTeamsOfTeamsClient()).getTeamMemberships(teamId),
    staleTime: 10000,
    enabled: !!teamId && enabled,
  })
}

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
    select: (data) => _.sortBy(data, ['displayName']),
    staleTime: 60000,
  })
}

export const useGetEmployeeOptions = (includeInactive: boolean = false) => {
  return useQuery({
    queryKey: [QK.EMPLOYEE_OPTIONS, includeInactive],
    queryFn: async () => (await getEmployeesClient()).getList(includeInactive),
    select: (data) => {
      const statuses = _.sortBy(data, ['displayName'])
      const options: OptionModel[] = statuses.map((e) => ({
        value: e.id,
        label: e.displayName,
      }))
      return options
    },
  })
}

import { useMutation, useQuery, useQueryClient } from 'react-query'
import { QK } from './query-keys'
import {
  getEmployeesClient,
  getTeamsClient,
  getTeamsOfTeamsClient,
} from '../clients'
import _ from 'lodash'
import { OptionModel } from '@/src/app/components/types'
import {
  AddTeamMembershipRequest,
  UpdateTeamMembershipRequest,
} from '../moda-api'
import { TeamTypeName } from '@/src/app/organizations/types'

// TEAM OF TEAMS
export const useGetTeamOfTeamsOptions = (includeInactive: boolean = false) => {
  return useQuery({
    queryKey: [QK.TEAM_OF_TEAMS_OPTIONS],
    queryFn: async () =>
      (await getTeamsOfTeamsClient()).getList(includeInactive),
    select: (data) => {
      const teams = _.sortBy(data, ['name'])
      const options: OptionModel[] = teams.map((t) => ({
        value: t.id,
        label: t.isActive ? t.name : `${t.name} (Inactive)`,
      }))
      return options
    },
  })
}

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

export interface CreateTeamMembershipMutationRequest {
  membership: AddTeamMembershipRequest
  teamType: TeamTypeName
}
export const useCreateTeamMembershipMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({
      membership,
      teamType,
    }: CreateTeamMembershipMutationRequest) => {
      if (teamType === 'Team') {
        return (await getTeamsClient()).addTeamMembership(
          membership.teamId,
          membership,
        )
      } else if (teamType === 'Team of Teams') {
        return (await getTeamsOfTeamsClient()).addTeamMembership(
          membership.teamId,
          membership,
        )
      } else {
        throw new Error(`Invalid team type: ${teamType}`)
      }
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([
        QK.TEAM_MEMBERSHIPS,
        variables.membership.teamId,
      ])
      queryClient.invalidateQueries([
        QK.TEAM_MEMBERSHIPS,
        variables.membership.parentTeamId,
      ])
    },
  })
}

export interface UpdateTeamMembershipMutationRequest {
  membership: UpdateTeamMembershipRequest
  parentTeamId: string
  teamType: TeamTypeName
}
export const useUpdateTeamMembershipMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({
      membership,
      parentTeamId,
      teamType,
    }: UpdateTeamMembershipMutationRequest) => {
      if (teamType === 'Team') {
        return (await getTeamsClient()).updateTeamMembership(
          membership.teamId,
          membership.teamMembershipId,
          membership,
        )
      } else if (teamType === 'Team of Teams') {
        return (await getTeamsOfTeamsClient()).updateTeamMembership(
          membership.teamId,
          membership.teamMembershipId,
          membership,
        )
      } else {
        throw new Error(`Invalid team type: ${teamType}`)
      }
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([
        QK.TEAM_MEMBERSHIPS,
        variables.membership.teamId,
      ])
      queryClient.invalidateQueries([
        QK.TEAM_MEMBERSHIPS,
        variables.parentTeamId,
      ])
    },
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
        label: e.isActive ? e.displayName : `${e.displayName} (Inactive)`,
      }))
      return options
    },
  })
}

import { useMutation, useQuery, useQueryClient } from 'react-query'
import { QK } from './query-keys'
import { getTeamsClient, getTeamsOfTeamsClient } from '../clients'
import _ from 'lodash'
import { OptionModel } from '@/src/components/types'
import {
  AddTeamMembershipRequest,
  UpdateTeamMembershipRequest,
} from '../moda-api'
import { TeamTypeName } from '@/src/app/organizations/types'

// TEAM OF TEAMS
export const useGetTeamOfTeamsOptions = (includeInactive: boolean = false) => {
  return useQuery({
    queryKey: [QK.TEAM_OF_TEAMS_OPTIONS],
    queryFn: async () => getTeamsOfTeamsClient().getList(includeInactive),
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
    queryFn: async () => getTeamsClient().getTeamMemberships(teamId),
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
    queryFn: async () => getTeamsOfTeamsClient().getTeamMemberships(teamId),
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
        return getTeamsClient().addTeamMembership(membership.teamId, membership)
      } else if (teamType === 'Team of Teams') {
        return getTeamsOfTeamsClient().addTeamMembership(
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
    mutationFn: async (request: UpdateTeamMembershipMutationRequest) => {
      if (request.teamType === 'Team') {
        return getTeamsClient().updateTeamMembership(
          request.membership.teamId,
          request.membership.teamMembershipId,
          request.membership,
        )
      } else if (request.teamType === 'Team of Teams') {
        return getTeamsOfTeamsClient().updateTeamMembership(
          request.membership.teamId,
          request.membership.teamMembershipId,
          request.membership,
        )
      } else {
        throw new Error(`Invalid team type: ${request.teamType}`)
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

export interface DeleteTeamMembershipMutationRequest {
  teamMembershipId: string
  teamId: string
  parentTeamId: string
  teamType: TeamTypeName
}
export const useDeleteTeamMembershipMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (request: DeleteTeamMembershipMutationRequest) => {
      if (request.teamType === 'Team') {
        return getTeamsClient().removeTeamMembership(
          request.teamId,
          request.teamMembershipId,
        )
      } else if (request.teamType === 'Team of Teams') {
        return getTeamsOfTeamsClient().removeTeamMembership(
          request.teamId,
          request.teamMembershipId,
        )
      } else {
        throw new Error(`Invalid team type: ${request.teamType}`)
      }
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.TEAM_MEMBERSHIPS, variables.teamId])
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
    queryFn: async () => getTeamsClient().getRisks(id, includeClosed),
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
    queryFn: async () => getTeamsOfTeamsClient().getRisks(id, includeClosed),
    staleTime: 10000,
    enabled: !!id && enabled,
  })
}

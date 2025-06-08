import {
  DeactivateTeamOfTeamsRequest,
  DeactivateTeamRequest,
  FunctionalOrganizationChartDto,
  // Add missing imports for memberships and risks
  AddTeamMembershipRequest,
  UpdateTeamMembershipRequest,
  TeamMembershipDto,
  RiskListDto,
} from './../../../services/moda-api'
import { TeamListItem, TeamTypeName } from '@/src/app/organizations/types'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'
import { BaseOptionType } from 'antd/es/select'
import { DependencyDto, WorkItemBacklogItemDto } from '@/src/services/moda-api'
import { OptionModel } from '@/src/components/types'

export const teamApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getTeams: builder.query<TeamListItem[], boolean>({
      queryFn: async (includeInactive) => {
        try {
          const teams = await getTeamsClient().getList(includeInactive)
          const teamsOfTeams =
            await getTeamsOfTeamsClient().getList(includeInactive)
          const data = [
            ...(teams as TeamListItem[]),
            ...(teamsOfTeams as TeamListItem[]),
          ]
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, includeInactive) => [
        { type: QueryTags.Team, id: `LIST-${includeInactive}` },
        QueryTags.Team, // Keep general tag for invalidation
      ],
    }),
    deactivateTeam: builder.mutation<void, DeactivateTeamRequest>({
      queryFn: async (request) => {
        try {
          const data = await getTeamsClient().deactivate(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => [
        // Invalidate all team-related cache entries
        QueryTags.Team,
        QueryTags.TeamOptions,
      ],
    }),
    deactivateTeamOfTeams: builder.mutation<void, DeactivateTeamOfTeamsRequest>(
      {
        queryFn: async (request) => {
          try {
            const data = await getTeamsOfTeamsClient().deactivate(
              request.id,
              request,
            )
            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
        invalidatesTags: () => [
          // Invalidate all team-related cache entries
          QueryTags.Team,
          QueryTags.TeamOptions,
        ],
      },
    ),
    getTeamOptions: builder.query<BaseOptionType[], boolean>({
      queryFn: async (includeInactive) => {
        try {
          const teams = await getTeamsClient().getList(includeInactive)
          const teamsOfTeams =
            await getTeamsOfTeamsClient().getList(includeInactive)
          const teamsData = [
            ...(teams as TeamListItem[]),
            ...(teamsOfTeams as TeamListItem[]),
          ]
          const data: BaseOptionType[] = teamsData
            .sort((a, b) => a.name.localeCompare(b.name))
            .map((team) => ({
              label: team.name,
              value: team.id,
            }))
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, includeInactive) => [
        { type: QueryTags.TeamOptions, id: `OPTIONS-${includeInactive}` },
        QueryTags.TeamOptions, // Keep general tag for invalidation
      ],
    }),
    getTeamBacklog: builder.query<WorkItemBacklogItemDto[], string>({
      queryFn: async (idOrCode: string) => {
        try {
          const data = await getTeamsClient().getTeamBacklog(idOrCode)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.TeamBacklog,
        ...result.map(({ key }) => ({ type: QueryTags.TeamBacklog, key })),
      ],
    }),
    getTeamDependencies: builder.query<DependencyDto[], string>({
      queryFn: async (id: string) => {
        try {
          const data = await getTeamsClient().getTeamDependencies(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.TeamDependency,
        ...result.map((result, error, arg) => ({
          type: QueryTags.TeamDependency,
          arg,
        })),
      ],
    }),
    getFunctionalOrganizationChart: builder.query<
      FunctionalOrganizationChartDto,
      string | null | undefined
    >({
      queryFn: async (asOfDate: string) => {
        try {
          const date = asOfDate ? new Date(asOfDate) : undefined
          const data =
            await getTeamsClient().getFunctionalOrganizationChart(date)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        {
          type: QueryTags.FunctionalOrganizationChart,
          id: arg,
        },
      ],
    }), // TEAM OF TEAMS OPTIONS
    getTeamOfTeamsOptions: builder.query<OptionModel[], boolean>({
      queryFn: async (includeInactive = false) => {
        try {
          const teams = await getTeamsOfTeamsClient().getList(includeInactive)
          const data: OptionModel[] = teams
            .sort((a, b) => a.name.localeCompare(b.name))
            .map((t) => ({
              value: t.id,
              label: t.isActive ? t.name : `${t.name} (Inactive)`,
            }))
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, includeInactive) => [
        { type: QueryTags.TeamOptions, id: `TEAM_OF_TEAMS-${includeInactive}` },
        QueryTags.TeamOptions, // Keep general tag for invalidation
      ],
    }),

    // TEAM MEMBERSHIPS
    getTeamMemberships: builder.query<
      TeamMembershipDto[],
      { teamId: string; enabled?: boolean }
    >({
      queryFn: async ({ teamId }) => {
        try {
          const data = await getTeamsClient().getTeamMemberships(teamId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, { teamId }) => [
        { type: QueryTags.TeamMembership, id: teamId },
        { type: QueryTags.TeamMembership, id: 'LIST' },
      ],
    }),

    getTeamOfTeamsMemberships: builder.query<
      TeamMembershipDto[],
      { teamId: string; enabled?: boolean }
    >({
      queryFn: async ({ teamId }) => {
        try {
          const data = await getTeamsOfTeamsClient().getTeamMemberships(teamId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, { teamId }) => [
        { type: QueryTags.TeamMembership, id: teamId },
        { type: QueryTags.TeamMembership, id: 'LIST' },
      ],
    }),

    // TEAM MEMBERSHIP MUTATIONS
    createTeamMembership: builder.mutation<
      void,
      { membership: AddTeamMembershipRequest; teamType: TeamTypeName }
    >({
      queryFn: async ({ membership, teamType }) => {
        try {
          let data
          if (teamType === 'Team') {
            data = await getTeamsClient().addTeamMembership(
              membership.teamId,
              membership,
            )
          } else if (teamType === 'Team of Teams') {
            data = await getTeamsOfTeamsClient().addTeamMembership(
              membership.teamId,
              membership,
            )
          } else {
            throw new Error(`Invalid team type: ${teamType}`)
          }
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { membership }) => [
        { type: QueryTags.TeamMembership, id: membership.teamId },
        { type: QueryTags.TeamMembership, id: membership.parentTeamId },
        { type: QueryTags.TeamMembership, id: 'LIST' },
        QueryTags.Team,
        QueryTags.TeamOptions,
      ],
    }),

    updateTeamMembership: builder.mutation<
      void,
      {
        membership: UpdateTeamMembershipRequest
        parentTeamId: string
        teamType: TeamTypeName
      }
    >({
      queryFn: async ({ membership, teamType }) => {
        try {
          let data
          if (teamType === 'Team') {
            data = await getTeamsClient().updateTeamMembership(
              membership.teamId,
              membership.teamMembershipId,
              membership,
            )
          } else if (teamType === 'Team of Teams') {
            data = await getTeamsOfTeamsClient().updateTeamMembership(
              membership.teamId,
              membership.teamMembershipId,
              membership,
            )
          } else {
            throw new Error(`Invalid team type: ${teamType}`)
          }
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { membership, parentTeamId }) => [
        { type: QueryTags.TeamMembership, id: membership.teamId },
        { type: QueryTags.TeamMembership, id: parentTeamId },
        { type: QueryTags.TeamMembership, id: 'LIST' },
        QueryTags.Team,
        QueryTags.TeamOptions,
      ],
    }),

    deleteTeamMembership: builder.mutation<
      void,
      {
        teamMembershipId: string
        teamId: string
        parentTeamId: string
        teamType: TeamTypeName
      }
    >({
      queryFn: async ({ teamMembershipId, teamId, teamType }) => {
        try {
          let data
          if (teamType === 'Team') {
            data = await getTeamsClient().removeTeamMembership(
              teamId,
              teamMembershipId,
            )
          } else if (teamType === 'Team of Teams') {
            data = await getTeamsOfTeamsClient().removeTeamMembership(
              teamId,
              teamMembershipId,
            )
          } else {
            throw new Error(`Invalid team type: ${teamType}`)
          }
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { teamId, parentTeamId }) => [
        { type: QueryTags.TeamMembership, id: teamId },
        { type: QueryTags.TeamMembership, id: parentTeamId },
        { type: QueryTags.TeamMembership, id: 'LIST' },
        QueryTags.Team,
        QueryTags.TeamOptions,
      ],
    }),

    // TEAM RISKS
    getTeamRisks: builder.query<
      RiskListDto[],
      { id: string; includeClosed?: boolean; enabled?: boolean }
    >({
      queryFn: async ({ id, includeClosed = false }) => {
        try {
          const data = await getTeamsClient().getRisks(id, includeClosed)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, { id, includeClosed }) => [
        { type: QueryTags.TeamRisk, id: `${id}-${includeClosed}` },
        { type: QueryTags.TeamRisk, id: 'LIST' },
      ],
    }),

    getTeamOfTeamsRisks: builder.query<
      RiskListDto[],
      { id: string; includeClosed?: boolean; enabled?: boolean }
    >({
      queryFn: async ({ id, includeClosed = false }) => {
        try {
          const data = await getTeamsOfTeamsClient().getRisks(id, includeClosed)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, { id, includeClosed }) => [
        { type: QueryTags.TeamRisk, id: `${id}-${includeClosed}` },
        { type: QueryTags.TeamRisk, id: 'LIST' },
      ],
    }),
  }),
})

export const {
  useGetTeamsQuery,
  useDeactivateTeamMutation,
  useDeactivateTeamOfTeamsMutation,
  useGetTeamOptionsQuery,
  useGetTeamBacklogQuery,
  useGetTeamDependenciesQuery,
  useGetFunctionalOrganizationChartQuery,
  useGetTeamOfTeamsOptionsQuery,
  useGetTeamMembershipsQuery,
  useGetTeamOfTeamsMembershipsQuery,
  useCreateTeamMembershipMutation,
  useUpdateTeamMembershipMutation,
  useDeleteTeamMembershipMutation,
  useGetTeamRisksQuery,
  useGetTeamOfTeamsRisksQuery,
} = teamApi

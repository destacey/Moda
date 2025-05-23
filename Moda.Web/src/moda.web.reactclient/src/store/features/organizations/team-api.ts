import {
  DeactivateTeamOfTeamsRequest,
  DeactivateTeamRequest,
  FunctionalOrganizationChartDto,
} from './../../../services/moda-api'
import { TeamListItem } from '@/src/app/organizations/types'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'
import { BaseOptionType } from 'antd/es/select'
import { DependencyDto, WorkItemBacklogItemDto } from '@/src/services/moda-api'

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
      providesTags: (result) => [
        QueryTags.Team,
        ...result.map(({ key }) => ({ type: QueryTags.Team, key })),
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
      providesTags: (result) => [
        QueryTags.Team,
        ...result.map(({ key }) => ({ type: QueryTags.Team, key })),
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
} = teamApi

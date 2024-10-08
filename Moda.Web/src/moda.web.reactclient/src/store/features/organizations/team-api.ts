import { TeamListItem } from '@/src/app/organizations/types'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'
import { BaseOptionType } from 'antd/es/select'
import { WorkItemBacklogItemDto } from '@/src/services/moda-api'

export const teamApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getTeams: builder.query<TeamListItem[], boolean>({
      queryFn: async (includeInactive) => {
        try {
          const teams = await (await getTeamsClient()).getList(includeInactive)
          const teamsOfTeams = await (
            await getTeamsOfTeamsClient()
          ).getList(includeInactive)
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
    getTeamOptions: builder.query<BaseOptionType[], boolean>({
      queryFn: async (includeInactive) => {
        try {
          const teams = await (await getTeamsClient()).getList(includeInactive)
          const teamsOfTeams = await (
            await getTeamsOfTeamsClient()
          ).getList(includeInactive)
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
          const data = await (await getTeamsClient()).getTeamBacklog(idOrCode)
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
  }),
})

export const {
  useGetTeamsQuery,
  useGetTeamOptionsQuery,
  useGetTeamBacklogQuery,
} = teamApi

import { getTeamsClient, getTeamsOfTeamsClient, getEmployeesClient } from '@/src/services/clients'
import {
  AddTeamMemberRequest,
  TeamMemberDto,
  UpdateTeamMemberRequest,
} from '@/src/services/wayd-api'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'

export type { TeamMemberDto }

export const teamMembersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getTeamMembers: builder.query<TeamMemberDto[], { teamId: string }>({
      queryFn: async ({ teamId }) => {
        try {
          const data = await getTeamsClient().getMembers(teamId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.TeamMembership, id: `MEMBERS-${arg.teamId}` },
      ],
    }),

    addTeamMember: builder.mutation<void, { teamId: string; request: AddTeamMemberRequest }>({
      queryFn: async ({ teamId, request }) => {
        try {
          const data = await getTeamsClient().addMember(teamId, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.TeamMembership, id: `MEMBERS-${arg.teamId}` },
      ],
    }),

    updateTeamMember: builder.mutation<void, { teamId: string; employeeId: string; request: UpdateTeamMemberRequest }>({
      queryFn: async ({ teamId, employeeId, request }) => {
        try {
          const data = await getTeamsClient().updateMember(teamId, employeeId, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.TeamMembership, id: `MEMBERS-${arg.teamId}` },
      ],
    }),

    removeTeamMember: builder.mutation<void, { teamId: string; employeeId: string }>({
      queryFn: async ({ teamId, employeeId }) => {
        try {
          const data = await getTeamsClient().removeMember(teamId, employeeId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.TeamMembership, id: `MEMBERS-${arg.teamId}` },
      ],
    }),

    getTeamOfTeamsMembers: builder.query<TeamMemberDto[], { teamId: string }>({
      queryFn: async ({ teamId }) => {
        try {
          const data = await getTeamsOfTeamsClient().getMembers(teamId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.TeamMembership, id: `MEMBERS-${arg.teamId}` },
      ],
    }),

    addTeamOfTeamsMember: builder.mutation<void, { teamId: string; request: AddTeamMemberRequest }>({
      queryFn: async ({ teamId, request }) => {
        try {
          const data = await getTeamsOfTeamsClient().addMember(teamId, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.TeamMembership, id: `MEMBERS-${arg.teamId}` },
      ],
    }),

    updateTeamOfTeamsMember: builder.mutation<void, { teamId: string; employeeId: string; request: UpdateTeamMemberRequest }>({
      queryFn: async ({ teamId, employeeId, request }) => {
        try {
          const data = await getTeamsOfTeamsClient().updateMember(teamId, employeeId, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.TeamMembership, id: `MEMBERS-${arg.teamId}` },
      ],
    }),

    removeTeamOfTeamsMember: builder.mutation<void, { teamId: string; employeeId: string }>({
      queryFn: async ({ teamId, employeeId }) => {
        try {
          const data = await getTeamsOfTeamsClient().removeMember(teamId, employeeId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.TeamMembership, id: `MEMBERS-${arg.teamId}` },
      ],
    }),

    getEmployeeTeamMemberships: builder.query<TeamMemberDto[], { employeeId: string }>({
      queryFn: async ({ employeeId }) => {
        try {
          const data = await getEmployeesClient().getTeamMemberships(employeeId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.TeamMembership, id: `EMP-TEAMS-${arg.employeeId}` },
      ],
    }),
  }),
})

export const {
  useGetTeamMembersQuery,
  useAddTeamMemberMutation,
  useUpdateTeamMemberMutation,
  useRemoveTeamMemberMutation,
  useGetTeamOfTeamsMembersQuery,
  useAddTeamOfTeamsMemberMutation,
  useUpdateTeamOfTeamsMemberMutation,
  useRemoveTeamOfTeamsMemberMutation,
  useGetEmployeeTeamMembershipsQuery,
} = teamMembersApi

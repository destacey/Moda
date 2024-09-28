import { getWorkProcessesClient } from '@/src/services/clients'
import {
  WorkProcessDto,
  WorkProcessListDto,
  WorkProcessSchemeDto,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'

// function providesList<R extends { id: string | number }[], T extends string>(
//   resultsWithIds: R | undefined,
//   tagType: T,
// ) {
//   return resultsWithIds
//     ? [
//         { type: tagType, id: 'LIST' },
//         ...resultsWithIds.map(({ id }) => ({ type: tagType, id })),
//       ]
//     : [{ type: tagType, id: 'LIST' }]
// }

export interface ChangeWorkProcessIsActiveMutationRequest {
  id: string
  isActive: boolean
}

export const workProcessApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getWorkProcesses: builder.query<WorkProcessListDto[], boolean>({
      queryFn: async (includeInactive) => {
        try {
          const data = await (
            await getWorkProcessesClient()
          ).getList(includeInactive)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      // TODO: why is the id on the model optional
      //providesTags: (result, error, arg) => providesList(result, QueryTags.WorkProcess),
      providesTags: (result) => [
        QueryTags.WorkProcess,
        ...result.map(({ id }) => ({ type: QueryTags.WorkProcess, id })),
      ],
    }),
    getWorkProcess: builder.query<WorkProcessDto, string>({
      queryFn: async (idOrKey: string) => {
        try {
          const data = await (await getWorkProcessesClient()).get(idOrKey)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.WorkProcess, id: arg }, // typically arg is the key
      ],
    }),
    changeWorkProcessIsActive: builder.mutation<
      null,
      ChangeWorkProcessIsActiveMutationRequest
    >({
      queryFn: async ({ id, isActive }) => {
        try {
          let data
          if (isActive) {
            data = await (await getWorkProcessesClient()).activate(id)
          } else {
            data = await (await getWorkProcessesClient()).deactivate(id)
          }
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.WorkProcess, id: arg.id }]
      },
    }),
    getWorkProcessSchemes: builder.query<WorkProcessSchemeDto[], string>({
      queryFn: async (workProcessId: string) => {
        try {
          const data = await (
            await getWorkProcessesClient()
          ).getSchemes(workProcessId)

          data.sort((a, b) => a.workType.name.localeCompare(b.workType.name))

          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.WorkProcessScheme, id: arg },
        ...result.map(({ id }) => ({ type: QueryTags.WorkProcessScheme, id })),
      ],
    }),
  }),
  overrideExisting: false,
})

export const {
  useGetWorkProcessesQuery,
  useGetWorkProcessQuery,
  useChangeWorkProcessIsActiveMutation,
  useGetWorkProcessSchemesQuery,
} = workProcessApi

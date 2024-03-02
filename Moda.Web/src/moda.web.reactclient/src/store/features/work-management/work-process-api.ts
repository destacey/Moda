import { getWorkProcessesClient } from '@/src/services/clients'
import { WorkProcessDto, WorkProcessListDto } from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'

function providesList<R extends { id: string | number }[], T extends string>(
  resultsWithIds: R | undefined,
  tagType: T,
) {
  return resultsWithIds
    ? [
        { type: tagType, id: 'LIST' },
        ...resultsWithIds.map(({ id }) => ({ type: tagType, id })),
      ]
    : [{ type: tagType, id: 'LIST' }]
}

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
          return { error }
        }
      },
      // TODO: why is the id on the model optional
      //providesTags: (result, error, arg) => providesList(result, QueryTags.WorkProcess),
      providesTags: (result, error, arg) => [
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
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.WorkProcess, id: arg }, // typically arg is the key
        { type: QueryTags.WorkProcess, id: result.id },
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
          console.log('changeWorkProcessIsActive', data)
          return { data }
        } catch (error) {
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        console.log('invalidatesTags', result, error, arg)
        return [{ type: QueryTags.WorkProcess, id: arg.id }]
      },
    }),
  }),
  overrideExisting: false,
})

export const {
  useGetWorkProcessesQuery,
  useGetWorkProcessQuery,
  useChangeWorkProcessIsActiveMutation,
} = workProcessApi

import {
  AddStrategicInitiativeKpiMeasurementRequest,
  CreateStrategicInitiativeKpiRequest,
  ManageStrategicInitiativeKpiCheckpointPlanRequest,
  ManageStrategicInitiativeProjectsRequest,
  ProjectListDto,
  StrategicInitiativeKpiCheckpointDetailsDto,
  StrategicInitiativeKpiCheckpointDto,
  StrategicInitiativeKpiDetailsDto,
  StrategicInitiativeKpiListDto,
  UpdateStrategicInitiativeKpiRequest,
} from './../../../services/moda-api'
import { apiSlice } from './../apiSlice'
import { QueryTags } from '../query-tags'
import { getStrategicInitiativesClient } from '@/src/services/clients'
import {
  CreateStrategicInitiativeRequest,
  ObjectIdAndKey,
  StrategicInitiativeDetailsDto,
  StrategicInitiativeListDto,
  UpdateStrategicInitiativeRequest,
} from '@/src/services/moda-api'
import { BaseOptionType } from 'antd/es/select'

export const strategicInitiativesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getStrategicInitiatives: builder.query<
      StrategicInitiativeListDto[],
      number | undefined
    >({
      queryFn: async (status = undefined) => {
        try {
          const data =
            await getStrategicInitiativesClient().getStrategicInitiatives(
              status,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.StrategicInitiative, id: 'LIST' }],
    }),
    getStrategicInitiative: builder.query<
      StrategicInitiativeDetailsDto,
      number
    >({
      queryFn: async (key) => {
        try {
          const data =
            await getStrategicInitiativesClient().getStrategicInitiative(
              key.toString(),
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.StrategicInitiative, id: arg },
      ],
    }),
    createStrategicInitiative: builder.mutation<
      ObjectIdAndKey,
      CreateStrategicInitiativeRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getStrategicInitiativesClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.PortfolioStrategicInitiatives, id: 'LIST' },
        ]
      },
    }),
    updateStrategicInitiative: builder.mutation<
      void,
      { request: UpdateStrategicInitiativeRequest; cacheKey: number }
    >({
      queryFn: async ({ request, cacheKey }) => {
        try {
          const data = await getStrategicInitiativesClient().update(
            request.id,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.StrategicInitiative, id: cacheKey },
          { type: QueryTags.PortfolioStrategicInitiatives, id: 'LIST' },
        ]
      },
    }),
    approveStrategicInitiative: builder.mutation<
      void,
      { id: string; cacheKey: number }
    >({
      queryFn: async ({ id }) => {
        try {
          const data = await getStrategicInitiativesClient().approve(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.StrategicInitiative, id: cacheKey },
          { type: QueryTags.PortfolioStrategicInitiatives, id: 'LIST' },
        ]
      },
    }),
    activateStrategicInitiative: builder.mutation<
      void,
      { id: string; cacheKey: number }
    >({
      queryFn: async ({ id }) => {
        try {
          const data = await getStrategicInitiativesClient().activate(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.StrategicInitiative, id: cacheKey },
          { type: QueryTags.PortfolioStrategicInitiatives, id: 'LIST' },
        ]
      },
    }),
    completeStrategicInitiative: builder.mutation<
      void,
      { id: string; cacheKey: number }
    >({
      queryFn: async ({ id }) => {
        try {
          const data = await getStrategicInitiativesClient().complete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.StrategicInitiative, id: cacheKey },
          { type: QueryTags.PortfolioStrategicInitiatives, id: 'LIST' },
        ]
      },
    }),
    cancelStrategicInitiative: builder.mutation<
      void,
      { id: string; cacheKey: number }
    >({
      queryFn: async ({ id }) => {
        try {
          const data = await getStrategicInitiativesClient().cancel(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.StrategicInitiative, id: cacheKey },
          { type: QueryTags.PortfolioStrategicInitiatives, id: 'LIST' },
        ]
      },
    }),
    deleteStrategicInitiative: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getStrategicInitiativesClient().delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.PortfolioStrategicInitiatives, id: 'LIST' },
        ]
      },
    }),
    getStrategicInitiativeKpis: builder.query<
      StrategicInitiativeKpiListDto[],
      string
    >({
      queryFn: async (strategicInitiativeId) => {
        try {
          const data = await getStrategicInitiativesClient().getKpis(
            strategicInitiativeId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.StrategicInitiativeKpi, id: arg },
      ],
    }),
    getStrategicInitiativeKpi: builder.query<
      StrategicInitiativeKpiDetailsDto,
      { strategicInitiativeId: string; kpiId: string }
    >({
      queryFn: async (request) => {
        try {
          const data = await getStrategicInitiativesClient().getKpi(
            request.strategicInitiativeId,
            request.kpiId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.StrategicInitiativeKpi, id: arg.kpiId },
      ],
    }),
    createStrategicInitiativeKpi: builder.mutation<
      ObjectIdAndKey,
      CreateStrategicInitiativeKpiRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getStrategicInitiativesClient().createKpi(
            request.strategicInitiativeId,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          {
            type: QueryTags.StrategicInitiativeKpi,
            id: arg.strategicInitiativeId,
          },
        ]
      },
    }),
    updateStrategicInitiativeKpi: builder.mutation<
      void,
      UpdateStrategicInitiativeKpiRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getStrategicInitiativesClient().updateKpi(
            request.strategicInitiativeId,
            request.kpiId,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          {
            type: QueryTags.StrategicInitiativeKpi,
            id: arg.strategicInitiativeId,
          },
          { type: QueryTags.StrategicInitiativeKpi, id: arg.kpiId },
        ]
      },
    }),
    deleteStrategicInitiativeKpi: builder.mutation<
      void,
      { strategicInitiativeId: string; kpiId: string }
    >({
      queryFn: async (request) => {
        try {
          const data = await getStrategicInitiativesClient().deleteKpi(
            request.strategicInitiativeId,
            request.kpiId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          {
            type: QueryTags.StrategicInitiativeKpi,
            id: arg.strategicInitiativeId,
          },
        ]
      },
    }),
    addStrategicInitiativeKpiMeasurement: builder.mutation<
      void,
      AddStrategicInitiativeKpiMeasurementRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getStrategicInitiativesClient().addKpiMeasurement(
            request.strategicInitiativeId,
            request.kpiId,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          {
            type: QueryTags.StrategicInitiativeKpi,
            id: arg.strategicInitiativeId,
          },
          { type: QueryTags.StrategicInitiativeKpi, id: arg.kpiId },
          {
            type: QueryTags.StrategicInitiativeKpiCheckpointPlan,
            id: arg.kpiId,
          },
        ]
      },
    }),
    getStrategicInitiativeKpiUnitOptions: builder.query<BaseOptionType[], void>(
      {
        queryFn: async () => {
          try {
            const units = await getStrategicInitiativesClient().getKpiUnits()

            const data: BaseOptionType[] = units
              .sort((a, b) => a.order - b.order)
              .map((category) => ({
                label: category.name,
                value: category.id,
              }))

            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
        providesTags: () => [
          { type: QueryTags.StrategicInitiativeKpiUnit, id: 'LIST' },
        ],
      },
    ),
    getStrategicInitiativeKpiTargetDirectionOptions: builder.query<
      BaseOptionType[],
      void
    >({
      queryFn: async () => {
        try {
          const targetDirections =
            await getStrategicInitiativesClient().getKpiTargetDirections()

          const data: BaseOptionType[] = targetDirections
            .sort((a, b) => a.order - b.order)
            .map((category) => ({
              label: category.name,
              value: category.id,
            }))

          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [
        { type: QueryTags.StrategicInitiativeKpiTargetDirection, id: 'LIST' },
      ],
    }),
    getStrategicInitiativeProjects: builder.query<ProjectListDto[], string>({
      queryFn: async (idOrKey) => {
        try {
          const data =
            await getStrategicInitiativesClient().getProjects(idOrKey)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.StrategicInitiativeProject, id: 'LIST' },
        { type: QueryTags.StrategicInitiativeProject, id: arg },
      ],
    }),
    manageStrategicInitiativeProjects: builder.mutation<
      void,
      ManageStrategicInitiativeProjectsRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getStrategicInitiativesClient().manageProjects(
            request.id,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.StrategicInitiativeProject, id: 'LIST' },
        { type: QueryTags.StrategicInitiativeProject, id: arg.id },
      ],
    }),
    getStrategicInitiativeKpiCheckpoints: builder.query<
      StrategicInitiativeKpiCheckpointDto[],
      { strategicInitiativeId: string; kpiId: string }
    >({
      queryFn: async (request) => {
        try {
          const data = await getStrategicInitiativesClient().getKpiCheckpoints(
            request.strategicInitiativeId,
            request.kpiId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.StrategicInitiativeKpiCheckpoint, id: arg.kpiId },
      ],
    }),
    getStrategicInitiativeKpiCheckpointPlan: builder.query<
      StrategicInitiativeKpiCheckpointDetailsDto[],
      { strategicInitiativeId: string; kpiId: string }
    >({
      queryFn: async (request) => {
        try {
          const data =
            await getStrategicInitiativesClient().getKpiCheckpointPlan(
              request.strategicInitiativeId,
              request.kpiId,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        {
          type: QueryTags.StrategicInitiativeKpiCheckpointPlan,
          id: arg.kpiId,
        },
      ],
    }),
    manageStrategicInitiativeKpiCheckpointPlan: builder.mutation<
      void,
      ManageStrategicInitiativeKpiCheckpointPlanRequest
    >({
      queryFn: async (request) => {
        try {
          const data =
            await getStrategicInitiativesClient().manageKpiCheckpointPlan(
              request.strategicInitiativeId,
              request.kpiId,
              request,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        {
          type: QueryTags.StrategicInitiativeKpiCheckpoint,
          id: arg.kpiId,
        },
        {
          type: QueryTags.StrategicInitiativeKpiCheckpointPlan,
          id: arg.kpiId,
        },
      ],
    }),
    removeStrategicInitiativeKpiMeasurement: builder.mutation<
      void,
      { strategicInitiativeId: string; kpiId: string; measurementId: string }
    >({
      queryFn: async (request) => {
        try {
          const data =
            await getStrategicInitiativesClient().removeKpiMeasurement(
              request.strategicInitiativeId,
              request.kpiId,
              request.measurementId,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        {
          type: QueryTags.StrategicInitiativeKpiMeasurement,
          id: arg.kpiId,
        },
        {
          type: QueryTags.StrategicInitiativeKpi,
          id: arg.kpiId,
        },
        {
          type: QueryTags.StrategicInitiativeKpiCheckpointPlan,
          id: arg.kpiId,
        },
      ],
    }),
  }),
})

export const {
  useGetStrategicInitiativesQuery,
  useGetStrategicInitiativeQuery,
  useCreateStrategicInitiativeMutation,
  useUpdateStrategicInitiativeMutation,
  useApproveStrategicInitiativeMutation,
  useActivateStrategicInitiativeMutation,
  useCompleteStrategicInitiativeMutation,
  useCancelStrategicInitiativeMutation,
  useDeleteStrategicInitiativeMutation,
  useGetStrategicInitiativeKpisQuery,
  useGetStrategicInitiativeKpiQuery,
  useCreateStrategicInitiativeKpiMutation,
  useUpdateStrategicInitiativeKpiMutation,
  useDeleteStrategicInitiativeKpiMutation,
  useAddStrategicInitiativeKpiMeasurementMutation,
  useGetStrategicInitiativeKpiUnitOptionsQuery,
  useGetStrategicInitiativeKpiTargetDirectionOptionsQuery,
  useGetStrategicInitiativeProjectsQuery,
  useManageStrategicInitiativeProjectsMutation,
  useGetStrategicInitiativeKpiCheckpointsQuery,
  useGetStrategicInitiativeKpiCheckpointPlanQuery,
  useManageStrategicInitiativeKpiCheckpointPlanMutation,
  useRemoveStrategicInitiativeKpiMeasurementMutation,
} = strategicInitiativesApi

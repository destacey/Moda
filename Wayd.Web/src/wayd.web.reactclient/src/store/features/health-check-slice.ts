import createCrudSlice, { CrudState } from '../crud-slice'
import {
  CreatePlanningIntervalObjectiveHealthCheckRequest,
  PlanningIntervalObjectiveHealthCheckDetailsDto,
  UpdatePlanningIntervalObjectiveHealthCheckRequest,
} from '../../services/wayd-api'
import { getPlanningIntervalsClient } from '../../services/clients'
import { CreateHealthCheckFormValues } from '../../components/common/health-check/create-health-check-form'
import { PayloadAction, createAsyncThunk } from '@reduxjs/toolkit'
import { OptionModel } from '../../components/types'
import { apiSlice } from './apiSlice'
import { QueryTags } from './query-tags'

interface HealthCheckState
  extends CrudState<PlanningIntervalObjectiveHealthCheckDetailsDto> {
  context: {
    planningIntervalId: string
    objectiveId: string
  }
  createContext: {
    planningIntervalId: string
    objectiveId: string
  }
  statusOptions: OptionModel<number>[]
}

export const getHealthCheckStatusOptions = createAsyncThunk(
  'healthCheck/getHealthCheckStatusOptions',
  async () => {
    return await getPlanningIntervalsClient().getHealthStatuses()
  },
)

const healthCheckSlice = createCrudSlice({
  name: 'healthCheck',
  initialState: (
    defaultState: CrudState<PlanningIntervalObjectiveHealthCheckDetailsDto>,
  ): HealthCheckState => ({
    ...defaultState,
    context: { planningIntervalId: '', objectiveId: '' },
    statusOptions: [],
    createContext: { planningIntervalId: '', objectiveId: '' },
  }),
  reducers: {
    beginHealthCheckCreate: (
      state,
      action: PayloadAction<{
        planningIntervalId: string
        objectiveId: string
      }>,
    ) => {
      state.createContext = action.payload
      state.detail.isInEditMode = true
    },
    cancelHealthCheckCreate: (state) => {
      state.createContext = { planningIntervalId: '', objectiveId: '' }
      state.detail.isInEditMode = false
    },
    setHealthReportContext: (
      state,
      action: PayloadAction<{
        planningIntervalId: string
        objectiveId: string
      }>,
    ) => {
      state.context = action.payload
    },
  },
  extraReducers: (builder) => {
    builder.addCase(getHealthCheckStatusOptions.fulfilled, (state, action) => {
      state.statusOptions = action.payload.map((status) => ({
        value: status.id,
        label: status.name,
      }))
    })
  },
  additionalThunkReducers: ({ createDetail }) => {
    return {
      [createDetail.fulfilled.type]: (state) => {
        state.createContext = { planningIntervalId: '', objectiveId: '' }
      },
    }
  },
  getData: async (arg, { getState, rejectWithValue }) => {
    try {
      const { healthCheck: healthCheckState } = getState() as {
        healthCheck: HealthCheckState
      }
      const { planningIntervalId, objectiveId } = healthCheckState.context
      return await getPlanningIntervalsClient().getObjectiveHealthChecks(
        planningIntervalId,
        objectiveId,
      )
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
  getDetail: async (id: string, { getState, rejectWithValue }) => {
    try {
      const { healthCheck: healthCheckState } = getState() as {
        healthCheck: HealthCheckState
      }
      const { planningIntervalId, objectiveId } = healthCheckState.context
      return await getPlanningIntervalsClient().getObjectiveHealthCheck(
        planningIntervalId,
        objectiveId,
        id,
      )
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
  createDetail: async (
    newHealthCheck: CreateHealthCheckFormValues,
    { getState, dispatch, rejectWithValue },
  ) => {
    try {
      const { healthCheck: healthCheckState } = getState() as {
        healthCheck: HealthCheckState
      }
      const { planningIntervalId, objectiveId } =
        healthCheckState.createContext
      const request: CreatePlanningIntervalObjectiveHealthCheckRequest = {
        planningIntervalObjectiveId: objectiveId,
        statusId: newHealthCheck.statusId,
        expiration: newHealthCheck.expiration,
        note: newHealthCheck.note,
      }
      const id = await getPlanningIntervalsClient().createObjectiveHealthCheck(
        planningIntervalId,
        objectiveId,
        request,
      )
      const detail = await getPlanningIntervalsClient().getObjectiveHealthCheck(
        planningIntervalId,
        objectiveId,
        id,
      )
      dispatch(
        apiSlice.util.invalidateTags([
          { type: QueryTags.PlanningIntervalObjective, id: 'LIST' },
          { type: QueryTags.HealthChecksHealthReport, id: objectiveId },
        ]),
      )
      return detail
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
  updateDetail: async (
    healthCheck: UpdatePlanningIntervalObjectiveHealthCheckRequest,
    { getState, dispatch, rejectWithValue },
  ) => {
    try {
      const { healthCheck: healthCheckState } = getState() as {
        healthCheck: HealthCheckState
      }
      const { planningIntervalId } = healthCheckState.context
      const updated = await getPlanningIntervalsClient().updateObjectiveHealthCheck(
        planningIntervalId,
        healthCheck.planningIntervalObjectiveId,
        healthCheck.id,
        healthCheck,
      )
      dispatch(
        apiSlice.util.invalidateTags([
          { type: QueryTags.PlanningIntervalObjective, id: 'LIST' },
          {
            type: QueryTags.HealthChecksHealthReport,
            id: healthCheck.planningIntervalObjectiveId,
          },
        ]),
      )
      return updated
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
})

export const {
  getData: getHealthReport,
  getDetail: getHealthCheck,
  createDetail: createHealthCheck,
  updateDetail: updateHealthCheck,
  beginHealthCheckCreate,
  cancelHealthCheckCreate,
  setEditMode,
  setHealthReportContext,
} = healthCheckSlice.actions

export const {
  selectDataContext: selectHealthReportContext,
  selectDetail: selectHealthCheck,
  selectDetailContext: selectHealthCheckContext,
  selectEditContext: selectHealthCheckEditContext,
  selectDetailIsInEditMode: selectHealthCheckIsInEditMode,
} = healthCheckSlice.selectors

export default healthCheckSlice.reducer

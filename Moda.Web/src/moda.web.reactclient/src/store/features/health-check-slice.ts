import createCrudSlice, { CrudState } from '../crud-slice'
import {
  CreateHealthCheckRequest,
  HealthCheckDto,
  UpdateHealthCheckRequest,
} from '../../services/moda-api'
import { getHealthChecksClient } from '../../services/clients'
import { CreateHealthCheckFormValues } from '../../app/components/common/health-check/create-health-check-form'
import { SystemContext } from '../../app/components/constants'
import { PayloadAction, createAsyncThunk } from '@reduxjs/toolkit'
import { OptionModel } from '../../app/components/types'

interface HealthCheckState extends CrudState<HealthCheckDto> {
  objectId?: string
  createContext: {
    objectId: string
    contextId: SystemContext | null
  }
  statusOptions: OptionModel<number>[]
}

export const getHealthCheckStatusOptions = createAsyncThunk(
  'healthCheck/getHealthCheckStatusOptions',
  async () => {
    return await (await getHealthChecksClient()).getStatuses()
  },
)

const healthCheckSlice = createCrudSlice({
  name: 'healthCheck',
  initialState: (
    defaultState: CrudState<HealthCheckDto>,
  ): HealthCheckState => ({
    ...defaultState,
    objectId: null,
    statusOptions: [],
    createContext: {
      objectId: '',
      contextId: null,
    },
  }),
  reducers: {
    beginHealthCheckCreate: (
      state,
      action: PayloadAction<{ objectId: string; contextId: SystemContext }>,
    ) => {
      state.createContext = action.payload
      state.detail.isInEditMode = true
    },
    cancelHealthCheckCreate: (state) => {
      state.createContext = {
        objectId: '',
        contextId: null,
      }
      state.detail.isInEditMode = false
    },
    setHealthReportId: (state, action) => {
      state.objectId = action.payload
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
      [createDetail.fulfilled.type]: (state, action) => {
        state.createContext = {
          objectId: '',
          contextId: null,
        }
      },
    }
  },
  getData: async (arg, { getState, rejectWithValue }) => {
    try {
      const { healthCheck: healthCheckState } = getState() as {
        healthCheck: HealthCheckState
      }
      return await (
        await getHealthChecksClient()
      ).getHealthReport(healthCheckState.objectId)
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
  getDetail: async (id: string, { rejectWithValue }) => {
    try {
      return await (await getHealthChecksClient()).getById(id)
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
  createDetail: async (
    newHealthCheck: CreateHealthCheckFormValues,
    { getState, rejectWithValue },
  ) => {
    try {
      const { healthCheck: healthCheckState } = getState() as {
        healthCheck: HealthCheckState
      }
      const healthCheck: CreateHealthCheckRequest = {
        ...newHealthCheck,
        ...healthCheckState.createContext,
      }
      const id = await (await getHealthChecksClient()).create(healthCheck)
      return await (await getHealthChecksClient()).getById(id)
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
  updateDetail: async (
    healthCheck: UpdateHealthCheckRequest,
    { rejectWithValue },
  ) => {
    try {
      await (await getHealthChecksClient()).update(healthCheck.id, healthCheck)
      return await (await getHealthChecksClient()).getById(healthCheck.id)
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
  setHealthReportId,
} = healthCheckSlice.actions

export const {
  selectDataContext: selectHealthReportContext,
  selectDetail: selectHealthCheck,
  selectDetailContext: selectHealthCheckContext,
  selectEditContext: selectHealthCheckEditContext,
  selectDetailIsInEditMode: selectHealthCheckIsInEditMode,
} = healthCheckSlice.selectors

export default healthCheckSlice.reducer

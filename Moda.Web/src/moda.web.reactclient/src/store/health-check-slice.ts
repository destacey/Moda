import createCrudSlice, { CrudState } from './crud-slice'
import {
  CreateHealthCheckRequest,
  HealthCheckDto,
  UpdateHealthCheckRequest,
} from '../services/moda-api'
import { getHealthChecksClient } from '../services/clients'
import { CreateHealthCheckFormValues } from '../app/components/common/health-check/create-health-check-form'
import { SystemContext } from '../app/components/constants'
import { PayloadAction } from '@reduxjs/toolkit'

interface HealthCheckState extends CrudState<HealthCheckDto> {
  createContext: {
    objectId: string
    contextId: SystemContext | null
  }
}

const healthCheckSlice = createCrudSlice({
  name: 'healthCheck',
  initialState: (
    defaultState: CrudState<HealthCheckDto>,
  ): HealthCheckState => ({
    ...defaultState,
    createContext: {
      objectId: '',
      contextId: null,
    },
  }),
  reducers: {
    beginHealthCheckCreate: (state, action:PayloadAction<{objectId: string, contextId: SystemContext}>) => {
      state.createContext = action.payload
      state.detail.isInEditMode = true
    },
  },
  additionalThunkReducers: ({createDetail}) => {
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
      return (await (await getHealthChecksClient()).getHealthReport('test'))
        .healthChecks
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
  getDetail: getHealthCheck,
  createDetail: createHealthCheck,
  updateDetail: updateHealthCheck,
  beginHealthCheckCreate,
  setEditMode,
} = healthCheckSlice.actions

export const {
  selectDetail: selectHealthCheck,
  selectDetailContext: selectHealthCheckContext,
  selectEditContext: selectHealthCheckEditContext,
  selectDetailIsInEditMode: selectHealthCheckIsInEditMode,
} = healthCheckSlice.selectors

export default healthCheckSlice.reducer

import { WorkProcessDto } from '@/src/services/moda-api'
import { getWorkProcessesClient } from '@/src/services/clients'
import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'

interface WorkProcessState {
  isLoading: boolean
  workProcesses: WorkProcessDto[]
  includeInactive: boolean
  error: any | null
}

const initialState: WorkProcessState = {
  isLoading: false,
  workProcesses: [],
  includeInactive: false,
  error: null,
}

// Generates pending, fulfilled, and rejected action types
export const fetchWorkProcesss = createAsyncThunk(
  'workType/fetchWorkProcesses',
  async (includeInactive: boolean) => {
    return await (await getWorkProcessesClient()).getList(includeInactive)
  },
)

const workProcessSlice = createSlice({
  name: 'workProcess',
  initialState: initialState,
  reducers: {
    setIncludeInactive: (state, action) => {
      state.includeInactive = action.payload
    },
  },
  extraReducers: (builder) => {
    builder.addCase(fetchWorkProcesss.pending, (state) => {
      state.isLoading = true
      state.error = null
    })
    builder.addCase(fetchWorkProcesss.fulfilled, (state, action) => {
      state.isLoading = false
      state.workProcesses = action.payload
      state.error = null
    })
    builder.addCase(fetchWorkProcesss.rejected, (state, action) => {
      state.isLoading = false
      state.workProcesses = []
      state.error = action.error
    })
  },
})

export const { setIncludeInactive } = workProcessSlice.actions
export default workProcessSlice.reducer

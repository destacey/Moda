import { WorkStatusDto } from '@/src/services/moda-api'
import { getWorkStatusesClient } from '@/src/services/clients'
import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'

interface WorkStatusState {
  isLoading: boolean
  workStatuses: WorkStatusDto[]
  includeInactive: boolean
  error: any | null
}

const initialState: WorkStatusState = {
  isLoading: false,
  workStatuses: [],
  includeInactive: false,
  error: null,
}

// Generates pending, fulfilled, and rejected action types
export const fetchWorkStatuses = createAsyncThunk(
  'workType/fetchWorkStatuses',
  async (includeInactive: boolean) => {
    return await (await getWorkStatusesClient()).getList(includeInactive)
  },
)

const workStatusSlice = createSlice({
  name: 'workStatus',
  initialState: initialState,
  reducers: {
    setIncludeInactive: (state, action) => {
      state.includeInactive = action.payload
    },
  },
  extraReducers: (builder) => {
    builder.addCase(fetchWorkStatuses.pending, (state) => {
      state.isLoading = true
      state.error = null
    })
    builder.addCase(fetchWorkStatuses.fulfilled, (state, action) => {
      state.isLoading = false
      state.workStatuses = action.payload
      state.error = null
    })
    builder.addCase(fetchWorkStatuses.rejected, (state, action) => {
      state.isLoading = false
      state.workStatuses = []
      state.error = action.error
    })
  },
})

export const { setIncludeInactive } = workStatusSlice.actions
export default workStatusSlice.reducer

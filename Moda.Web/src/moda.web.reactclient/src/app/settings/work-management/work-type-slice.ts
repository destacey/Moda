import { WorkTypeDto } from '@/src/services/moda-api'
import { getWorkTypesClient } from '@/src/services/clients'
import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'

interface WorkTypeState {
  isLoading: boolean
  workTypes: WorkTypeDto[]
  includeInactive: boolean
  error: any | null
}

const initialState: WorkTypeState = {
  isLoading: false,
  workTypes: [],
  includeInactive: false,
  error: null,
}

// Generates pending, fulfilled, and rejected action types
export const fetchWorkTypes = createAsyncThunk(
  'workType/fetchWorkTypes',
  async (includeInactive: boolean) => {
    return await (await getWorkTypesClient()).getList(includeInactive)
  },
)

const workTypeSlice = createSlice({
  name: 'workType',
  initialState: initialState,
  reducers: {
    setIncludeInactive: (state, action) => {
      state.includeInactive = action.payload
    },
  },
  extraReducers: (builder) => {
    builder.addCase(fetchWorkTypes.pending, (state) => {
      state.isLoading = true
    })
    builder.addCase(fetchWorkTypes.fulfilled, (state, action) => {
      state.isLoading = false
      state.workTypes = action.payload
      state.error = null
    })
    builder.addCase(fetchWorkTypes.rejected, (state, action) => {
      state.isLoading = false
      state.workTypes = []
      state.error = action.error
    })
  },
})

export const { setIncludeInactive } = workTypeSlice.actions
export default workTypeSlice.reducer

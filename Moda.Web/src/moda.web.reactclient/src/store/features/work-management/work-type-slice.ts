import { createSlice } from '@reduxjs/toolkit'

interface WorkTypeState {
  includeInactive: boolean
}

const initialState: WorkTypeState = {
  includeInactive: false,
}

const workTypeSlice = createSlice({
  name: 'workType',
  initialState: initialState,
  reducers: {
    setIncludeInactive: (state, action) => {
      state.includeInactive = action.payload
    },
  },
})

export const { setIncludeInactive } = workTypeSlice.actions
export default workTypeSlice.reducer

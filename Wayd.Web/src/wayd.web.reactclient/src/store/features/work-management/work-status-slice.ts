import { createSlice } from '@reduxjs/toolkit'

interface WorkStatusState {
  includeInactive: boolean
}

const initialState: WorkStatusState = {
  includeInactive: false,
}

const workStatusSlice = createSlice({
  name: 'workStatus',
  initialState: initialState,
  reducers: {
    setIncludeInactive: (state, action) => {
      state.includeInactive = action.payload
    },
  },
})

export const { setIncludeInactive } = workStatusSlice.actions
export default workStatusSlice.reducer

import { createSlice } from '@reduxjs/toolkit'

interface WorkProcessState {
  includeInactive: boolean
}

const initialState: WorkProcessState = {
  includeInactive: false,
}

const workProcessSlice = createSlice({
  name: 'workProcess',
  initialState: initialState,
  reducers: {
    setIncludeInactive: (state, action) => {
      state.includeInactive = action.payload
    },
  },
})

export const { setIncludeInactive } = workProcessSlice.actions
export default workProcessSlice.reducer

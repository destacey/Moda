import { createSlice } from '@reduxjs/toolkit'

interface WorkspaceState {
  includeInactive: boolean
}

const initialState: WorkspaceState = {
  includeInactive: false,
}

const workspaceSlice = createSlice({
  name: 'workSpace',
  initialState: initialState,
  reducers: {
    setIncludeInactive: (state, action) => {
      state.includeInactive = action.payload
    },
  },
})

export const { setIncludeInactive } = workspaceSlice.actions
export default workspaceSlice.reducer

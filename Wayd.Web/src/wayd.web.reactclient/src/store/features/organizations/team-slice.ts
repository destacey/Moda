import { createSlice, PayloadAction } from '@reduxjs/toolkit'

interface TeamState {
  includeInactive: boolean
}

const initialState: TeamState = {
  includeInactive: false,
}

const teamSlice = createSlice({
  name: 'team',
  initialState,
  reducers: {
    setIncludeInactive: (state, action: PayloadAction<boolean>) => {
      state.includeInactive = action.payload
    },
  },
})

export const { setIncludeInactive } = teamSlice.actions
export default teamSlice.reducer

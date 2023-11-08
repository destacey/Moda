import { configureStore } from '@reduxjs/toolkit'
import teamReducer from '../app/organizations/team-slice'
import breadcrumbReducer from './breadcrumbs/breadcrumb-slice'
import healthCheckReducer from './health-check-slice'

export const store = configureStore({
  reducer: {
    team: teamReducer,
    breadcrumb: breadcrumbReducer,
    healthCheck: healthCheckReducer,
  },
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

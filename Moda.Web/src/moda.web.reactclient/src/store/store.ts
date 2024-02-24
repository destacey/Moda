import { configureStore } from '@reduxjs/toolkit'
import logger from 'redux-logger'
import breadcrumbReducer from './breadcrumbs/breadcrumb-slice'
import healthCheckReducer from './features/health-check-slice'
import teamReducer from './features/organizations/team-slice'
import workStatusReducer from './features/work-management/work-status-slice'
import workTypeReducer from './features/work-management/work-type-slice'

const middlewares = []

if (process.env.NODE_ENV === 'development') {
  middlewares.push(logger)
}

export const store = configureStore({
  reducer: {
    team: teamReducer,
    breadcrumb: breadcrumbReducer,
    healthCheck: healthCheckReducer,
    workStatus: workStatusReducer,
    workType: workTypeReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(...middlewares),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

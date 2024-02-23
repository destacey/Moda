import { configureStore } from '@reduxjs/toolkit'
import logger from 'redux-logger'
import breadcrumbReducer from './breadcrumbs/breadcrumb-slice'
import healthCheckReducer from './health-check-slice'
import teamReducer from '../app/organizations/team-slice'
import workTypeReducer from '../app/settings/work-management/work-type-slice'

const middlewares = []

if (process.env.NODE_ENV === 'development') {
  middlewares.push(logger)
}

export const store = configureStore({
  reducer: {
    team: teamReducer,
    breadcrumb: breadcrumbReducer,
    healthCheck: healthCheckReducer,
    workType: workTypeReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(...middlewares),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

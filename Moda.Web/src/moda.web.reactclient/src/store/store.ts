import { configureStore } from '@reduxjs/toolkit'
import logger from 'redux-logger'
import breadcrumbReducer from './breadcrumbs/breadcrumb-slice'
import healthCheckReducer from './features/health-check-slice'
import teamReducer from './features/organizations/team-slice'
import workProcessReducer from './features/work-management/work-process-slice'
import workspaceReducer from './features/work-management/workspace-slice'
import workStatusReducer from './features/work-management/work-status-slice'
import workTypeReducer from './features/work-management/work-type-slice'
import { apiSlice } from './features/apiSlice'

const middlewares = []

// if (process.env.NODE_ENV === 'development') {
//   middlewares.push(logger)
// }

export const store = configureStore({
  reducer: {
    team: teamReducer,
    breadcrumb: breadcrumbReducer,
    healthCheck: healthCheckReducer,
    workProcess: workProcessReducer,
    workspace: workspaceReducer,
    workStatus: workStatusReducer,
    workType: workTypeReducer,
    [apiSlice.reducerPath]: apiSlice.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(...middlewares, apiSlice.middleware),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

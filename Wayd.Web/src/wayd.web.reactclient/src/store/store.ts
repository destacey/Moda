import { configureStore, isPlain } from '@reduxjs/toolkit'
import breadcrumbReducer from './breadcrumbs/breadcrumb-slice'
import teamReducer from './features/organizations/team-slice'
import workProcessReducer from './features/work-management/work-process-slice'
import workspaceReducer from './features/work-management/workspace-slice'
import workStatusReducer from './features/work-management/work-status-slice'
import workTypeReducer from './features/work-management/work-type-slice'
import { apiSlice } from './features/apiSlice'

// The NSwag-generated API client returns Date objects in query results, so the
// RTK Query cache already holds Dates at rest. Optimistic updateQueryData patches
// carry those same Dates through patch actions, which trips the default
// serializability check. Treat Date as serializable since it round-trips through
// JSON (toISOString / new Date(string)).
const isSerializable = (value: unknown) =>
  value instanceof Date || isPlain(value)

export const store = configureStore({
  reducer: {
    team: teamReducer,
    breadcrumb: breadcrumbReducer,
    workProcess: workProcessReducer,
    workspace: workspaceReducer,
    workStatus: workStatusReducer,
    workType: workTypeReducer,
    [apiSlice.reducerPath]: apiSlice.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: { isSerializable },
    }).concat(apiSlice.middleware),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

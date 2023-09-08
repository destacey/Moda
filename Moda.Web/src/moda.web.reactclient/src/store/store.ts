import { configureStore } from "@reduxjs/toolkit"
import teamsReducer from "../app/organizations/teams-slice"
import breadcrumbReducer from "./breadcrumbs/breadcrumb-slice"

export const store = configureStore({
    reducer: {
        teams: teamsReducer,
        breadcrumb: breadcrumbReducer
    },
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
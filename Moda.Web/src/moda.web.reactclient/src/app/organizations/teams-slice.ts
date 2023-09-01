import {createSlice, PayloadAction, createAsyncThunk, createEntityAdapter, createSelector} from '@reduxjs/toolkit'
import { FieldData } from 'rc-field-form/lib/interface'
import type {CreateTeamFormValues, EditTeamFormValues, TeamListItem, TeamType} from './types'
import { TeamDetailsDto, TeamOfTeamsDetailsDto } from '@/src/services/moda-api'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'
import { RootState } from '@/src/store'
import { toFormErrors } from '@/src/utils'
import { useAppSelector } from '../hooks'

const teamsAdapter = createEntityAdapter<TeamListItem>({
    selectId: (team) => team.key,
    // Compare teams by key
    sortComparer: (a, b) => a.key - b.key
})

// Define a type for the slice state
interface TeamsState extends ReturnType<typeof teamsAdapter.getInitialState> {
    activeTeam: TeamDetailsDto | TeamOfTeamsDetailsDto | null
    includeInactiveTeams: boolean
    isLoading: boolean
    teamsLoadingError: any | null
    activeTeamLoadingError: any | null
    activeTeamNotFound: boolean
    editTeam: {
        isOpen: boolean
        isSaving: boolean
        isLoading: boolean
        validationErrors: FieldData[]
        saveError: any | null
    }
    createTeam: {
        isOpen: boolean
        isSaving: boolean
        validationErrors: FieldData[]
        saveError: any | null
    }
}



// Define the initial state using that type
const initialState: TeamsState = teamsAdapter.getInitialState({
    activeTeam: null,
    includeInactiveTeams: false,
    isLoading: false,
    teamsLoadingError: null,
    activeTeamLoadingError: null,
    activeTeamNotFound: false,
    editTeam: {
        isOpen: false,
        isSaving: false,
        isLoading: false,
        validationErrors: [],
        saveError: null
    },
    createTeam: {
        isOpen: false,
        isSaving: false,
        validationErrors: [],
        saveError: null
    }
})

export const retrieveTeams = createAsyncThunk('teams/getTeams', async (arg, {getState, rejectWithValue}) => {
    try {
    const state = getState() as RootState
    const teams = await (await getTeamsClient()).getList(state.teams.includeInactiveTeams)
    const teamsOfTeams = await (await getTeamsOfTeamsClient()).getList(state.teams.includeInactiveTeams)
    return [...teams as TeamListItem[], ...teamsOfTeams as TeamListItem[]]
    } catch (error) {
        return rejectWithValue({error})
    }
})

export const retrieveTeam = createAsyncThunk('teams/getTeam', async (teamRequest: {key: number, type: TeamType}, {getState, rejectWithValue}) => {
    try {
        switch (teamRequest.type) {
            case 'Team':
                return await (await getTeamsClient()).getById(teamRequest.key)
            case 'Team of Teams':
                return await (await getTeamsOfTeamsClient()).getById(teamRequest.key)
        }
    } catch (error) {
        return rejectWithValue({error})
    }
})

export const refreshActiveTeam = createAsyncThunk('teams/refreshActiveTeam', async (arg, {getState, rejectWithValue}) => {
    try {
        const activeTeam = (getState() as RootState).teams.activeTeam
        if(!activeTeam) {
            return rejectWithValue({error: 'No active team to refresh'})
        }
        
        switch (activeTeam.type) {
            case 'Team':
                return await (await getTeamsClient()).getById(activeTeam.key)
            case 'Team of Teams':
                return await (await getTeamsOfTeamsClient()).getById(activeTeam.key)
        }
    } catch (error) {
        return rejectWithValue({error})
    }
})

export const createTeam = createAsyncThunk('teams/createTeam', async (newTeam: CreateTeamFormValues, {rejectWithValue}) => {
    try {
        const teamClient = newTeam.type == 'Team' 
            ? await getTeamsClient() 
            : await getTeamsOfTeamsClient()

        return await teamClient.create(newTeam)
      } catch (error) {
        if (error.status === 422 && error.errors) {
          const formErrors = toFormErrors(error.errors)
          return rejectWithValue({formErrors})
        } else {
            return rejectWithValue({error})
        }  
      }
})

export const updateTeam = createAsyncThunk('teams/updateTeam', async (team: EditTeamFormValues , {rejectWithValue}) => {
    try {
        const teamClient = team.type == 'Team' 
            ? await getTeamsClient() 
            : await getTeamsOfTeamsClient()

        return await teamClient.update(team.id, team)
      } catch (error) {
        if (error.status === 422 && error.errors) {
          const formErrors = toFormErrors(error.errors)
          return rejectWithValue({formErrors})
        } else {
            return rejectWithValue({error})
        }  
      }
})

export const teamsSlice = createSlice({
    name: 'teams',
    // `createSlice` will infer the state type from the `initialState` argument
    initialState,
    reducers: {
        setIncludeDisabled: (state, action: PayloadAction<boolean>) => {
            state.includeInactiveTeams = action.payload        
        },
        errorAcknowledged: (state) => {
            state.teamsLoadingError = null
        },
        setCreateTeamOpen: (state, action: PayloadAction<boolean>) => {
            state.createTeam.isOpen = action.payload
        },
        setEditTeamOpen: (state, action: PayloadAction<boolean>) => {
            state.editTeam.isOpen = action.payload
        },
        resetActiveTeam: (state) => {
            state.activeTeam = null
        }
    },
    extraReducers: (builder) => {
        builder
            .addCase(retrieveTeams.pending, (state) => {
                state.isLoading = true
            })
            .addCase(retrieveTeams.fulfilled, (state, action) => {
                state.isLoading = false
                teamsAdapter.setAll(state, action.payload)
            })
            .addCase(retrieveTeams.rejected, (state, action: PayloadAction<{error?: any}>) => {
                state.isLoading = false
                state.teamsLoadingError = action.payload?.error
            })
            .addCase(createTeam.pending, (state) => {
                state.createTeam.isSaving = true
                state.createTeam.validationErrors = []
                state.createTeam.saveError = null
            })
            .addCase(createTeam.fulfilled, (state) => {
                state.createTeam.isSaving = false
                state.createTeam.isOpen = false
            })
            .addCase(createTeam.rejected, (state, action: PayloadAction<{formErrors?: FieldData[], error?: any}>) => {
                state.createTeam.isSaving = false
                if (action.payload?.formErrors) {
                    state.createTeam.validationErrors = action.payload.formErrors
                }
                else if (action.payload?.error) {
                    state.createTeam.saveError = action.payload.error
                }
            })
            .addCase(updateTeam.pending, (state) => {
                state.editTeam.isSaving = true
                state.editTeam.validationErrors = []
                state.editTeam.saveError = null
            })
            .addCase(updateTeam.fulfilled, (state) => {
                state.editTeam.isSaving = false
                state.editTeam.isOpen = false
            })
            .addCase(updateTeam.rejected, (state, action: PayloadAction<{formErrors?: FieldData[], error?: any}>) => {
                state.editTeam.isSaving = false
                if (action.payload?.formErrors) {
                    state.editTeam.validationErrors = action.payload.formErrors
                }
                else if (action.payload?.error) {
                    state.editTeam.saveError = action.payload.error
                }
            })
            .addCase(retrieveTeam.pending, (state) => {
                state.editTeam.isLoading = true
                state.activeTeamLoadingError = null
                state.activeTeamNotFound = false
            })
            .addCase(retrieveTeam.fulfilled, (state, action) => {
                state.editTeam.isLoading = false
                state.activeTeam = action.payload
            })
            .addCase(retrieveTeam.rejected, (state, action: PayloadAction<{error?: any}>) => {
                state.editTeam.isLoading = false
                state.activeTeamLoadingError = action.payload?.error
                if(action.payload?.error?.status == 404) {
                    state.activeTeamNotFound = true
                }
            })
            .addCase(refreshActiveTeam.pending, (state) => {
                state.editTeam.isLoading = true
                state.activeTeamLoadingError = null
                state.activeTeamNotFound = false
            })
            .addCase(refreshActiveTeam.fulfilled, (state, action) => {
                state.editTeam.isLoading = false
                state.activeTeam = action.payload
            })
            .addCase(refreshActiveTeam.rejected, (state, action: PayloadAction<{error?: any}>) => {
                state.editTeam.isLoading = false
                state.activeTeamLoadingError = action.payload?.error
                if(action.payload?.error?.status == 404) {
                    state.activeTeamNotFound = true
                }
            })
    },
})

export const { 
    setIncludeDisabled, 
    errorAcknowledged, 
    setCreateTeamOpen, 
    setEditTeamOpen, 
    resetActiveTeam 
} = teamsSlice.actions

export const {
    selectAll: selectAllTeams,
} = teamsAdapter.getSelectors((state: RootState) => state.teams)

export const selectCreateTeamIsOpen = (state: RootState) => state.teams.createTeam.isOpen
export const selectCreateTeamIsSaving = (state: RootState) => state.teams.createTeam.isSaving
export const selectCreateTeamValidationErrors = (state: RootState) => state.teams.createTeam.validationErrors
export const selectCreateTeamSaveError = (state: RootState) => state.teams.createTeam.saveError

export const selectCreateTeam = createSelector(
    [selectCreateTeamIsOpen, selectCreateTeamIsSaving, selectCreateTeamValidationErrors, selectCreateTeamSaveError], 
    (isOpen, isSaving, validationErrors, saveError) => {
        return {isOpen, isSaving, validationErrors, saveError}
    }
)

export const selectEditTeamIsOpen = (state: RootState) => state.teams.editTeam.isOpen
export const selectEditTeamIsSaving = (state: RootState) => state.teams.editTeam.isSaving
export const selectEditTeamValidationErrors = (state: RootState) => state.teams.editTeam.validationErrors
export const selectEditTeamSaveError = (state: RootState) => state.teams.editTeam.saveError
export const selectActiveTeam = (state: RootState) => state.teams.activeTeam

export const selectEditTeam = createSelector(
    [selectEditTeamIsOpen, selectEditTeamIsSaving, selectEditTeamValidationErrors, selectEditTeamSaveError, selectActiveTeam],
    (isOpen, isSaving, validationErrors, saveError, team) => {
        return {isOpen, isSaving, validationErrors, saveError, team}
    }
)

export const selectActiveTeamIsLoading = (state: RootState) => state.teams.editTeam.isLoading
export const selectActiveTeamLoadingError = (state: RootState) => state.teams.activeTeamLoadingError
export const selectActiveTeamNotFound = (state: RootState) => state.teams.activeTeamNotFound

export const selectTeamDetail = createSelector(
    [selectActiveTeam, selectEditTeamIsOpen, selectActiveTeamIsLoading, selectActiveTeamLoadingError, selectActiveTeamNotFound],
    (team, isEditOpen, isLoading, error, teamNotFound) => {
        return {team, isEditOpen, isLoading, error, teamNotFound}
    }
)

export default teamsSlice.reducer
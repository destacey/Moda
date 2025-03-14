import createCrudSlice, { CrudState } from '@/src/store/crud-slice'
import {
  CreateTeamFormValues,
  EditTeamFormValues,
  TeamListItem,
  TeamTypeName,
} from '../../../app/organizations/types'
import { TeamDetailsDto, TeamOfTeamsDetailsDto } from '@/src/services/moda-api'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'

type TeamDetails = TeamDetailsDto | TeamOfTeamsDetailsDto
interface TeamState extends CrudState<TeamListItem, TeamDetails> {
  includeInactive: boolean
}

const teamSlice = createCrudSlice({
  name: 'team',
  createAdapterOptions: {
    selectId: (team) => team.key,
    sortComparer: (a, b) => a.name.localeCompare(b.name),
  },
  initialState: (
    defaultState: CrudState<TeamListItem, TeamDetails>,
  ): TeamState => ({
    ...defaultState,
    includeInactive: false,
  }),
  reducers: {
    setIncludeInactive: (state, action) => {
      state.includeInactive = action.payload
    },
  },
  getData: async (arg, { getState, rejectWithValue }) => {
    try {
      const { team: teamState } = getState() as { team: TeamState }
      const teams = await getTeamsClient().getList(teamState.includeInactive)
      const teamsOfTeams = await getTeamsOfTeamsClient().getList(
        teamState.includeInactive,
      )
      return [...(teams as TeamListItem[]), ...(teamsOfTeams as TeamListItem[])]
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
  getDetail: async (
    teamRequest: { key: number; type: TeamTypeName },
    { rejectWithValue },
  ) => {
    try {
      switch (teamRequest.type) {
        case 'Team':
          return await getTeamsClient().getById(teamRequest.key)
        case 'Team of Teams':
          return await getTeamsOfTeamsClient().getById(teamRequest.key)
      }
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
  refreshDetail: async (arg, { getState, rejectWithValue }) => {
    try {
      const { team: teamState } = getState() as { team: TeamState }
      const activeTeam = teamState.detail.item
      if (!activeTeam) {
        return rejectWithValue({ error: 'No active team to refresh' })
      }
      switch (activeTeam.type) {
        case 'Team':
          return await getTeamsClient().getById(activeTeam.key)
        case 'Team of Teams':
          return await getTeamsOfTeamsClient().getById(activeTeam.key)
      }
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
  createDetail: async (newTeam: CreateTeamFormValues, { rejectWithValue }) => {
    try {
      const teamClient =
        newTeam.type == 'Team' ? getTeamsClient() : getTeamsOfTeamsClient()

      const request = {
        ...newTeam,
        activeDate: (newTeam.activeDate as any)?.format('YYYY-MM-DD'),
      }

      const id = await teamClient.create(request)
      return await teamClient.getById(id)
    } catch (error) {
      console.log('API Error:', error)
      return rejectWithValue({ error })
    }
  },
  updateDetail: async (
    team: EditTeamFormValues,
    { getState, rejectWithValue },
  ) => {
    try {
      const teamClient =
        team.type == 'Team' ? getTeamsClient() : getTeamsOfTeamsClient()

      await teamClient.update(team.id, team)
      const { team: teamState } = getState() as { team: TeamState }
      return await teamClient.getById(teamState.detail.item.key)
    } catch (error) {
      return rejectWithValue({ error })
    }
  },
})

export const {
  setIncludeInactive,
  setEditMode,
  getData: retrieveTeams,
  getDetail: retrieveTeam,
  refreshDetail: refreshActiveTeam,
  createDetail: createTeam,
  updateDetail: updateTeam,
} = teamSlice.actions

export const {
  selectDataContext: selectTeamsContext,
  selectDetailContext: selectTeamContext,
  selectEditContext: selectEditTeamContext,
  selectDetailIsInEditMode: selectTeamIsInEditMode,
} = teamSlice.selectors

export default teamSlice.reducer

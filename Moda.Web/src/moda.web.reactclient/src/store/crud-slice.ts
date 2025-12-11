import {
  ActionReducerMapBuilder,
  AsyncThunkPayloadCreator,
  Draft,
  EntityId,
  EntityState,
  PayloadAction,
  SliceCaseReducers,
  ValidateSliceCaseReducers,
  createAsyncThunk,
  createEntityAdapter,
  createSelector,
  createSlice,
  AsyncThunk,
} from '@reduxjs/toolkit'
import { FieldData } from '@rc-component/form/lib/interface'
import { toFormErrors } from '../utils'
import { NoInfer } from 'react-redux'

type CrudThunkAction = AsyncThunk<any, any, any>

/**
 * Options for creating a CRUD slice using `createSlice` from Redux Toolkit.
 * @template T The type of the data being managed by the CRUD slice.
 */
export interface CreateCrudSliceOptions<
  TName extends string,
  TItem,
  TDetail extends TItem = TItem,
  State extends CrudState<TItem, TDetail> = CrudState<TItem, TDetail>,
  TArgsGetData = void,
  TArgsGetDetail = void,
  TArgsUpdateDetail = void,
  TArgsCreateDetail = void,
  TDeleteDetailArgs = void,
  TRefreshDetailArgs = void,
  CR extends SliceCaseReducers<State> = SliceCaseReducers<State>,
> {
  /**
   * The name of the slice. This will be used as the prefix for the action types.
   */
  name: TName
  createAdapterOptions?: Parameters<
    typeof createEntityAdapter<TItem, EntityId>
  >[0]
  /**
   * The initial state of the slice, if not provided the default initial state will be used.
   */
  initialState: (defaultState: CrudState<TItem, TDetail>) => State
  /**
   * Reducers for the slice.
   */
  reducers: ValidateSliceCaseReducers<State, CR>
  /**
   * Extra reducers for the slice.
   */
  extraReducers?: (builder: ActionReducerMapBuilder<NoInfer<State>>) => void
  /**
   * Async thunk action for getting a list of items.
   */
  getData: AsyncThunkPayloadCreator<TItem[], TArgsGetData> //<TArgs = void>(args: TArgs, thunkAPI: BaseThunkAPI<any, any, Dispatch, unknown>) => Promise<TItem[]> | any;
  /**
   * Async thunk action for getting a specific item.
   */
  getDetail: AsyncThunkPayloadCreator<TDetail, TArgsGetDetail>
  /**
   * Async thunk action for updating an item.
   */
  updateDetail?: AsyncThunkPayloadCreator<TDetail, TArgsUpdateDetail>
  /**
   * Async thunk action for creating a new item.
   */
  createDetail?: AsyncThunkPayloadCreator<TDetail, TArgsCreateDetail>
  /**
   * Async thunk action for refreshing the currently selected item.
   * This is optional and only needed if you want to refresh the currently selected item.
   */
  refreshDetail?: AsyncThunkPayloadCreator<TDetail, TRefreshDetailArgs>
  /**
   * Async thunk action for deleting an item.
   * This is optional and only needed if you want to delete an item.
   */
  deleteDetail?: AsyncThunkPayloadCreator<EntityId, TDeleteDetailArgs>
  additionalThunkReducers?: (params: {
    getData: CrudThunkAction
    getDetail: CrudThunkAction
    updateDetail?: CrudThunkAction
    createDetail?: CrudThunkAction
    deleteDetail?: CrudThunkAction
    refreshDetail?: CrudThunkAction
  }) => {
    [actionType: string]: (
      state: Draft<NoInfer<State>>,
      action: PayloadAction<any>,
    ) => void
  }
}

export interface CrudState<TItem, TDetail = TItem> {
  data: EntityState<TItem, EntityId>
  isLoading: boolean
  error: any | null
  detail: DetailState<TDetail>
}

export interface DetailState<T> {
  item: T | null
  isLoading: boolean
  notFound: boolean
  isSaving: boolean
  isInEditMode: boolean
  validationErrors: FieldData[]
  error: any | null
}

/**
 * Simplies creation of common CRUD slice functionality using `createSlice` from Redux Toolkit.
 * @template T The type of the data being managed by the CRUD slice.
 * @param options The options for creating the CRUD slice.
 */
const createCrudSlice = <
  TName extends string,
  TItem,
  TDetail extends TItem = TItem,
  State extends CrudState<TItem, TDetail> = CrudState<TItem, TDetail>,
  CR extends SliceCaseReducers<State> = SliceCaseReducers<State>,
  TArgsGetData = void,
  TArgsGetDetail = void,
  TArgsUpdateDetail = void,
  TArgsCreateDetail = void,
  TArgsDeleteDetail = void,
  TArgsRefreshDetail = void,
>(
  options: CreateCrudSliceOptions<
    TName,
    TItem,
    TDetail,
    State,
    TArgsGetData,
    TArgsGetDetail,
    TArgsUpdateDetail,
    TArgsCreateDetail,
    TArgsDeleteDetail,
    TArgsRefreshDetail,
    CR
  >,
) => {
  const itemsAdapter = createEntityAdapter<TItem, EntityId>(
    options.createAdapterOptions,
  )
  const initialState: State = options.initialState({
    data: itemsAdapter.getInitialState(),
    isLoading: false,
    error: null,
    detail: {
      item: null,
      isLoading: false,
      notFound: false,
      isSaving: false,
      isInEditMode: false,
      validationErrors: [],
      error: null,
    },
  })

  const getData = createAsyncThunk(`${options.name}/getData`, options.getData)
  const getDetail = createAsyncThunk(
    `${options.name}/getDetail`,
    options.getDetail,
  )
  const updateDetail =
    options.updateDetail &&
    createAsyncThunk(`${options.name}/updateDetail`, options.updateDetail)
  const createDetail =
    options.createDetail &&
    createAsyncThunk(`${options.name}/createDetail`, options.createDetail)
  const deleteDetail =
    options.deleteDetail &&
    createAsyncThunk(`${options.name}/deleteDetail`, options.deleteDetail)
  const refreshDetail =
    options.refreshDetail &&
    createAsyncThunk(`${options.name}/refreshDetail`, options.refreshDetail)

  const additionalThunkReducers =
    (options.additionalThunkReducers &&
      options.additionalThunkReducers({
        getData,
        getDetail,
        updateDetail,
        createDetail,
        deleteDetail,
        refreshDetail,
      })) ??
    {}

  const slice = createSlice({
    name: options.name,
    initialState,
    reducers: {
      setEditMode(state, action: PayloadAction<boolean>) {
        state.detail.isInEditMode = action.payload
        if (!action.payload) {
          state.detail.validationErrors = []
        }
      },
      ...options.reducers,
    },
    extraReducers: (builder) => {
      builder.addCase(getData.pending, (state, action) => {
        state.isLoading = true
        state.error = null
        additionalThunkReducers[action.type] &&
          additionalThunkReducers[action.type](state, action)
      })
      builder.addCase(getData.fulfilled, (state, action) => {
        // known issue with immer and generics: https://github.com/immerjs/use-immer/issues/72
        itemsAdapter.setAll(
          state.data as EntityState<TItem, EntityId>,
          action.payload,
        )
        state.isLoading = false
        additionalThunkReducers[action.type] &&
          additionalThunkReducers[action.type](state, action)
      })
      builder.addCase(
        getData.rejected,
        (state, action: PayloadAction<{ error?: any }>) => {
          state.isLoading = false
          state.error = action.payload.error
          additionalThunkReducers[action.type] &&
            additionalThunkReducers[action.type](state, action)
        },
      )
      builder.addCase(getDetail.pending, (state, action) => {
        state.detail.isLoading = true
        state.detail.error = null
        state.detail.notFound = false
        additionalThunkReducers[action.type] &&
          additionalThunkReducers[action.type](state, action)
      })
      builder.addCase(getDetail.fulfilled, (state, action) => {
        ;(state.detail.item as TDetail) = action.payload
        itemsAdapter.upsertOne(
          state.data as EntityState<TItem, EntityId>,
          action.payload,
        )
        state.detail.isLoading = false
        state.detail.notFound = false
        additionalThunkReducers[action.type] &&
          additionalThunkReducers[action.type](state, action)
      })
      builder.addCase(
        getDetail.rejected,
        (state, action: PayloadAction<{ error?: any }>) => {
          state.detail.isLoading = false
          state.detail.error = action.payload?.error
          if (action.payload?.error?.status === 404) {
            state.detail.notFound = true
          }
          additionalThunkReducers[action.type] &&
            additionalThunkReducers[action.type](state, action)
        },
      )
      if (updateDetail) {
        builder.addCase(updateDetail.pending, (state, action) => {
          state.detail.isSaving = true
          state.detail.error = null
          state.detail.validationErrors = []
          additionalThunkReducers[action.type] &&
            additionalThunkReducers[action.type](state, action)
        })
        builder.addCase(updateDetail.fulfilled, (state, action) => {
          ;(state.detail.item as TDetail) = action.payload
          itemsAdapter.upsertOne(
            state.data as EntityState<TItem, EntityId>,
            action.payload,
          )
          state.detail.isSaving = false
          state.detail.isInEditMode = false
          additionalThunkReducers[action.type] &&
            additionalThunkReducers[action.type](state, action)
        })
        builder.addCase(
          updateDetail.rejected,
          (state, action: PayloadAction<{ error?: any }>) => {
            state.detail.isSaving = false
            const error = action.payload?.error
            if (error?.status === 422 && error?.errors) {
              state.detail.validationErrors = toFormErrors(error.errors)
            } else {
              state.detail.error = error
            }
            additionalThunkReducers[action.type] &&
              additionalThunkReducers[action.type](state, action)
          },
        )
      }
      if (createDetail) {
        builder.addCase(createDetail.pending, (state, action) => {
          state.detail.isSaving = true
          state.detail.error = null
          state.detail.validationErrors = []
          additionalThunkReducers[action.type] &&
            additionalThunkReducers[action.type](state, action)
        })
        builder.addCase(createDetail.fulfilled, (state, action) => {
          itemsAdapter.addOne(
            state.data as EntityState<TItem, EntityId>,
            action.payload,
          )
          state.detail.isSaving = false
          state.detail.isInEditMode = false
          additionalThunkReducers[action.type] &&
            additionalThunkReducers[action.type](state, action)
        })
        builder.addCase(
          createDetail.rejected,
          (state, action: PayloadAction<{ error?: any }>) => {
            state.detail.isSaving = false
            const error = action.payload?.error
            if (error?.status === 422 && error?.errors) {
              state.detail.validationErrors = toFormErrors(error.errors)
            } else {
              state.detail.error = error
            }
            additionalThunkReducers[action.type] &&
              additionalThunkReducers[action.type](state, action)
          },
        )
      }
      if (deleteDetail) {
        builder.addCase(deleteDetail.pending, (state, action) => {
          state.detail.isSaving = true
          state.detail.error = null
          additionalThunkReducers[action.type] &&
            additionalThunkReducers[action.type](state, action)
        })
        builder.addCase(deleteDetail.fulfilled, (state, action) => {
          itemsAdapter.removeOne(
            state.data as EntityState<TItem, EntityId>,
            action.payload,
          )
          state.detail.item = null
          state.detail.isSaving = false
          additionalThunkReducers[action.type] &&
            additionalThunkReducers[action.type](state, action)
        })
        builder.addCase(
          deleteDetail.rejected,
          (state, action: PayloadAction<{ error?: any }>) => {
            state.detail.isSaving = false
            state.detail.error = action.payload?.error
            additionalThunkReducers[action.type] &&
              additionalThunkReducers[action.type](state, action)
          },
        )
        if (refreshDetail) {
          builder.addCase(refreshDetail.pending, (state, action) => {
            state.detail.isLoading = true
            state.detail.error = null
            additionalThunkReducers[action.type] &&
              additionalThunkReducers[action.type](state, action)
          })
          builder.addCase(refreshDetail.fulfilled, (state, action) => {
            ;(state.detail.item as TDetail) = action.payload
            itemsAdapter.upsertOne(
              state.data as EntityState<TItem, EntityId>,
              action.payload,
            )
            state.detail.isLoading = false
            additionalThunkReducers[action.type] &&
              additionalThunkReducers[action.type](state, action)
          })
          builder.addCase(
            refreshDetail.rejected,
            (state, action: PayloadAction<{ error?: any }>) => {
              state.detail.isLoading = false
              state.detail.error = action.payload?.error
              additionalThunkReducers[action.type] &&
                additionalThunkReducers[action.type](state, action)
            },
          )
        }
      }
      options.extraReducers && options.extraReducers(builder)
    },
  })
  const allSelectors = {
    selectData: itemsAdapter.getSelectors(
      (state: any): any => state[options.name].data,
    ).selectAll,
    selectIsLoading: (state: any) => state[options.name].isLoading,
    selectError: (state: any) => state[options.name].error,
    selectDetail: (state: any) => state[options.name].detail.item,
    selectDetailIsLoading: (state: any) => state[options.name].detail.isLoading,
    selectDetailNotFound: (state: any) => state[options.name].detail.notFound,
    selectDetailError: (state: any) => state[options.name].detail.error,
    selectDetailIsSaving: (state: any) => state[options.name].detail.isSaving,
    selectDetailIsInEditMode: (state: any) =>
      state[options.name].detail.isInEditMode,
    selectDetailValidationErrors: (state: any) =>
      state[options.name].detail.validationErrors,
  }

  return {
    slice,
    actions: {
      getData,
      getDetail,
      updateDetail,
      createDetail,
      deleteDetail,
      refreshDetail,
      ...slice.actions,
    },
    selectors: {
      ...allSelectors,
      selectDataContext: createSelector(
        [
          allSelectors.selectData,
          allSelectors.selectIsLoading,
          allSelectors.selectError,
        ],
        (data, isLoading, error) => ({ data, isLoading, error }),
      ),
      selectEditContext: createSelector(
        [
          allSelectors.selectDetail,
          allSelectors.selectDetailIsLoading,
          allSelectors.selectDetailNotFound,
          allSelectors.selectDetailError,
          allSelectors.selectDetailIsSaving,
          allSelectors.selectDetailIsInEditMode,
          allSelectors.selectDetailValidationErrors,
        ],
        (
          item,
          isLoading,
          notFound,
          error,
          isSaving,
          isInEditMode,
          validationErrors,
        ) => ({
          item,
          isLoading,
          notFound,
          error,
          isSaving,
          isInEditMode,
          validationErrors,
        }),
      ),
      selectDetailContext: createSelector(
        [
          allSelectors.selectDetail,
          allSelectors.selectDetailIsLoading,
          allSelectors.selectDetailNotFound,
          allSelectors.selectDetailError,
          allSelectors.selectDetailIsInEditMode,
        ],
        (item, isLoading, notFound, error, isInEditMode) => ({
          item,
          isLoading,
          notFound,
          error,
          isInEditMode,
        }),
      ),
    },
    reducer: slice.reducer,
  }
}

export default createCrudSlice

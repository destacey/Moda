import { useDispatch, useSelector, TypedUseSelectorHook } from 'react-redux'
import { AppDispatch, RootState } from '../../store'

export { useLocalStorageState } from './use-local-storage-state'
export { useDocumentTitle } from './use-document-title'

export const useAppDispatch: () => AppDispatch = useDispatch
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector
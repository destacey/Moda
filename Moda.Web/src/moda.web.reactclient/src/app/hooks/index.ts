import { useDispatch, useSelector } from 'react-redux'
import type { TypedUseSelectorHook } from 'react-redux'
import type { AppDispatch, RootState } from '@/src/store'

export { useLocalStorageState } from './use-local-storage-state'
export { useDocumentTitle } from './use-document-title'

export const useAppDispatch: () => AppDispatch = useDispatch
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector

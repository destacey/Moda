import { useDispatch, useSelector } from 'react-redux'
import type { TypedUseSelectorHook } from 'react-redux'
import type { AppDispatch, RootState } from '@/src/store'

export { useLocalStorageState } from './use-local-storage-state'
export { useDocumentTitle } from './use-document-title'
export { useDebounce } from './use-debounce'
export { default as useModalForm } from './use-modal-form'
export type {
  UseModalFormOptions,
  UseModalFormReturn,
} from './use-modal-form'
export { default as useConfirmModal } from './use-confirm-modal'
export type {
  UseConfirmModalOptions,
  UseConfirmModalReturn,
} from './use-confirm-modal'

export const useAppDispatch: () => AppDispatch = useDispatch
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector

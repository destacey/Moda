import type { FormInstance } from 'antd'

/**
 * Minimum shape that any tree node must satisfy.
 * Consumers extend this for their domain types.
 */
export interface TreeNode {
  id: string
  children: TreeNode[]
  parentId?: string | null
}

/**
 * Flattened tree node with DnD metadata. Generic over the node type.
 * Wraps the original node as `node: T` to avoid type conflicts when spreading.
 */
export interface FlattenedTreeNode<T extends TreeNode = TreeNode> {
  node: T
  depth: number
  parentId: string | null
  ancestorIds: string[]
  flatIndex: number
}

/**
 * Result of projecting where a dragged item will land.
 */
export interface DragProjection<T extends TreeNode = TreeNode> {
  depth: number
  maxDepth: number
  minDepth: number
  parentId: string | null
  parentNode: T | null
  canDrop: boolean
  reason?: string
}

/**
 * Validation function for domain-specific move rules.
 * The generic layer calls this; consumers supply domain logic.
 */
export type MoveValidator<T extends TreeNode = TreeNode> = (
  activeNode: FlattenedTreeNode<T>,
  targetParentNode: T | null,
  targetParentId: string | null,
) => { canMove: boolean; reason?: string }

/**
 * Draft item for inline creation.
 */
export interface DraftItem {
  id: string
  parentId?: string
  order: number
}

/**
 * Filter option for select-type column filters.
 */
export interface FilterOption {
  label: string
  value: string
}

/**
 * Arguments passed to the row click handler.
 */
export interface RowClickArgs {
  rowId: string
  isEditableColumn: (columnId: string) => boolean
  getClickedColumnId: (target: HTMLElement) => string | null
}

/**
 * Configuration for the inline editing hook.
 */
export interface TreeGridEditingConfig<T extends TreeNode> {
  /** The tree data (including any merged drafts). */
  data: T[]

  /** Whether the user has permission to edit. */
  canEdit: boolean

  /** Ant Design Form instance for managing inline form state. */
  form: FormInstance

  /** CSS class name of the table wrapper element (used for click-outside detection). */
  tableWrapperClassName: string

  /**
   * Which columns are editable. Can be a static list or a function
   * that returns different columns based on the selected row (e.g., drafts may have extra editable fields).
   */
  editableColumnIds: string[] | ((selectedRowId: string | null) => string[])

  /**
   * Called to save changes for a row. Returns true on success, false on failure.
   * For draft rows, this creates a new item. For existing rows, this patches the item.
   */
  onSave: (rowId: string, updates: Record<string, any>) => Promise<boolean>

  /** Current field-level validation errors. */
  fieldErrors: Record<string, string>

  /** Setter for field-level validation errors. */
  setFieldErrors: (errors: Record<string, string>) => void

  /**
   * Given a row ID and the current data, return the initial form values.
   * Domain-specific: the generic hook doesn't know field shapes.
   */
  getFormValues: (rowId: string, data: T[]) => Record<string, any>

  /**
   * Given a row ID, current form values, and original data, compute
   * the changed fields. Returns null if nothing changed (skip save).
   * For drafts, typically return all form values.
   */
  computeChanges: (
    rowId: string,
    formValues: Record<string, any>,
    data: T[],
  ) => Record<string, any> | null

  /**
   * Optional client-side cross-field validation before save.
   * Return field errors or empty object.
   */
  validateFields?: (
    rowId: string,
    formValues: Record<string, any>,
  ) => Record<string, string>

  /**
   * Column IDs ordered for suffix matching (longest-suffix first).
   * Used by focusCellById to identify which column a cell ID refers to.
   * Example: ['estimatedEffortHours', 'plannedStart', 'plannedEnd', 'assignees', 'priority', 'status', 'progress', 'type', 'name']
   */
  cellIdColumnMatchOrder: readonly string[]

  /** Called when a draft row is cancelled (Escape on a draft). */
  onCancelDraft?: (rowId: string) => void

  /** Prefix used to identify draft rows (default: 'draft-'). */
  draftPrefix?: string
}

/**
 * Props for the generic TreeGridToolbar.
 */
export interface TreeGridToolbarProps {
  displayedRowCount: number
  totalRowCount: number
  searchValue: string
  onSearchChange: (e: React.ChangeEvent<HTMLInputElement>) => void
  onRefresh?: () => Promise<any>
  onClearFilters: () => void
  hasActiveFilters: boolean
  onExportCsv?: () => void
  isLoading: boolean
  /** Slot for domain-specific actions (e.g., create button) rendered on the left. */
  leftSlot?: React.ReactNode
  /** Content rendered inside the help popover. */
  helpContent?: React.ReactNode
}

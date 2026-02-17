import type { ColumnDef } from '@tanstack/react-table'
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
  /** Slot for actions rendered on the far right of the toolbar (e.g., view selector). */
  rightSlot?: React.ReactNode
}

/**
 * Extended column metadata for TreeGrid.
 * Stored in TanStack's `columnDef.meta` field.
 */
export interface TreeGridColumnMeta {
  /** Filter UI type. Omit to use text input (the default when column has filtering enabled). */
  filterType?: 'text' | 'select' | 'numericRange'
  /** Options for 'select' filter type. */
  filterOptions?: FilterOption[]
  /** Placeholder text for text/numericRange filter inputs. */
  filterPlaceholder?: string
  /** Whether to include this column in CSV export. Default: true if column has an accessor. */
  enableExport?: boolean
  /** Custom CSV formatter for this column's values. */
  exportFormatter?: (value: unknown, row: any) => string
  /** Override the CSV header text for this column. */
  exportHeader?: string
}

/**
 * Context passed to the `columns` and `leftSlot` function props of TreeGrid.
 * Provides editing, DnD, and draft state so domain code can build columns reactively.
 */
export interface TreeGridColumnContext {
  selectedRowId: string | null
  handleKeyDown: (
    e: React.KeyboardEvent,
    rowId: string,
    columnId: string,
  ) => Promise<void>
  getFieldError: (fieldName: string) => string | undefined
  editableColumns: string[]
  isDragEnabled: boolean
  canCreateDraft: boolean
  addDraftAtRoot: () => string | null
  addDraftAsChild: (parentId: string) => string | null
}

/**
 * Inline editing configuration for TreeGrid consumers.
 * TreeGrid fills in `data`, `tableWrapperClassName`, `fieldErrors`, `setFieldErrors`,
 * and `onCancelDraft` internally.
 */
export interface TreeGridInlineEditingConfig<T extends TreeNode> {
  canEdit: boolean
  form: FormInstance
  editableColumnIds: string[] | ((selectedRowId: string | null) => string[])
  onSave: (rowId: string, updates: Record<string, any>) => Promise<boolean>
  getFormValues: (rowId: string, data: T[]) => Record<string, any>
  computeChanges: (
    rowId: string,
    formValues: Record<string, any>,
    data: T[],
  ) => Record<string, any> | null
  validateFields?: (
    rowId: string,
    formValues: Record<string, any>,
  ) => Record<string, string>
  cellIdColumnMatchOrder: readonly string[]
  draftPrefix?: string
}

/**
 * Props for the TreeGrid component.
 */
export interface TreeGridProps<T extends TreeNode> {
  /** Tree data (without drafts — TreeGrid merges drafts internally). */
  data: T[]
  /** How to extract child rows. Default: `(row) => row.children as T[]` */
  getSubRows?: (row: T) => T[]
  /** Loading state. */
  isLoading: boolean
  /**
   * Column definitions. Can be a static array or a function that receives
   * editing/DnD/draft context and returns columns.
   */
  columns:
    | ColumnDef<T, any>[]
    | ((context: TreeGridColumnContext) => ColumnDef<T, any>[])

  // -- Toolbar --
  onRefresh?: () => Promise<any>
  /**
   * Left slot for the toolbar. Can be a ReactNode or a function receiving context
   * (useful for Create buttons that need `canCreateDraft` / `addDraftAtRoot`).
   */
  leftSlot?:
    | React.ReactNode
    | ((context: TreeGridColumnContext) => React.ReactNode)
  helpContent?: React.ReactNode
  /** Slot for actions rendered on the far right of the toolbar (e.g., view selector). */
  rightSlot?: React.ReactNode
  emptyMessage?: string
  /** File name prefix for CSV export (e.g., 'project-tasks'). */
  csvFileName?: string

  // -- DnD (enabled when onNodeMove is provided) --
  enableDragAndDrop?: boolean
  /** Called when a node is moved via DnD. Receives the node ID, new parent ID, and order. */
  onNodeMove?: (
    nodeId: string,
    parentId: string | null,
    order: number,
  ) => Promise<void>
  /** Called when a DnD move is rejected by the projection/validator. */
  onMoveRejected?: (reason: string) => void
  moveValidator?: MoveValidator<T>

  // -- Inline editing (enabled when editingConfig is provided) --
  editingConfig?: TreeGridInlineEditingConfig<T>
  /** External field-level validation errors (e.g., from API 422 responses). */
  fieldErrors?: Record<string, string>
  /** Called when field errors change (cleared on successful validation, set on failure). */
  onFieldErrorsChange?: (errors: Record<string, string>) => void

  // -- Drafts (enabled when createDraftNode is provided) --
  /** Factory to create a full tree node from a draft item. */
  createDraftNode?: (draft: DraftItem) => T
  /** Called when a draft is cancelled (e.g., Escape key on a draft row). */
  onDraftCancelled?: (draftId: string) => void
  /** Called when the internal draft list changes. */
  onDraftsChange?: (drafts: DraftItem[]) => void
}

/**
 * Handle exposed by TreeGrid via ref.
 */
export interface TreeGridHandle {
  /** The TanStack table instance. */
  table: any
  /** The currently selected row ID (from editing hook), or null. */
  selectedRowId: string | null
}

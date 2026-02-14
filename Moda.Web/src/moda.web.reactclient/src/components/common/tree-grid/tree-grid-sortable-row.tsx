'use client'

import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { createContext, useContext, CSSProperties, ReactNode } from 'react'

// Context to share drag listeners with child components (drag handle)
const TreeGridDragHandleContext = createContext<{
  listeners?: any
  attributes?: any
} | null>(null)

/**
 * Hook to access drag handle listeners and attributes.
 * Must be used within a TreeGridSortableRow.
 */
export function useTreeGridDragHandle() {
  const context = useContext(TreeGridDragHandleContext)
  if (!context) {
    throw new Error(
      'useTreeGridDragHandle must be used within TreeGridSortableRow',
    )
  }
  return context
}

interface TreeGridSortableRowProps {
  nodeId: string
  isDragEnabled: boolean
  isDragging?: boolean
  className?: string
  onClick?: (e: React.MouseEvent<HTMLTableRowElement>) => void
  children: ReactNode
}

/**
 * Sortable table row wrapper for drag-and-drop functionality.
 * Uses @dnd-kit/sortable to make table rows draggable via a drag handle.
 */
export function TreeGridSortableRow({
  nodeId,
  isDragEnabled,
  isDragging: parentIsDragging,
  className = '',
  onClick,
  children,
}: TreeGridSortableRowProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: nodeId,
    disabled: !isDragEnabled,
  })

  const style: CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging || parentIsDragging ? 0.4 : 1,
    position: 'relative',
    zIndex: isDragging ? 999 : 'auto',
  }

  return (
    <TreeGridDragHandleContext.Provider value={{ listeners, attributes }}>
      <tr
        ref={setNodeRef}
        style={style}
        className={className}
        onClick={onClick}
        data-row-id={nodeId}
        data-dragging={isDragging}
      >
        {children}
      </tr>
    </TreeGridDragHandleContext.Provider>
  )
}

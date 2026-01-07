'use client'

import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { createContext, useContext, CSSProperties, ReactNode } from 'react'

// Context to share drag listeners with child components (drag handle)
const DragHandleContext = createContext<{
  listeners?: any
  attributes?: any
} | null>(null)

export function useDragHandle() {
  const context = useContext(DragHandleContext)
  if (!context) {
    throw new Error('useDragHandle must be used within ProjectTaskSortableRow')
  }
  return context
}

interface ProjectTaskSortableRowProps {
  taskId: string
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
export function ProjectTaskSortableRow({
  taskId,
  isDragEnabled,
  isDragging: parentIsDragging,
  className = '',
  onClick,
  children,
}: ProjectTaskSortableRowProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: taskId,
    disabled: !isDragEnabled,
  })

  // Combine transform and transition for smooth dragging
  const style: CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging || parentIsDragging ? 0.4 : 1,
    position: 'relative',
    zIndex: isDragging ? 999 : 'auto',
  }

  return (
    <DragHandleContext.Provider value={{ listeners, attributes }}>
      <tr
        ref={setNodeRef}
        style={style}
        className={className}
        onClick={onClick}
        data-row-id={taskId}
        data-dragging={isDragging}
      >
        {children}
      </tr>
    </DragHandleContext.Provider>
  )
}


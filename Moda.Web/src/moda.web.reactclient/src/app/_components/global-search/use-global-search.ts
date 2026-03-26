'use client'

import { useCallback, useEffect, useRef, useState } from 'react'

export type SearchScope = 'app' | 'docs'

export function useGlobalSearch() {
  const [open, setOpen] = useState(false)
  const requestedScopeRef = useRef<SearchScope | null>(null)

  const openSearch = useCallback(() => setOpen(true), [])
  const closeSearch = useCallback(() => setOpen(false), [])

  // Ctrl+K opens modal (if closed) and sets scope to 'app'
  // If already open, the modal's own keydown handler catches Ctrl+K to switch scope
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault()
        if (!open) {
          requestedScopeRef.current = 'app'
          setOpen(true)
        }
        // When open, let the modal handle Ctrl+K for scope switching
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [open])

  /**
   * Returns the scope requested by the keyboard shortcut (if any),
   * then clears it so it's only consumed once.
   */
  const consumeRequestedScope = useCallback((): SearchScope | null => {
    const scope = requestedScopeRef.current
    requestedScopeRef.current = null
    return scope
  }, [])

  return { open, openSearch, closeSearch, consumeRequestedScope }
}

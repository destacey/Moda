import { useContext } from 'react'
import { BreadcrumbsContextType } from './types'
import { BreadcrumbsContext } from './breadcrumbs-context'

const useBreadcrumbs = (): BreadcrumbsContextType => {
  const context = useContext(BreadcrumbsContext)
  if (!context) {
    throw new Error('useBreadcrumbs must be used within an BreadcrumbProvider')
  }
  return context
}

export default useBreadcrumbs

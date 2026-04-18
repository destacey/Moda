export interface BreadcrumbState {
  items: BreadcrumbItem[]
  isVisible: boolean
  forPath: string
}

export interface BreadcrumbItem {
  title: string
  href?: string
  path?: string
  key?: string | number
  className?: string
  dropdownProps?: any
  onClick?: (e?: React.MouseEvent<HTMLAnchorElement>) => void
  // Add index signature for data attributes
  [key: `data-${string}`]: any
}

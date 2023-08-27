'use client'

import { Result } from 'antd'
import useBreadcrumbs from './components/contexts/breadcrumbs'

export default function NotFound() {
  const { setBreadcrumbIsVisible } = useBreadcrumbs()

  setBreadcrumbIsVisible(false)
  return (
    <Result status="404" title="404" subTitle="Resource not found"></Result>
  )
}

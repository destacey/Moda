'use client'

import { Result } from 'antd'
import useBreadcrumbs from './components/contexts/breadcrumbs'
import { useEffect } from 'react'

export default function NotFound() {
  const { setBreadcrumbIsVisible } = useBreadcrumbs()

  useEffect(() => {
    setBreadcrumbIsVisible(false)
  }, [setBreadcrumbIsVisible])

  return (
    <Result status="404" title="404" subTitle="Resource not found"></Result>
  )
}

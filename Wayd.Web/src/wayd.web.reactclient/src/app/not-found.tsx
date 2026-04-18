'use client'

import { Result } from 'antd'
import { useEffect } from 'react'
import { usePathname } from 'next/navigation'
import { useAppDispatch } from '../hooks'
import { disableBreadcrumb } from '../store/breadcrumbs'

// This is the global 404 Not Found page for handling missing resources. It should not be
// used by components routing 404s, which are handled by Next.js automatically. Components
// should use the `notFound` function from 'next/navigation' to trigger a 404.
export default function NotFound() {
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  useEffect(() => {
    dispatch(disableBreadcrumb(pathname))
  }, [dispatch, pathname])

  return (
    <Result status="404" title="404" subTitle="Resource not found"></Result>
  )
}

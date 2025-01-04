'use client'

import { Result } from 'antd'
import { useEffect } from 'react'
import { usePathname } from 'next/navigation'
import { useAppDispatch } from '../hooks'
import { disableBreadcrumb } from '../store/breadcrumbs'

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

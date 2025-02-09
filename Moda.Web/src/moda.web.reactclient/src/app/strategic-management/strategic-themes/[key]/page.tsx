'use client'

import { PageTitle } from '@/src/components/common'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { useGetStrategicThemeQuery } from '@/src/store/features/strategic-management/strategic-themes-api'
import { Descriptions, message, Space } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import StrategicThemeDetailsLoading from './loading'
import { useEffect } from 'react'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'

const { Item } = Descriptions

const StrategicThemeDetailsPage = ({ params }) => {
  useDocumentTitle('Strategic Theme Details')

  const [messageApi, contextHolder] = message.useMessage()

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canUpdateStrategicTheme = hasPermissionClaim(
    'Permissions.StrategicThemes.Update',
  )
  const canDeleteStrategicTheme = hasPermissionClaim(
    'Permissions.StrategicThemes.Delete',
  )

  const {
    data: strategicThemeData,
    isLoading,
    error,
    refetch: refetchStrategicTheme,
  } = useGetStrategicThemeQuery(params.key)

  useEffect(() => {
    if (!strategicThemeData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Stratgic Management',
      },
      {
        href: `/stratgic-management/strategic-themes`,
        title: 'StrategicTheme',
      },
    ]

    breadcrumbRoute.push({
      title: 'Details',
    })

    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname, strategicThemeData])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  if (isLoading) {
    return <StrategicThemeDetailsLoading />
  }

  if (!strategicThemeData) {
    notFound()
  }

  return (
    <>
      {contextHolder}
      <PageTitle
        title={`${strategicThemeData?.key} - ${strategicThemeData?.name}`}
        subtitle="Strategic Theme Details"
        // actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Space direction="vertical" size="small">
        <Descriptions>
          <Item label="State">{strategicThemeData.state.name}</Item>
        </Descriptions>
        <Descriptions layout="vertical" size="small">
          <Item label="Description">
            <MarkdownRenderer markdown={strategicThemeData.name} />
          </Item>
        </Descriptions>
      </Space>
    </>
  )
}

const StrategicThemeDetailsPageWithAuthorization = authorizePage(
  StrategicThemeDetailsPage,
  'Permission',
  'Permissions.StrategicThemes.View',
)

export default StrategicThemeDetailsPageWithAuthorization

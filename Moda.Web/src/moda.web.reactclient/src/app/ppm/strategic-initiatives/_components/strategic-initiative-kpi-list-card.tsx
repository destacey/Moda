'use client'

import { ContainerCard, ModaEmpty } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { useGetStrategicInitiativeKpisQuery } from '@/src/store/features/ppm/strategic-initiatives-api'
import { PlusOutlined } from '@ant-design/icons'
import { Button, List, message } from 'antd'
import { FC, useCallback, useMemo, useState } from 'react'
import { CreateStrategicInitiativeKpiForm } from '.'

export interface StrategicInitiativeKpiListCardProps {
  strategicInitiativeId: string
}

const StrategicInitiativeKpiListCard: FC<
  StrategicInitiativeKpiListCardProps
> = ({ strategicInitiativeId }) => {
  const [openCreateKpiForm, setOpenCreateKpiForm] = useState(false)

  const [messageApi, contextHolder] = message.useMessage()

  const {
    data: kpiData,
    isLoading,
    error,
    refetch,
  } = useGetStrategicInitiativeKpisQuery(strategicInitiativeId, {
    skip: !strategicInitiativeId,
  })

  const { hasPermissionClaim } = useAuth()
  const canCreateKpis = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Update',
  )

  const actions = useMemo(() => {
    if (!canCreateKpis) return null

    return (
      <Button
        type="text"
        icon={<PlusOutlined />}
        onClick={() => setOpenCreateKpiForm(true)}
      />
    )
  }, [canCreateKpis])

  const onCreateKpiFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCreateKpiForm(false)
      if (wasSaved) {
        refetch()
      }
    },
    [refetch],
  )

  if (!strategicInitiativeId) return null

  return (
    <>
      {contextHolder}
      <ContainerCard title="KPIs" count={kpiData?.length} actions={actions}>
        <List
          size="small"
          dataSource={kpiData}
          loading={isLoading}
          locale={{
            emptyText: <ModaEmpty message="No KPIs" />,
          }}
          renderItem={(kpi) => (
            <List.Item key={kpi.id}>
              {kpi.key} - {kpi.name}
            </List.Item>
          )}
        />
      </ContainerCard>
      {openCreateKpiForm && (
        <CreateStrategicInitiativeKpiForm
          strategicInitiativeId={strategicInitiativeId}
          showForm={openCreateKpiForm}
          onFormComplete={() => onCreateKpiFormClosed(true)}
          onFormCancel={() => onCreateKpiFormClosed(false)}
          messageApi={messageApi}
        />
      )}
    </>
  )
}

export default StrategicInitiativeKpiListCard

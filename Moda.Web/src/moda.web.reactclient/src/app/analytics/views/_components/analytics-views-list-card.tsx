'use client'

import { AnalyticsViewListDto, Visibility } from '@/src/services/moda-api'
import { Button, Card, Checkbox, Form, Table, Tag } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import type { FormInstance } from 'antd/es/form'
import { useMemo } from 'react'
import { FormValues } from '../types'

interface AnalyticsViewsListCardProps {
  form: FormInstance<FormValues>
  canCreate: boolean
  isViewsLoading: boolean
  viewsData?: AnalyticsViewListDto[]
  onResetForCreate: () => void
  onSelectView: (id: string) => void
  onIncludeInactiveChanged: () => void
}

const AnalyticsViewsListCard = ({
  form,
  canCreate,
  isViewsLoading,
  viewsData,
  onResetForCreate,
  onSelectView,
  onIncludeInactiveChanged,
}: AnalyticsViewsListCardProps) => {
  const viewColumns = useMemo<ColumnsType<AnalyticsViewListDto>>(
    () => [
      { title: 'Name', dataIndex: 'name', key: 'name' },
      { title: 'Dataset', dataIndex: 'dataset', key: 'dataset' },
      {
        title: 'Visibility',
        dataIndex: 'visibility',
        key: 'visibility',
        render: (value: Visibility) => (
          <Tag color={value === Visibility.Public ? 'blue' : 'default'}>
            {value}
          </Tag>
        ),
      },
      {
        title: 'Active',
        dataIndex: 'isActive',
        key: 'isActive',
        render: (value: boolean) => (value ? 'Yes' : 'No'),
      },
    ],
    [],
  )

  return (
    <Card
      title="Views"
      extra={
        <Button type="primary" onClick={onResetForCreate} disabled={!canCreate}>
          New
        </Button>
      }
    >
      <Form form={form} layout="inline" style={{ marginBottom: 12 }}>
        <Form.Item
          name="includeInactive"
          valuePropName="checked"
          label="Include inactive"
        >
          <Checkbox onChange={onIncludeInactiveChanged} />
        </Form.Item>
      </Form>

      <Table<AnalyticsViewListDto>
        rowKey="id"
        loading={isViewsLoading}
        columns={viewColumns}
        dataSource={viewsData}
        size="small"
        pagination={{ pageSize: 8 }}
        onRow={(record) => ({
          onClick: () => onSelectView(record.id),
        })}
      />
    </Card>
  )
}

export default AnalyticsViewsListCard

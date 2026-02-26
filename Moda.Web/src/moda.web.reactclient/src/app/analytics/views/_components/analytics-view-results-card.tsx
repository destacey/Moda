'use client'

import { AnalyticsViewDataResultDto } from '@/src/store/features/analytics/analytics-views-api'
import { Button, Card, InputNumber, Space, Table, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { useMemo } from 'react'

const { Text, Title } = Typography

interface AnalyticsViewResultsCardProps {
  data: AnalyticsViewDataResultDto | null
  page: number
  pageSize: number
  isLoading: boolean
  onPrev: () => void
  onNext: () => void
  onPageSizeChanged: (size: number) => void
  onRefresh: () => void
}

const AnalyticsViewResultsCard = ({
  data,
  page,
  pageSize,
  isLoading,
  onPrev,
  onNext,
  onPageSizeChanged,
  onRefresh,
}: AnalyticsViewResultsCardProps) => {
  const columns = data?.columns
  const rows = data?.rows

  const resultColumns = useMemo<ColumnsType<Record<string, unknown>>>(() => {
    if (!columns?.length) return []

    return columns.map((col) => ({
      title: col.displayName,
      dataIndex: col.displayName,
      key: col.field,
      ellipsis: true,
      render: (value: unknown) =>
        typeof value === 'object'
          ? JSON.stringify(value)
          : String(value ?? ''),
    }))
  }, [columns])

  const resultRows = useMemo(() => {
    if (!rows) return []
    return rows.map((row, idx) => ({ ...row, key: row.Id ?? `${idx}` }))
  }, [rows])

  const totalRows = data?.totalCount ?? 0
  const hasNextPage = page * pageSize < totalRows

  return (
    <Card style={{ marginTop: 16 }} title="Results">
      <Space style={{ marginBottom: 12 }}>
        <Text>Rows: {totalRows}</Text>
        <Button onClick={onPrev} disabled={page <= 1 || !data}>
          Prev
        </Button>
        <Button onClick={onNext} disabled={!hasNextPage}>
          Next
        </Button>
        <InputNumber
          min={1}
          max={500}
          value={pageSize}
          onChange={(value) => onPageSizeChanged(Number(value) || 25)}
        />
        <Button onClick={onRefresh} loading={isLoading}>
          Refresh
        </Button>
      </Space>

      {data ? (
        <Table
          size="small"
          columns={resultColumns}
          dataSource={resultRows}
          pagination={false}
          scroll={{ x: true }}
        />
      ) : (
        <Title level={5} style={{ margin: 0 }}>
          Run to view results.
        </Title>
      )}
    </Card>
  )
}

export default AnalyticsViewResultsCard

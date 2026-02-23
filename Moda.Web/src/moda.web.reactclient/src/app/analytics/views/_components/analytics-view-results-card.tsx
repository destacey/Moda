'use client'

import { AnalyticsViewResultDto } from '@/src/services/moda-api'
import { Button, Card, InputNumber, Space, Table, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { useMemo } from 'react'

const { Text, Title } = Typography

interface AnalyticsViewResultsCardProps {
  result: AnalyticsViewResultDto | null
  runPage: number
  runPageSize: number
  isRunning: boolean
  onPrev: () => void
  onNext: () => void
  onPageSizeChanged: (size: number) => void
  onRefresh: () => void
}

const AnalyticsViewResultsCard = ({
  result,
  runPage,
  runPageSize,
  isRunning,
  onPrev,
  onNext,
  onPageSizeChanged,
  onRefresh,
}: AnalyticsViewResultsCardProps) => {
  const resultColumns = useMemo<ColumnsType<Record<string, unknown>>>(() => {
    if (!result?.columns) return []
    return result.columns.map((col) => ({
      title: col.displayName,
      dataIndex: col.displayName,
      key: col.displayName,
      ellipsis: true,
      render: (value: unknown) =>
        typeof value === 'object' ? JSON.stringify(value) : String(value ?? ''),
    }))
  }, [result])

  const resultRows = useMemo(() => {
    if (!result?.rows) return []
    return result.rows.map((row, idx) => ({ ...row, key: `${idx}` }))
  }, [result])

  return (
    <Card style={{ marginTop: 16 }} title="Preview Result">
      <Space style={{ marginBottom: 12 }}>
        <Text>Rows: {result?.totalRows ?? 0}</Text>
        <Button onClick={onPrev} disabled={runPage <= 1 || !result}>
          Prev
        </Button>
        <Button onClick={onNext} disabled={!result}>
          Next
        </Button>
        <InputNumber
          min={1}
          max={500}
          value={runPageSize}
          onChange={(value) => onPageSizeChanged(Number(value) || 25)}
        />
        <Button onClick={onRefresh} loading={isRunning}>
          Refresh
        </Button>
      </Space>

      {result ? (
        <Table
          size="small"
          columns={resultColumns}
          dataSource={resultRows}
          pagination={false}
          scroll={{ x: true }}
        />
      ) : (
        <Title level={5} style={{ margin: 0 }}>
          Run preview to view results.
        </Title>
      )}
    </Card>
  )
}

export default AnalyticsViewResultsCard

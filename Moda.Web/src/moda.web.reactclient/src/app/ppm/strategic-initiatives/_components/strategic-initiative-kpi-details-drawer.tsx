'use client'

import { MarkdownRenderer } from '@/src/components/common/markdown'
import { useMessage } from '@/src/components/contexts/messaging'
import { KpiTrend, KpiUnit } from '@/src/services/moda-api'
import {
  useGetStrategicInitiativeKpiCheckpointPlanQuery,
  useGetStrategicInitiativeKpiQuery,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import { getDrawerWidthPixels } from '@/src/utils'
import {
  ArrowDownOutlined,
  ArrowUpOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ExclamationCircleOutlined,
  MinusOutlined,
  QuestionOutlined,
} from '@ant-design/icons'
import {
  Button,
  Descriptions,
  Divider,
  Drawer,
  Flex,
  Table,
  Tag,
  Typography,
} from 'antd'
import dayjs from 'dayjs'
import { FC, useEffect, useState } from 'react'
import {
  AddStrategicInitiativeKpiMeasurementForm,
  EditStrategicInitiativeKpiForm,
  ManageStrategicInitiativeKpiCheckpointPlanForm,
} from '.'

const { Item } = Descriptions
const { Text } = Typography

export interface StrategicInitiativeKpiDetailsDrawerProps {
  strategicInitiativeId: string
  kpiId: string
  drawerOpen: boolean
  onDrawerClose: () => void
  canManageKpis: boolean
  onRefresh: () => void
}

const formatValue = (value: number | undefined, unit: string): string => {
  if (value === undefined || value === null) return '-'
  if (unit === KpiUnit.USD) return `$${value.toLocaleString()}`
  if (unit === KpiUnit.Percentage) return `${value}%`
  return value.toLocaleString()
}

const TrendTag: FC<{ trend: KpiTrend | undefined }> = ({ trend }) => {
  if (!trend || trend === KpiTrend.NoData)
    return (
      <Tag icon={<QuestionOutlined />} color="default">
        No Data
      </Tag>
    )
  if (trend === KpiTrend.Improving)
    return (
      <Tag icon={<ArrowUpOutlined />} color="success">
        Improving
      </Tag>
    )
  if (trend === KpiTrend.Worsening)
    return (
      <Tag icon={<ArrowDownOutlined />} color="error">
        Worsening
      </Tag>
    )
  return (
    <Tag icon={<MinusOutlined />} color="warning">
      Stable
    </Tag>
  )
}

// KpiHealth values (mirrors KpiHealth enum in backend)
const KPI_HEALTH_HEALTHY = 'Healthy'
const KPI_HEALTH_AT_RISK = 'AtRisk'
const KPI_HEALTH_UNHEALTHY = 'Unhealthy'

const KpiHealthTag: FC<{ health: string | undefined }> = ({ health }) => {
  if (!health)
    return (
      <Tag icon={<QuestionOutlined />} color="default">
        No Data
      </Tag>
    )
  if (health === KPI_HEALTH_HEALTHY)
    return (
      <Tag icon={<CheckCircleOutlined />} color="success">
        Healthy
      </Tag>
    )
  if (health === KPI_HEALTH_AT_RISK)
    return (
      <Tag icon={<ExclamationCircleOutlined />} color="warning">
        At Risk
      </Tag>
    )
  return (
    <Tag icon={<CloseCircleOutlined />} color="error">
      Unhealthy
    </Tag>
  )
}

const StrategicInitiativeKpiDetailsDrawer: FC<
  StrategicInitiativeKpiDetailsDrawerProps
> = (props) => {
  const {
    strategicInitiativeId,
    kpiId,
    drawerOpen,
    onDrawerClose,
    canManageKpis,
    onRefresh,
  } = props

  const [size, setSize] = useState(getDrawerWidthPixels())
  const [openEditKpiForm, setOpenEditKpiForm] = useState(false)
  const [openAddMeasurementForm, setOpenAddMeasurementForm] = useState(false)
  const [openManageCheckpointPlanForm, setOpenManageCheckpointPlanForm] =
    useState(false)

  const messageApi = useMessage()

  const {
    data: kpiData,
    isLoading: kpiIsLoading,
    error: kpiError,
    refetch: refetchKpi,
  } = useGetStrategicInitiativeKpiQuery(
    { strategicInitiativeId, kpiId },
    { skip: !drawerOpen },
  )

  const {
    data: checkpointData,
    isLoading: checkpointsIsLoading,
    error: checkpointsError,
    refetch: refetchCheckpoints,
  } = useGetStrategicInitiativeKpiCheckpointPlanQuery(
    { strategicInitiativeId, kpiId },
    { skip: !drawerOpen },
  )

  useEffect(() => {
    if (kpiError) {
      messageApi.error(
        kpiError.detail ??
          'An error occurred while loading KPI data. Please try again.',
      )
    }
  }, [kpiError, messageApi])

  useEffect(() => {
    if (checkpointsError) {
      messageApi.error(
        checkpointsError.detail ??
          'An error occurred while loading checkpoint data. Please try again.',
      )
    }
  }, [checkpointsError, messageApi])

  const unit = kpiData?.unit as unknown as string

  const onEditFormClosed = (wasSaved: boolean) => {
    setOpenEditKpiForm(false)
    if (wasSaved) {
      refetchKpi()
      onRefresh()
    }
  }

  const onAddMeasurementFormClosed = (wasSaved: boolean) => {
    setOpenAddMeasurementForm(false)
    if (wasSaved) {
      refetchCheckpoints()
      onRefresh()
    }
  }

  const onManageCheckpointPlanFormClosed = (wasSaved: boolean) => {
    setOpenManageCheckpointPlanForm(false)
    if (wasSaved) {
      refetchCheckpoints()
      onRefresh()
    }
  }

  const checkpointColumns = [
    {
      title: 'Label',
      dataIndex: 'dateLabel',
      key: 'dateLabel',
      width: 90,
    },
    {
      title: 'Date',
      dataIndex: 'checkpointDate',
      key: 'checkpointDate',
      width: 130,
      render: (date: Date) =>
        date ? dayjs(date).format('YYYY-MM-DD h:mm A') : '-',
    },
    {
      title: 'Target',
      dataIndex: 'targetValue',
      key: 'targetValue',
      width: 90,
      align: 'right' as const,
      render: (value: number) => formatValue(value, unit),
    },
    {
      title: 'Actual',
      key: 'actual',
      width: 90,
      align: 'right' as const,
      render: (_, record) =>
        record.measurement
          ? formatValue(record.measurement.actualValue, unit)
          : '-',
    },
    {
      title: 'Health',
      dataIndex: 'health',
      key: 'health',
      width: 110,
      render: (health: string | undefined) => <KpiHealthTag health={health} />,
    },
    {
      title: 'Trend',
      dataIndex: 'trend',
      key: 'trend',
      width: 110,
      render: (trend: KpiTrend | undefined) => <TrendTag trend={trend} />,
    },
  ]

  const extraButtons = canManageKpis ? (
    <Flex gap="small">
      <Button size="small" onClick={() => setOpenManageCheckpointPlanForm(true)}>
        Checkpoint Plan
      </Button>
      <Button size="small" onClick={() => setOpenAddMeasurementForm(true)}>
        Add Measurement
      </Button>
      <Button size="small" onClick={() => setOpenEditKpiForm(true)}>
        Edit
      </Button>
    </Flex>
  ) : undefined

  return (
    <>
      <Drawer
        title={kpiData ? `KPI ${kpiData.key} - ${kpiData.name}` : 'KPI Details'}
        placement="right"
        onClose={onDrawerClose}
        open={drawerOpen}
        loading={kpiIsLoading}
        size={size}
        resizable={{ onResize: (newSize) => setSize(newSize) }}
        destroyOnHidden={true}
        styles={{ body: { scrollbarWidth: 'auto' } as React.CSSProperties }}
        className="custom-drawer"
        extra={extraButtons}
      >
        <Flex vertical gap="middle">
          <Descriptions column={1} size="small">
            <Item label="Target Value">
              {formatValue(kpiData?.targetValue, unit)}
            </Item>
            <Item label="Actual Value">
              {kpiData?.actualValue !== undefined
                ? formatValue(kpiData.actualValue, unit)
                : '-'}
            </Item>
            <Item label="Unit">{unit}</Item>
            <Item label="Target Direction">
              {kpiData?.targetDirection as unknown as string}
            </Item>
          </Descriptions>
          {kpiData?.description && (
            <Descriptions column={1} layout="vertical" size="small">
              <Item label="Description">
                <MarkdownRenderer markdown={kpiData.description} />
              </Item>
            </Descriptions>
          )}
          <Divider orientation="left" orientationMargin={0}>
            <Text type="secondary" style={{ fontSize: 12 }}>
              Checkpoints
            </Text>
          </Divider>
          <Table
            dataSource={checkpointData ?? []}
            columns={checkpointColumns}
            rowKey="id"
            size="small"
            loading={checkpointsIsLoading}
            pagination={false}
            locale={{ emptyText: 'No checkpoints defined' }}
            scroll={{ x: 'max-content' }}
          />
        </Flex>
      </Drawer>
      {openEditKpiForm && (
        <EditStrategicInitiativeKpiForm
          strategicInitiativeId={strategicInitiativeId}
          kpiId={kpiId}
          showForm={openEditKpiForm}
          onFormComplete={() => onEditFormClosed(true)}
          onFormCancel={() => onEditFormClosed(false)}
        />
      )}
      {openAddMeasurementForm && (
        <AddStrategicInitiativeKpiMeasurementForm
          strategicInitiativeId={strategicInitiativeId}
          kpiId={kpiId}
          showForm={openAddMeasurementForm}
          onFormComplete={() => onAddMeasurementFormClosed(true)}
          onFormCancel={() => onAddMeasurementFormClosed(false)}
        />
      )}
      {openManageCheckpointPlanForm && (
        <ManageStrategicInitiativeKpiCheckpointPlanForm
          strategicInitiativeId={strategicInitiativeId}
          kpiId={kpiId}
          showForm={openManageCheckpointPlanForm}
          onFormComplete={() => onManageCheckpointPlanFormClosed(true)}
          onFormCancel={() => onManageCheckpointPlanFormClosed(false)}
        />
      )}
    </>
  )
}

export default StrategicInitiativeKpiDetailsDrawer

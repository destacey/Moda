'use client'

import { MarkdownRenderer } from '@/src/components/common/markdown'
import { useMessage } from '@/src/components/contexts/messaging'
import { KpiHealth, KpiTrend, KpiUnit } from '@/src/services/moda-api'
import {
  useGetStrategicInitiativeKpiCheckpointPlanQuery,
  useGetStrategicInitiativeKpiMeasurementsQuery,
  useGetStrategicInitiativeKpiQuery,
  useRemoveStrategicInitiativeKpiMeasurementMutation,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import { getDrawerWidthPixels } from '@/src/utils'
import {
  ArrowDownOutlined,
  ArrowUpOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  DeleteOutlined,
  ExclamationCircleOutlined,
  MinusOutlined,
  MoreOutlined,
} from '@ant-design/icons'
import {
  Button,
  Descriptions,
  Divider,
  Drawer,
  Dropdown,
  Flex,
  Popconfirm,
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
  if (!trend || trend === KpiTrend.NoData) return undefined
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

const KpiHealthTag: FC<{ health: KpiHealth | undefined }> = ({ health }) => {
  if (!health) return undefined
  if (health === KpiHealth.Healthy)
    return (
      <Tag icon={<CheckCircleOutlined />} color="success">
        Healthy
      </Tag>
    )
  if (health === KpiHealth.AtRisk)
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

  const {
    data: measurementData,
    isLoading: measurementsIsLoading,
    error: measurementsError,
    refetch: refetchMeasurements,
  } = useGetStrategicInitiativeKpiMeasurementsQuery(
    { strategicInitiativeId, kpiId },
    { skip: !drawerOpen },
  )

  const [removeMeasurement] =
    useRemoveStrategicInitiativeKpiMeasurementMutation()

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

  useEffect(() => {
    if (measurementsError) {
      messageApi.error(
        measurementsError.detail ??
          'An error occurred while loading measurement data. Please try again.',
      )
    }
  }, [measurementsError, messageApi])

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
      refetchMeasurements()
      onRefresh()
    }
  }

  const onDeleteMeasurement = async (measurementId: string) => {
    try {
      await removeMeasurement({
        strategicInitiativeId,
        kpiId,
        measurementId,
      }).unwrap()
      messageApi.success('Measurement deleted successfully.')
      onRefresh()
    } catch {
      messageApi.error(
        'An error occurred while deleting the measurement. Please try again.',
      )
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
      render: (health: KpiHealth | undefined) => <KpiHealthTag health={health} />,
    },
    {
      title: 'Trend',
      dataIndex: 'trend',
      key: 'trend',
      width: 110,
      render: (trend: KpiTrend | undefined) => <TrendTag trend={trend} />,
    },
  ]

  const measurementColumns = [
    {
      title: 'Date',
      dataIndex: 'measurementDate',
      key: 'measurementDate',
      width: 160,
      render: (date: Date) =>
        date ? dayjs(date).format('YYYY-MM-DD h:mm A') : '-',
    },
    {
      title: 'Actual Value',
      dataIndex: 'actualValue',
      key: 'actualValue',
      width: 110,
      align: 'right' as const,
      render: (value: number) => formatValue(value, unit),
    },
    {
      title: 'Measured By',
      key: 'measuredBy',
      width: 140,
      render: (_, record) => record.measuredBy?.name ?? '-',
    },
    {
      title: 'Note',
      dataIndex: 'note',
      key: 'note',
      render: (note: string | undefined) => note ?? '-',
    },
    ...(canManageKpis
      ? [
          {
            key: 'actions',
            width: 50,
            align: 'center' as const,
            render: (_, record) => (
              <Popconfirm
                title="Delete measurement?"
                description="This action cannot be undone."
                onConfirm={() => onDeleteMeasurement(record.id)}
                okText="Delete"
                okButtonProps={{ danger: true }}
              >
                <Button
                  size="small"
                  type="text"
                  danger
                  icon={<DeleteOutlined />}
                />
              </Popconfirm>
            ),
          },
        ]
      : []),
  ]

  const extraMenu = canManageKpis ? (
    <Dropdown
      menu={{
        items: [
          {
            key: 'edit',
            label: 'Edit',
            onClick: () => setOpenEditKpiForm(true),
          },
          {
            key: 'checkpoint-plan',
            label: 'Checkpoint Plan',
            onClick: () => setOpenManageCheckpointPlanForm(true),
          },
          {
            key: 'add-measurement',
            label: 'Add Measurement',
            onClick: () => setOpenAddMeasurementForm(true),
          },
        ],
      }}
      trigger={['click']}
    >
      <Button type="text" size="small" icon={<MoreOutlined />} />
    </Dropdown>
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
        extra={extraMenu}
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
          <Divider titlePlacement="start">
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
          <Divider titlePlacement="start">
            <Text type="secondary" style={{ fontSize: 12 }}>
              Measurements
            </Text>
          </Divider>
          <Table
            dataSource={measurementData ?? []}
            columns={measurementColumns}
            rowKey="id"
            size="small"
            loading={measurementsIsLoading}
            pagination={false}
            locale={{ emptyText: 'No measurements recorded' }}
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

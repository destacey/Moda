'use client'

import { WaydGrid } from '@/src/components/common'
import { useMessage } from '@/src/components/contexts/messaging'
import { WaydStatisticNumber } from '@/src/components/common/metrics'
import { StrategicInitiativeKpiListDto } from '@/src/services/wayd-api'
import { useReorderStrategicInitiativeKpisMutation } from '@/src/store/features/ppm/strategic-initiatives-api'
import { isApiError } from '@/src/utils'
import { HolderOutlined, MoreOutlined } from '@ant-design/icons'
import styles from './strategic-initiative-kpis-grid.module.css'
import {
  ColDef,
  GetRowIdParams,
  ICellRendererParams,
  RowDragEndEvent,
} from 'ag-grid-community'
import { AgGridReact } from 'ag-grid-react'
import { Button, Dropdown, Flex, MenuProps } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import { FC, useState, useMemo, useRef, useCallback } from 'react'
import {
  AddStrategicInitiativeKpiMeasurementForm,
  DeleteStrategicInitiativeKpiForm,
  EditStrategicInitiativeKpiForm,
  ManageStrategicInitiativeKpiCheckpointPlanForm,
  StrategicInitiativeKpiDetailsDrawer,
} from '.'

export interface StrategicInitiativeKpisGridProps {
  strategicInitiativeId: string
  kpis: StrategicInitiativeKpiListDto[]
  canManageKpis: boolean
  isLoading: boolean
  refetch: () => void
  gridHeight?: number | undefined
  isReadOnly?: boolean
  viewSelector?: React.ReactNode
}

const StatisticNumberCellRenderer = (params: ICellRendererParams<StrategicInitiativeKpiListDto>) => {
  return <WaydStatisticNumber value={params.value} />
}

interface RowMenuProps extends MenuProps {
  kpiId: string
  strategicInitiativeId: string
  canManageKpis: boolean
  onEditKpiMenuClicked: (id: string) => void
  onDeleteKpiMenuClicked: (id: string) => void
  onAddMeasurementMenuClicked: (id: string) => void
  onManageCheckpointPlanMenuClicked: (id: string) => void
}

const getRowMenuItems = (props: RowMenuProps): ItemType[] => {
  if (
    !props.kpiId ||
    !props.canManageKpis ||
    !props.onEditKpiMenuClicked ||
    !props.onDeleteKpiMenuClicked ||
    !props.onAddMeasurementMenuClicked ||
    !props.onManageCheckpointPlanMenuClicked
  ) {
    return []
  }

  return [
    {
      key: 'edit-kpi',
      label: 'Edit KPI',
      onClick: () => props.onEditKpiMenuClicked(props.kpiId),
    },
    {
      key: 'delete-kpi',
      label: 'Delete KPI',
      danger: true,
      onClick: () => props.onDeleteKpiMenuClicked(props.kpiId),
    },
    { key: 'divider', type: 'divider' },
    {
      key: 'manage-checkpoint-plan',
      label: 'Manage Checkpoint Plan',
      onClick: () => props.onManageCheckpointPlanMenuClicked(props.kpiId),
    },
    {
      key: 'add-measurement',
      label: 'Add Measurement',
      onClick: () => props.onAddMeasurementMenuClicked(props.kpiId),
    },
  ]
}

interface DragAndMenuCellRendererProps
  extends ICellRendererParams<StrategicInitiativeKpiListDto> {
  menuItems: ItemType[]
  canDrag: boolean
}

const DragAndMenuCellRenderer = (props: DragAndMenuCellRendererProps) => {
  const dragHandleRef = useCallback(
    (node: HTMLSpanElement | null) => {
      if (node && props.canDrag) {
        props.registerRowDragger(node)
      }
    },
    [props],
  )

  const showMenu = props.menuItems.length > 0

  if (!props.canDrag && !showMenu) return null

  return (
    <Flex align="center" gap={2} style={{ height: '100%' }}>
      {props.canDrag && (
        <span
          ref={dragHandleRef}
          style={{ cursor: 'grab', display: 'inline-flex', padding: '0 4px' }}
          aria-label="Drag to reorder"
        >
          <HolderOutlined />
        </span>
      )}
      {showMenu && (
        <Dropdown menu={{ items: props.menuItems }} trigger={['click']}>
          <Button type="text" size="small" icon={<MoreOutlined />} />
        </Dropdown>
      )}
    </Flex>
  )
}

const StrategicInitiativeKpisGrid: FC<StrategicInitiativeKpisGridProps> = (
  props,
) => {
  const {
    strategicInitiativeId,
    kpis,
    canManageKpis,
    isLoading,
    refetch,
    gridHeight,
    isReadOnly,
    viewSelector,
  } = props

  const [selectedKpiId, setSelectedKpiId] = useState<string | null>(null)
  const [openKpiDetailsDrawer, setOpenKpiDetailsDrawer] =
    useState<boolean>(false)
  const [openEditKpiForm, setOpenEditKpiForm] = useState<boolean>(false)
  const [openDeleteKpiForm, setOpenDeleteKpiForm] = useState<boolean>(false)
  const [openAddMeasurementForm, setOpenAddMeasurementForm] =
    useState<boolean>(false)
  const [openManageCheckpointPlanForm, setOpenManageCheckpointPlanForm] =
    useState<boolean>(false)

  const gridRef = useRef<AgGridReact<StrategicInitiativeKpiListDto>>(null)
  const messageApi = useMessage()
  const [reorderKpis] = useReorderStrategicInitiativeKpisMutation()

  const canReorder = canManageKpis && !isReadOnly

  const onEditKpiFormClosed = (wasSaved: boolean) => {
    setOpenEditKpiForm(false)
    setSelectedKpiId(null)
    if (wasSaved) {
      refresh()
    }
  }

  const onDeleteKpiFormClosed = (wasSaved: boolean) => {
    setOpenDeleteKpiForm(false)
    setSelectedKpiId(null)
    if (wasSaved) {
      refresh()
    }
  }

  const onAddMeasurementFormClosed = (wasSaved: boolean) => {
    setOpenAddMeasurementForm(false)
    setSelectedKpiId(null)
    if (wasSaved) {
      refresh()
    }
  }

  const onManageCheckpointPlanFormClosed = (wasSaved: boolean) => {
    setOpenManageCheckpointPlanForm(false)
    setSelectedKpiId(null)
    if (wasSaved) {
      refresh()
    }
  }

  const columnDefs = useMemo<ColDef<StrategicInitiativeKpiListDto>[]>(
    () => {
      const onViewDetailsMenuClicked = (id: string) => {
        setSelectedKpiId(id)
        setOpenKpiDetailsDrawer(true)
      }

      const onEditKpiMenuClicked = (id: string) => {
        setSelectedKpiId(id)
        setOpenEditKpiForm(true)
      }

      const onDeleteKpiMenuClicked = (id: string) => {
        setSelectedKpiId(id)
        setOpenDeleteKpiForm(true)
      }

      const onAddMeasurementMenuClicked = (id: string) => {
        setSelectedKpiId(id)
        setOpenAddMeasurementForm(true)
      }

      const onManageCheckpointPlanMenuClicked = (id: string) => {
        setSelectedKpiId(id)
        setOpenManageCheckpointPlanForm(true)
      }

      return [
      {
        width: 70,
        filter: false,
        sortable: false,
        resizable: false,
        suppressHeaderMenuButton: true,
        hide: !canManageKpis || isReadOnly,
        cellClass: styles.rowActionsCell,
        cellRenderer: (params: ICellRendererParams<StrategicInitiativeKpiListDto>) => {
          const menuItems = getRowMenuItems({
            kpiId: params.data!.id,
            strategicInitiativeId: strategicInitiativeId,
            canManageKpis,
            onEditKpiMenuClicked,
            onDeleteKpiMenuClicked,
            onAddMeasurementMenuClicked,
            onManageCheckpointPlanMenuClicked,
          })

          return (
            <DragAndMenuCellRenderer
              {...params}
              menuItems={menuItems}
              canDrag={canReorder}
            />
          )
        },
      },
      { field: 'key', width: 90 },
      {
        field: 'name',
        width: 300,
        cellRenderer: (params: ICellRendererParams<StrategicInitiativeKpiListDto>) => (
          <Button
            type="link"
            size="small"
            style={{ padding: 0 }}
            onClick={() => onViewDetailsMenuClicked(params.data!.id)}
          >
            {params.value}
          </Button>
        ),
      },
      {
        field: 'targetValue',
        headerName: 'Target Value',
        width: 125,
        type: 'numericColumn',
        cellRenderer: StatisticNumberCellRenderer,
      },
      {
        field: 'actualValue',
        headerName: 'Actual Value',
        width: 125,
        type: 'numericColumn',
        cellRenderer: StatisticNumberCellRenderer,
      },
      {
        headerName: 'Format',
        width: 125,
        valueGetter: (params) =>
          [params.data?.prefix, params.data?.suffix]
            .filter(Boolean)
            .join(' / ') || '-',
      },
      {
        field: 'targetDirection',
        headerName: 'Target Direction',
        width: 125,
      },
    ]},
    [
      canManageKpis,
      canReorder,
      isReadOnly,
      strategicInitiativeId,
    ],
  )

  const getRowId = (params: GetRowIdParams) => String(params.data.id)

  const refresh = async () => {
    refetch()
  }

  const hasActiveSort = () => {
    const columnState = gridRef.current?.api?.getColumnState() ?? []
    return columnState.some((c) => c.sort)
  }

  const onRowDragEnd = async (event: RowDragEndEvent<StrategicInitiativeKpiListDto>) => {
    if (!canReorder) return

    if (hasActiveSort()) {
      messageApi.warning(
        'Clear column sorting to reorder KPIs.',
      )
      refetch()
      return
    }

    const orderedIds: string[] = []
    event.api.forEachNodeAfterFilterAndSort((node) => {
      if (node.data?.id) orderedIds.push(node.data.id)
    })

    try {
      await reorderKpis({
        strategicInitiativeId,
        request: { orderedKpiIds: orderedIds },
      }).unwrap()
    } catch (error) {
      messageApi.error(
        (isApiError(error) ? error.detail : undefined) ??
          'Failed to reorder KPIs. Please try again.',
      )
      refetch()
    }
  }


  return (
    <>
      <WaydGrid
        ref={gridRef}
        columnDefs={columnDefs}
        rowData={kpis}
        loadData={refresh}
        loading={isLoading}
        height={gridHeight}
        emptyMessage="No KPIs found."
        getRowId={getRowId}
        toolbarActions={viewSelector}
        rowDragManaged={true}
        onRowDragEnd={onRowDragEnd}
      />
      {selectedKpiId && (
        <StrategicInitiativeKpiDetailsDrawer
          strategicInitiativeId={strategicInitiativeId}
          kpiId={selectedKpiId}
          drawerOpen={openKpiDetailsDrawer}
          onDrawerClose={() => {
            setOpenKpiDetailsDrawer(false)
            setSelectedKpiId(null)
          }}
          canManageKpis={canManageKpis && !isReadOnly}
          onRefresh={refresh}
        />
      )}
      {openEditKpiForm && selectedKpiId && (
        <EditStrategicInitiativeKpiForm
          strategicInitiativeId={strategicInitiativeId}
          kpiId={selectedKpiId}
          onFormComplete={() => onEditKpiFormClosed(true)}
          onFormCancel={() => onEditKpiFormClosed(false)}
        />
      )}
      {openDeleteKpiForm && selectedKpiId && (
        <DeleteStrategicInitiativeKpiForm
          strategicInitiativeId={strategicInitiativeId}
          kpi={kpis.find((kpi) => kpi.id === selectedKpiId)!}
          onFormComplete={() => onDeleteKpiFormClosed(true)}
          onFormCancel={() => onDeleteKpiFormClosed(false)}
        />
      )}
      {openAddMeasurementForm && selectedKpiId && (
        <AddStrategicInitiativeKpiMeasurementForm
          strategicInitiativeId={strategicInitiativeId}
          kpiId={selectedKpiId}
          onFormComplete={() => onAddMeasurementFormClosed(true)}
          onFormCancel={() => onAddMeasurementFormClosed(false)}
        />
      )}
      {openManageCheckpointPlanForm && selectedKpiId && (
        <ManageStrategicInitiativeKpiCheckpointPlanForm
          strategicInitiativeId={strategicInitiativeId}
          kpiId={selectedKpiId}
          onFormComplete={() => onManageCheckpointPlanFormClosed(true)}
          onFormCancel={() => onManageCheckpointPlanFormClosed(false)}
        />
      )}
    </>
  )
}

export default StrategicInitiativeKpisGrid

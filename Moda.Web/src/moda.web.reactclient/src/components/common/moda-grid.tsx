'use client'

import { AgGridReact, AgGridReactProps } from 'ag-grid-react'
import { useCallback, useRef, useState } from 'react'
import {
  Button,
  Col,
  Divider,
  Input,
  Row,
  Space,
  Spin,
  Tooltip,
  Typography,
} from 'antd'
import {
  DownloadOutlined,
  ReloadOutlined,
  SearchOutlined,
} from '@ant-design/icons'
import { ItemType } from 'antd/es/menu/interface'
import useTheme from '../contexts/theme'
import ModaEmpty from './moda-empty'
import { ControlItemsMenu } from './control-items-menu'

const { Text } = Typography

interface ModaGridProps extends AgGridReactProps {
  height?: number
  width?: number
  includeGlobalSearch?: boolean
  includeExportButton?: boolean
  actions?: React.ReactNode | null
  gridControlMenuItems?: ItemType[]
  toolbarActions?: React.ReactNode | null
  loadData?: () => Promise<void> | void
}

const modaDefaultColDef = {
  sortable: true,
  filter: true,
  resizable: true,
  floatingFilter: true,
}

const ModaGrid = ({
  height,
  width,
  includeGlobalSearch,
  includeExportButton,
  actions,
  gridControlMenuItems,
  toolbarActions,
  defaultColDef,
  rowData,
  loadData,
  ...props
}: ModaGridProps) => {
  const { agGridTheme } = useTheme()
  const [displayedRowCount, setDisplayedRowCount] = useState(0)
  const showGlobalSearch = includeGlobalSearch ?? true
  const showExportButton = includeExportButton ?? true
  const showGridControls = gridControlMenuItems?.length > 0

  const gridRef = useRef<AgGridReact>(null)

  const rowCount = rowData?.length ?? 0

  const onGridReady = () => {
    if (!props.loading && rowCount === 0 && loadData) {
      loadData()
    }
  }

  const onModelUpdated = useCallback(() => {
    setDisplayedRowCount(gridRef.current?.api.getDisplayedRowCount() ?? 0)
  }, [])

  const onGlobalSearchChange = useCallback((e) => {
    gridRef.current?.api.setGridOption('quickFilterText', e.target.value)
  }, [])

  const onBtnExport = useCallback(() => {
    gridRef.current?.api.exportDataAsCsv()
  }, [])

  return (
    <div style={{ width: width }}>
      <Space direction="vertical" style={{ width: '100%' }}>
        <Row>
          {actions && (
            <Col xs={24} sm={24} md={10}>
              {actions}
            </Col>
          )}
          <Col xs={24} sm={24} md={actions ? 14 : 24}>
            <Space style={{ display: 'flex', justifyContent: 'flex-end' }} wrap>
              <Space style={{ display: 'flex', justifyContent: 'flex-end' }}>
                <Text>
                  {displayedRowCount} of {rowCount}
                </Text>
                {showGlobalSearch && (
                  <Input
                    placeholder="Search"
                    allowClear={true}
                    onChange={onGlobalSearchChange}
                    suffix={<SearchOutlined />}
                  />
                )}
              </Space>
              <Space style={{ display: 'flex', justifyContent: 'flex-end' }}>
                {showGridControls && (
                  <ControlItemsMenu items={gridControlMenuItems} />
                )}
                {loadData && (
                  <Tooltip title="Refresh Grid">
                    <Button
                      type="text"
                      shape="circle"
                      icon={<ReloadOutlined />}
                      onClick={loadData}
                    />
                  </Tooltip>
                )}
                {(showExportButton || toolbarActions) && (
                  <Divider type="vertical" style={{ height: '30px' }} />
                )}
                {showExportButton && (
                  <Tooltip title="Export to CSV">
                    <Button
                      type="text"
                      shape="circle"
                      icon={<DownloadOutlined />}
                      onClick={onBtnExport}
                    />
                  </Tooltip>
                )}
                {toolbarActions && toolbarActions}
              </Space>
            </Space>
          </Col>
        </Row>

        <div style={{ height: height ?? 700 }}>
          <AgGridReact
            ref={gridRef}
            theme={agGridTheme}
            defaultColDef={defaultColDef ?? modaDefaultColDef}
            onGridReady={onGridReady}
            animateRows={false} // animation has to be off for cell text selection to work correctly
            rowData={rowData}
            onModelUpdated={onModelUpdated}
            multiSortKey="ctrl"
            loading={props.loading}
            loadingOverlayComponent={() => <Spin size="large" />}
            noRowsOverlayComponent={() => (
              <ModaEmpty message="No records found." />
            )}
            enableCellTextSelection={true}
            ensureDomOrder={true}
            {...props}
          ></AgGridReact>
        </div>
      </Space>
    </div>
  )
}

export default ModaGrid

'use client'

import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-balham.css'
import { AgGridReact, AgGridReactProps } from 'ag-grid-react'
import { useCallback, useEffect, useRef, useState } from 'react'
import {
  Button,
  Col,
  Dropdown,
  Input,
  Row,
  Space,
  Tooltip,
  Typography,
} from 'antd'
import {
  ControlOutlined,
  ExportOutlined,
  ReloadOutlined,
} from '@ant-design/icons'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import useTheme from '../contexts/theme'

interface ModaGridProps extends AgGridReactProps {
  height?: number
  width?: number
  includeGlobalSearch?: boolean
  includeExportButton?: boolean
  actions?: React.ReactNode | null
  gridControlMenuItems?: ItemType[]
  toolbarActions?: React.ReactNode | null
  loadData?: () => Promise<void> | void
  isDataLoading?: boolean
}

const modaDefaultColDef = {
  sortable: true,
  filter: true,
  resizable: true,
  floatingFilter: true,
}

// TODO: create a custom implementation for react-query
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
  isDataLoading,
  ...props
}: ModaGridProps) => {
  const { agGridTheme } = useTheme()
  const [displayedRowCount, setDisplayedRowCount] = useState(0)
  const showGlobalSearch = includeGlobalSearch ?? true
  const showExportButton = includeExportButton ?? true
  const showGridControls = gridControlMenuItems?.length > 0
  const toolbarMdSize = actions ? 12 : 24

  const gridRef = useRef<AgGridReact>(null)

  const rowCount = rowData?.length ?? 0

  const onModelUpdated = useCallback(() => {
    setDisplayedRowCount(gridRef.current?.api.getDisplayedRowCount() ?? 0)
  }, [])

  const onGlobalSearchChange = useCallback((e) => {
    gridRef.current?.api.setQuickFilter(e.target.value)
  }, [])

  const onBtnExport = useCallback(() => {
    gridRef.current?.api.exportDataAsCsv()
  }, [])

  useEffect(() => {
    if (!gridRef.current?.api) return

    if (isDataLoading) {
      gridRef.current?.api.showLoadingOverlay()
    } else {
      gridRef.current?.api.hideOverlay()
    }
  }, [isDataLoading])

  return (
    <div style={{ width: width }}>
      <Space direction="vertical" style={{ width: '100%' }}>
        <Row>
          {actions && (
            <Col xs={24} sm={24} md={toolbarMdSize}>
              {actions}
            </Col>
          )}
          <Col xs={24} sm={24} md={toolbarMdSize}>
            <Space style={{ display: 'flex', justifyContent: 'flex-end' }}>
              <Typography.Text>
                {displayedRowCount} of {rowCount}
              </Typography.Text>
              {showGlobalSearch && (
                <Input
                  placeholder="Search"
                  allowClear={true}
                  onChange={onGlobalSearchChange}
                />
              )}
              {showGridControls && (
                // TODO: this tooltip is triggering "findDOMNode is deprecated in StrictMode" warnings
                // <Tooltip title="Grid Controls">
                <Dropdown
                  menu={{ items: gridControlMenuItems }}
                  trigger={['click']}
                >
                  <Button
                    type="text"
                    shape="circle"
                    icon={<ControlOutlined />}
                  />
                </Dropdown>
                // </Tooltip>
              )}
              {loadData && (
                <Tooltip title="Refresh Grid">
                  <Button
                    type="text"
                    shape="circle"
                    icon={<ReloadOutlined />}
                    onClick={() => loadData?.()}
                  />
                </Tooltip>
              )}
              {showExportButton && (
                <Tooltip title="Export to CSV">
                  <Button
                    type="text"
                    shape="circle"
                    icon={<ExportOutlined />}
                    onClick={onBtnExport}
                  />
                </Tooltip>
              )}
              {toolbarActions && toolbarActions}
            </Space>
          </Col>
        </Row>

        <div className={agGridTheme} style={{ height: height ?? 700 }}>
          <AgGridReact
            ref={gridRef}
            defaultColDef={defaultColDef ?? modaDefaultColDef}
            onGridReady={() => loadData?.()}
            animateRows={true}
            rowData={rowData}
            onModelUpdated={onModelUpdated}
            multiSortKey="ctrl"
            {...props}
          ></AgGridReact>
        </div>
      </Space>
    </div>
  )
}

export default ModaGrid

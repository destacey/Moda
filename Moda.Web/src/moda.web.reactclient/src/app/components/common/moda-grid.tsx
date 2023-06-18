import { AgGridReact, AgGridReactProps } from "ag-grid-react";
import { useCallback, useContext, useRef, useState } from "react";
import { ThemeContext } from "../contexts/theme-context";
import { Button, Dropdown, Input, Space, Tooltip, Typography } from "antd";
import { ControlOutlined, ExportOutlined } from "@ant-design/icons";
import { ItemType } from "antd/es/menu/hooks/useItems";

import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-balham.css';

interface ModaGridProps extends AgGridReactProps {
    height?: number
    width?: number
    includeGlobalSearch?: boolean
    includeExportButton?: boolean,
    gridControlMenuItems?: ItemType[]
}

const modaDefaultColDef = {
    sortable: true,
    filter: true,
    resizable: true,
    floatingFilter: true,
}

const ModaGrid = ({ height, width, includeGlobalSearch, includeExportButton, gridControlMenuItems, defaultColDef, rowData, ...props }: ModaGridProps) => {
    const themeContext = useContext(ThemeContext)
    const [displayedRowCount, setDisplayedRowCount] = useState(0)
    const showGlobalSearch = includeGlobalSearch ?? true
    const showExportButton = includeExportButton ?? true
    const showGridControls = gridControlMenuItems?.length > 0

    const gridRef = useRef<AgGridReact>(null)

    const rowCount = rowData?.length ?? 0

    // TODO: add refresh button

    const onGridReady = useCallback(() => {
        gridRef.current?.api.sizeColumnsToFit()
    }, [])

    const onModelUpdated = useCallback(() => {
        setDisplayedRowCount(gridRef.current?.api.getDisplayedRowCount() ?? 0)
    }, [])

    const onGlobalSearchChange = useCallback((e) => {
        gridRef.current?.api.setQuickFilter(e.target.value)
    }, [])

    const onBtnExport = useCallback(() => {
        gridRef.current.api.exportDataAsCsv()
    }, [])

    return (
        <div style={{ width: width }}>
            <Space direction="vertical" style={{ width: '100%' }}>
                <Space style={{ display: 'flex', justifyContent: 'flex-end' }}>
                    <Typography.Text>{displayedRowCount} of {rowCount}</Typography.Text>
                    {showGlobalSearch ? (
                        <Input placeholder="Search"
                            allowClear={true}
                            onChange={onGlobalSearchChange} />
                    ) : null}
                    {showGridControls ? (
                        <Tooltip title="Grid Controls">
                            <Dropdown menu={{ items: gridControlMenuItems }} trigger={['click']}>
                                <Button type='text' shape="circle" icon={<ControlOutlined />} />
                            </Dropdown>
                        </Tooltip>
                    ) : null}
                    {showExportButton ? (
                        <Tooltip title="Export to CSV">
                            <Button type='text' shape="circle" icon={<ExportOutlined />} onClick={onBtnExport} />
                        </Tooltip>
                    ) : null}
                </Space>

                <div className={themeContext?.agGridTheme} style={{ height: height ?? 700 }}>
                    <AgGridReact
                        ref={gridRef}
                        defaultColDef={defaultColDef ?? modaDefaultColDef}
                        onGridReady={onGridReady}
                        animateRows={true}
                        rowData={rowData}
                        onModelUpdated={onModelUpdated}
                        {...props}>
                    </AgGridReact>
                </div>
            </Space>
        </div>
    )
}

export default ModaGrid
import { AgGridReact, AgGridReactProps } from "ag-grid-react";
import { useCallback, useContext, useRef } from "react";
import { ThemeContext } from "../contexts/theme-context";
import { Button, Dropdown, Input, MenuProps, Space, Switch, Tooltip } from "antd";
import { ControlOutlined, DropboxCircleFilled, ExportOutlined } from "@ant-design/icons";
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

const ModaGrid = ({ height, width, includeGlobalSearch, includeExportButton, gridControlMenuItems, defaultColDef, ...props }: ModaGridProps) => {
    const themeContext = useContext(ThemeContext)
    const showGlobalSearch = includeGlobalSearch ?? true
    const showExportButton = includeExportButton ?? true
    const showGridControls = gridControlMenuItems.length > 0
    const showToolbar = showGlobalSearch || showGridControls || showExportButton

    const gridRef = useRef<AgGridReact>(null)

    // TODO: add refresh button
    // TODO: add count label based on the current filter (e.g. 17 of 34)

    const onGridReady = useCallback(() => {
        gridRef.current?.api.sizeColumnsToFit()
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
                {showToolbar ? (
                    <Space style={{ display: 'flex', justifyContent: 'flex-end' }}>
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
                ) : null}

                <div className={themeContext?.agGridTheme} style={{ height: height ?? 700 }}>
                    <AgGridReact
                        ref={gridRef}
                        defaultColDef={defaultColDef ?? modaDefaultColDef}
                        onGridReady={onGridReady}
                        animateRows={true}
                        {...props}>
                    </AgGridReact>
                </div>
            </Space>
        </div>
    )
}

export default ModaGrid
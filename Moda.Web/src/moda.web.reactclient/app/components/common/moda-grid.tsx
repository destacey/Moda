import { AgGridReact, AgGridReactProps } from "ag-grid-react";
import { useCallback, useContext, useRef } from "react";
import { ThemeContext } from "../contexts/theme-context";

import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-balham.css';
import { Button, Input, Space, Tooltip } from "antd";
import { ExportOutlined } from "@ant-design/icons";

interface ModaGridProps extends AgGridReactProps {
    height?: number
    width?: number
    includeGlobalSearch?: boolean
    includeExportButton?: boolean
}

const modaDefaultColDef = {
    sortable: true,
    filter: true,
    resizable: true,
}

const ModaGrid = ({ height, width, includeGlobalSearch, includeExportButton, defaultColDef, ...props }: ModaGridProps) => {
    const themeContext = useContext(ThemeContext)
    const showGlobalSearch = includeGlobalSearch ?? true
    const showExportButton = includeExportButton ?? true
    const showToolbar = showGlobalSearch || showExportButton

    const gridRef = useRef<AgGridReact>(null)

    const onBtnExport = useCallback(() => {
        gridRef.current.api.exportDataAsCsv();
    }, []);

    return (
        <div style={{ width: width }}>
            <Space direction="vertical" style={{ width: '100%' }}>
                {showToolbar ? (
                    <Space style={{ display: 'flex', justifyContent: 'flex-end' }}>
                        {showGlobalSearch ? (
                            <Input placeholder="Search"
                                allowClear={true}
                                onChange={(e) => gridRef.current?.api.setQuickFilter(e.target.value)} />
                        ) : null}
                        {showExportButton ? (
                            <Tooltip title="Export to CSV">
                                <Button shape="circle" icon={<ExportOutlined />} onClick={onBtnExport} />
                            </Tooltip>
                        ) : null}
                    </Space>
                ) : null}

                <div className={themeContext?.agGridTheme} style={{ height: height ?? 700 }}>
                    <AgGridReact
                        ref={gridRef}
                        defaultColDef={defaultColDef ?? modaDefaultColDef}
                        animateRows={true}
                        {...props}>
                    </AgGridReact>
                </div>
            </Space>
        </div>
    )
}

export default ModaGrid
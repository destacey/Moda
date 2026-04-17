'use client'

import styles from './tree-grid.module.css'
import { Button, Input, Popover, Typography } from 'antd'
import ModaTooltip from '@/src/components/common/moda-tooltip'
import {
  ClearOutlined,
  DownloadOutlined,
  QuestionCircleOutlined,
  ReloadOutlined,
  SearchOutlined,
} from '@ant-design/icons'
import type { TreeGridToolbarProps } from './types'

const { Text } = Typography

/**
 * Generic toolbar for tree grid components.
 * Provides search, row count, refresh, clear filters, export CSV, and help popover.
 * Domain-specific actions go in the `leftSlot` prop.
 */
const TreeGridToolbar = ({
  displayedRowCount,
  totalRowCount,
  searchValue,
  onSearchChange,
  onRefresh,
  onClearFilters,
  hasActiveFilters,
  onExportCsv,
  isLoading,
  leftSlot,
  helpContent,
  rightSlot,
}: TreeGridToolbarProps) => {
  return (
    <div className={styles.toolbar}>
      <div>{leftSlot}</div>

      <div className={styles.toolbarRight}>
        <Text>
          {displayedRowCount} of {totalRowCount}
        </Text>
        <Input
          placeholder="Search"
          allowClear={true}
          value={searchValue}
          onChange={onSearchChange}
          suffix={<SearchOutlined />}
          className={styles.toolbarSearch}
        />
        {onRefresh && (
          <ModaTooltip title="Refresh">
            <Button
              type="text"
              shape="circle"
              icon={<ReloadOutlined />}
              onClick={onRefresh}
            />
          </ModaTooltip>
        )}
        <ModaTooltip title="Clear Filters and Sorting">
          <Button
            type="text"
            shape="circle"
            icon={<ClearOutlined />}
            onClick={onClearFilters}
            disabled={!hasActiveFilters}
          />
        </ModaTooltip>
        <span className={styles.toolbarDivider} />
        {onExportCsv && (
          <ModaTooltip title="Export to CSV">
            <Button
              type="text"
              shape="circle"
              icon={<DownloadOutlined />}
              onClick={onExportCsv}
              disabled={isLoading || displayedRowCount === 0}
            />
          </ModaTooltip>
        )}
        {helpContent && (
          <Popover
            content={helpContent}
            trigger="click"
            placement="bottomRight"
            getPopupContainer={() => document.body}
            overlayStyle={{ maxWidth: 'calc(100vw - 24px)' }}
          >
            <ModaTooltip title="Grid Actions Help">
              <Button
                type="text"
                shape="circle"
                icon={<QuestionCircleOutlined />}
              />
            </ModaTooltip>
          </Popover>
        )}
        {rightSlot && (
          <>
            <span className={styles.toolbarDivider} />
            {rightSlot}
          </>
        )}
      </div>
    </div>
  )
}

export default TreeGridToolbar

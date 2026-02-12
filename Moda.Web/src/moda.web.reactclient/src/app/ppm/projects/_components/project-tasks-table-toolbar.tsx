import { ChangeEvent } from 'react'
import styles from './project-tasks-table.module.css'
import { Button, Input, Popover, Tooltip, Typography } from 'antd'
import {
  ClearOutlined,
  DownloadOutlined,
  PlusOutlined,
  QuestionCircleOutlined,
  ReloadOutlined,
  SearchOutlined,
} from '@ant-design/icons'
import { ProjectTasksHelp } from './project-tasks-table.keyboard-shortcuts'

const { Text } = Typography

interface ProjectTasksTableToolbarProps {
  canManageTasks: boolean
  displayedRowCount: number
  totalRowCount: number
  searchValue: string
  onSearchChange: (e: ChangeEvent<HTMLInputElement>) => void
  refetch: () => Promise<any>
  onClearFilters: () => void
  hasActiveFilters: boolean
  onExportCsv: () => void
  isLoading: boolean
  onCreateTask: () => void
}

const ProjectTasksTableToolbar = ({
  canManageTasks,
  displayedRowCount,
  totalRowCount,
  searchValue,
  onSearchChange,
  refetch,
  onClearFilters,
  hasActiveFilters,
  onExportCsv,
  isLoading,
  onCreateTask,
}: ProjectTasksTableToolbarProps) => {
  return (
    <div className={styles.toolbar}>
      <div>
        {canManageTasks && (
          <Button type="primary" icon={<PlusOutlined />} onClick={onCreateTask}>
            Create Task
          </Button>
        )}
      </div>

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
        {refetch && (
          <Tooltip title="Refresh">
            <Button
              type="text"
              shape="circle"
              icon={<ReloadOutlined />}
              onClick={refetch}
            />
          </Tooltip>
        )}
        <Tooltip title="Clear Filters and Sorting">
          <Button
            type="text"
            shape="circle"
            icon={<ClearOutlined />}
            onClick={onClearFilters}
            disabled={!hasActiveFilters}
          />
        </Tooltip>
        <span className={styles.toolbarDivider} />
        <Tooltip title="Export to CSV">
          <Button
            type="text"
            shape="circle"
            icon={<DownloadOutlined />}
            onClick={onExportCsv}
            disabled={isLoading || displayedRowCount === 0}
          />
        </Tooltip>
        <Popover
          content={<ProjectTasksHelp />}
          trigger="click"
          placement="bottomRight"
          getPopupContainer={() => document.body}
          overlayStyle={{ maxWidth: 'calc(100vw - 24px)' }}
        >
          <Tooltip title="Grid Actions Help">
            <Button
              type="text"
              shape="circle"
              icon={<QuestionCircleOutlined />}
            />
          </Tooltip>
        </Popover>
      </div>
    </div>
  )
}

export default ProjectTasksTableToolbar

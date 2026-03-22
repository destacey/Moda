'use client'

import { ClearOutlined, ReloadOutlined } from '@ant-design/icons'
import { useGetProjectStatusOptionsQuery } from '@/src/store/features/ppm/projects-api'
import { Button, Flex, Skeleton, Space, Tooltip } from 'antd'
import { FC, RefObject } from 'react'
import styles from '../my-projects-dashboard.module.css'

const ROLE_OPTIONS = [
  { label: 'Sponsor', value: 1 },
  { label: 'Owner', value: 2 },
  { label: 'PM', value: 3 },
  { label: 'Member', value: 4 },
  { label: 'Task Assignee', value: 5 },
]

export interface MyProjectsDashboardFilterBarProps {
  selectedRoles: number[]
  onRoleChange: (roles: number[]) => void
  selectedStatuses: number[]
  onStatusChange: (statuses: number[]) => void
  onReset: () => void
  onRefresh: () => void
  containerRef?: RefObject<HTMLDivElement | null>
}

const MyProjectsDashboardFilterBar: FC<MyProjectsDashboardFilterBarProps> = ({
  selectedRoles,
  onRoleChange,
  selectedStatuses,
  onStatusChange,
  onReset,
  onRefresh,
  containerRef,
}) => {
  const { data: statusOptions, isLoading } =
    useGetProjectStatusOptionsQuery()

  if (isLoading) {
    return (
      <div ref={containerRef} className={styles.filterBar}>
        <Skeleton.Input active size="small" style={{ width: 300 }} />
      </div>
    )
  }

  const toggleRole = (value: number) => {
    const next = selectedRoles.includes(value)
      ? selectedRoles.filter((r) => r !== value)
      : [...selectedRoles, value]
    onRoleChange(next)
  }

  const toggleStatus = (value: number) => {
    const next = selectedStatuses.includes(value)
      ? selectedStatuses.filter((s) => s !== value)
      : [...selectedStatuses, value]
    onStatusChange(next)
  }

  return (
    <div ref={containerRef} className={styles.filterBar}>
      <Flex align="center" gap={16} wrap>
        <Space size="small" align="center">
          <span className={styles.filterLabel}>My Role:</span>
          <Flex gap={2}>
            <Button
              size="small"
              className={styles.chipButton}
              color={selectedRoles.length === 0 ? 'primary' : 'default'}
              variant="outlined"
              onClick={() => onRoleChange([])}
            >
              All
            </Button>
            {ROLE_OPTIONS.map((role) => {
              const isSelected = selectedRoles.includes(role.value)
              return (
                <Button
                  key={role.value}
                  size="small"
                  className={styles.chipButton}
                  color={isSelected ? 'primary' : 'default'}
                  variant="outlined"
                  onClick={() => toggleRole(role.value)}
                >
                  {role.label}
                </Button>
              )
            })}
          </Flex>
        </Space>

        <Space size="small" align="center">
          <span className={styles.filterLabel}>Status:</span>
          <Flex gap={2}>
            {statusOptions?.map((status) => {
              const isSelected = selectedStatuses.includes(status.value)
              return (
                <Button
                  key={status.value}
                  size="small"
                  className={styles.chipButton}
                  color={isSelected ? 'primary' : 'default'}
                  variant="outlined"
                  onClick={() => toggleStatus(status.value)}
                >
                  {status.label}
                </Button>
              )
            })}
          </Flex>
        </Space>

        <Flex gap={2}>
          <Tooltip title="Refresh Data">
            <Button
              type="text"
              shape="circle"
              icon={<ReloadOutlined />}
              aria-label="Refresh data"
              onClick={onRefresh}
            />
          </Tooltip>
          <Tooltip title="Reset Filters">
            <Button
              type="text"
              shape="circle"
              icon={<ClearOutlined />}
              aria-label="Reset filters"
              onClick={onReset}
            />
          </Tooltip>
        </Flex>
      </Flex>
    </div>
  )
}

export default MyProjectsDashboardFilterBar

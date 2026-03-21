'use client'

import { PageTitle } from '@/src/components/common'
import { authorizePage } from '@/src/components/hoc'
import {
  useAppDispatch,
  useDocumentTitle,
  useLocalStorageState,
  useRemainingHeight,
} from '@/src/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { useGetProjectsQuery } from '@/src/store/features/ppm/projects-api'
import { usePathname } from 'next/navigation'
import { FC, useCallback, useEffect, useRef, useState } from 'react'
import MyProjectsDashboardFilterBar from './_components/filter-bar'
import MyProjectsSummaryBar from './_components/summary-bar'
import PortfolioGroupList from './_components/portfolio-group-list'
import ProjectDetailPanel from './_components/project-detail-panel'
import styles from './my-projects-dashboard.module.css'
import { useMessage } from '@/src/components/contexts/messaging'

const PROJECT_STATUS = {
  Proposed: 1,
  Approved: 5,
  Active: 2,
  Completed: 3,
  Cancelled: 4,
} as const

const DEFAULT_STATUSES = [PROJECT_STATUS.Approved, PROJECT_STATUS.Active]
const ALL_ROLES = [1, 2, 3, 4, 5]

const getRoleFilterValues = (selectedRoles: number[]): number[] | undefined => {
  if (selectedRoles.length === 0) return ALL_ROLES
  return selectedRoles
}

const MyProjectsPage: FC = () => {
  useDocumentTitle('My Projects')
  const dispatch = useAppDispatch()
  const pathname = usePathname()
  const messageApi = useMessage()

  const [selectedStatuses, setSelectedStatuses] = useLocalStorageState<
    number[]
  >('my-projects-filter-statuses', DEFAULT_STATUSES)
  const [selectedRoles, setSelectedRoles] = useLocalStorageState<number[]>(
    'my-projects-filter-roles',
    [],
  )
  const layoutRef = useRef<HTMLDivElement>(null)
  const layoutHeight = useRemainingHeight(layoutRef)

  const [selectedProjectKey, setSelectedProjectKey] = useState<string | null>(
    null,
  )

  const {
    data: projects,
    isLoading,
    error,
    refetch,
  } = useGetProjectsQuery({
    status: selectedStatuses.length > 0 ? selectedStatuses : undefined,
    role: getRoleFilterValues(selectedRoles),
  })

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error('Failed to load projects.')
    }
  }, [error, messageApi])

  useEffect(() => {
    const breadcrumbRoute: BreadcrumbItem[] = [
      { title: 'PPM' },
      { title: 'Projects', href: '/ppm/projects' },
      { title: 'My Projects' },
    ]
    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname])

  const handleResetFilters = useCallback(() => {
    setSelectedStatuses(DEFAULT_STATUSES)
    setSelectedRoles([])
  }, [setSelectedStatuses, setSelectedRoles])

  return (
    <>
      <PageTitle title="My Projects" />
      <MyProjectsDashboardFilterBar
        selectedRoles={selectedRoles}
        onRoleChange={setSelectedRoles}
        selectedStatuses={selectedStatuses}
        onStatusChange={setSelectedStatuses}
        onReset={handleResetFilters}
        onRefresh={refetch}
      />
      <MyProjectsSummaryBar projects={projects} isLoading={isLoading} />
      <div ref={layoutRef} className={styles.layout} style={{ height: layoutHeight }}>
        <div className={styles.leftPanel}>
          <PortfolioGroupList
            projects={projects}
            isLoading={isLoading}
            selectedProjectKey={selectedProjectKey}
            onSelectProject={setSelectedProjectKey}
          />
        </div>
        <div className={styles.rightPanel}>
          <ProjectDetailPanel projectKey={selectedProjectKey} />
        </div>
      </div>
    </>
  )
}

const MyProjectsPageWithAuthorization = authorizePage(
  MyProjectsPage,
  'Permission',
  'Permissions.Projects.View',
)

export default MyProjectsPageWithAuthorization

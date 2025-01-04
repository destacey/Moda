'use client'

import PageTitle from '@/src/components/common/page-title'
import { authorizePage } from '../../../components/hoc'
import { useDocumentTitle } from '../../../hooks'
import { useGetWorkspacesQuery } from '@/src/store/features/work-management/workspace-api'
import { useEffect, useMemo, useState } from 'react'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { AppstoreOutlined, MenuOutlined } from '@ant-design/icons'
import WorkspacesCardGrid from './workspaces-card-grid'
import WorkspacesGrid from './workspaces-grid'

enum Views {
  Cards,
  List,
}

const viewSelectorOptions: SegmentedLabeledOption[] = [
  {
    value: Views.Cards,
    icon: <AppstoreOutlined alt="Cards" title="Cards" />,
  },
  {
    value: Views.List,
    icon: <MenuOutlined alt="List" title="List" />,
  },
]

const WorkspacesPage: React.FC = () => {
  useDocumentTitle('Workspaces')
  const [currentView, setCurrentView] = useState<string | number>(Views.Cards)

  // TODO: add the ability to filter by active/inactive workspaces on both views

  const {
    data: workspaceData,
    isLoading,
    error,
    refetch,
  } = useGetWorkspacesQuery(true)

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const viewSelector = useMemo(
    () => (
      <Segmented
        options={viewSelectorOptions}
        value={currentView}
        onChange={setCurrentView}
      />
    ),
    [currentView],
  )

  return (
    <>
      <PageTitle title="Workspaces" />
      {currentView === Views.Cards ? (
        <WorkspacesCardGrid
          workspaces={workspaceData}
          viewSelector={viewSelector}
          isLoading={isLoading}
        />
      ) : (
        <WorkspacesGrid
          workspaces={workspaceData}
          viewSelector={viewSelector}
          isLoading={isLoading}
          refetch={refetch}
        />
      )}
    </>
  )
}

const WorkspacesPageWithAuthorization = authorizePage(
  WorkspacesPage,
  'Permission',
  'Permissions.Workspaces.View',
)

export default WorkspacesPageWithAuthorization

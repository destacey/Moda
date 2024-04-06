'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { authorizePage } from '../../components/hoc'
import { useDocumentTitle } from '../../hooks'

const WorkspacesPage = () => {
  useDocumentTitle('Workspaces')
  return (
    <>
      <PageTitle title="Workspaces" />
    </>
  )
}

const WorkspacesPageWithAuthorization = authorizePage(
  WorkspacesPage,
  'Permission',
  'Permissions.Workspaces.View',
)

export default WorkspacesPageWithAuthorization

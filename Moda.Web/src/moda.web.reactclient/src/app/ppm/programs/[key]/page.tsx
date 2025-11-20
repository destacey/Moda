'use client'

import { PageActions, PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import {
  useGetProgramQuery,
  useGetProgramProjectsQuery,
} from '@/src/store/features/ppm/programs-api'
import { Alert, Card, MenuProps } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import ProgramDetailsLoading from './loading'
import {
  ChangeProgramStatusForm,
  DeleteProgramForm,
  EditProgramForm,
  ProgramDetails,
} from '../_components'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { ItemType } from 'antd/es/menu/interface'
import { ProgramStatusAction } from '../_components/change-program-status-form'
import { ProjectViewManager } from '@/src/app/ppm/_components'

enum ProgramTabs {
  Details = 'details',
  Projects = 'projects',
}

const tabs = [
  {
    key: ProgramTabs.Details,
    label: 'Details',
  },
  {
    key: ProgramTabs.Projects,
    label: 'Projects',
  },
]

enum ProgramAction {
  Edit = 'Edit',
  Delete = 'Delete',
  Activate = 'Activate',
  Complete = 'Complete',
  Cancel = 'Cancel',
}

const ProgramDetailsPage = (props: { params: Promise<{ key: string }> }) => {
  const { key } = use(props.params)
  const programKey = Number(key)

  useDocumentTitle('Program Details')

  const [activeTab, setActiveTab] = useState(ProgramTabs.Details)
  const [openEditProgramForm, setOpenEditProgramForm] = useState<boolean>(false)
  const [openActivateProgramForm, setOpenActivateProgramForm] =
    useState<boolean>(false)
  const [openCompleteProgramForm, setOpenCompleteProgramForm] =
    useState<boolean>(false)
  const [openCancelProgramForm, setOpenCancelProgramForm] =
    useState<boolean>(false)
  const [openDeleteProgramForm, setOpenDeleteProgramForm] =
    useState<boolean>(false)

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canUpdateProgram = hasPermissionClaim('Permissions.Programs.Update')
  const canDeleteProgram = hasPermissionClaim('Permissions.Programs.Delete')

  const {
    data: programData,
    isLoading,
    error,
    refetch: refetchProgram,
  } = useGetProgramQuery(programKey)

  const {
    data: projectsData,
    isLoading: projectsDataIsLoading,
    error: projectsDataError,
    refetch: refetchProjectsData,
  } = useGetProgramProjectsQuery(programData?.key.toString(), {
    skip: !programData?.key,
  })

  useEffect(() => {
    if (!programData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'PPM',
      },
      {
        href: `/ppm/programs`,
        title: 'Programs',
      },
    ]

    breadcrumbRoute.push({
      title: 'Details',
    })

    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname, programData])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case ProgramTabs.Details:
        return <ProgramDetails program={programData} />
      case ProgramTabs.Projects:
        return (
          <ProjectViewManager
            projects={projectsData}
            isLoading={projectsDataIsLoading}
            refetch={refetchProjectsData}
            hidePortfolio={true}
            hideProgram={true}
          />
        )
      default:
        return null
    }
  }, [
    activeTab,
    programData,
    refetchProjectsData,
    projectsData,
    projectsDataIsLoading,
  ])

  // doesn't trigger on first render
  const onTabChange = useCallback((tabKey: string) => {
    setActiveTab(tabKey as ProgramTabs)
  }, [])

  const missingDates = programData?.start === null || programData?.end === null

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const currentStatus = programData?.status.name
    const availableActions =
      currentStatus === 'Proposed'
        ? !missingDates
          ? [
              ProgramAction.Edit,
              ProgramAction.Delete,
              ProgramAction.Activate,
              ProgramAction.Cancel,
            ]
          : [ProgramAction.Edit, ProgramAction.Delete, ProgramAction.Cancel]
        : currentStatus === 'Active'
          ? [ProgramAction.Edit, ProgramAction.Complete, ProgramAction.Cancel]
          : []

    const items: ItemType[] = []
    if (canUpdateProgram && availableActions.includes(ProgramAction.Edit)) {
      items.push({
        key: 'edit',
        label: ProgramAction.Edit,
        onClick: () => setOpenEditProgramForm(true),
      })
    }
    if (canDeleteProgram && availableActions.includes(ProgramAction.Delete)) {
      items.push({
        key: 'delete',
        label: ProgramAction.Delete,
        onClick: () => setOpenDeleteProgramForm(true),
      })
    }

    if (
      canUpdateProgram &&
      (availableActions.includes(ProgramAction.Activate) ||
        availableActions.includes(ProgramAction.Complete) ||
        availableActions.includes(ProgramAction.Cancel))
    ) {
      items.push({
        key: 'manage-divider',
        type: 'divider',
      })
    }

    if (canUpdateProgram && availableActions.includes(ProgramAction.Activate)) {
      items.push({
        key: 'activate',
        label: ProgramAction.Activate,
        onClick: () => setOpenActivateProgramForm(true),
      })
    }

    if (canUpdateProgram && availableActions.includes(ProgramAction.Complete)) {
      items.push({
        key: 'complete',
        label: ProgramAction.Complete,
        onClick: () => setOpenCompleteProgramForm(true),
      })
    }

    if (canUpdateProgram && availableActions.includes(ProgramAction.Cancel)) {
      items.push({
        key: 'cancel',
        label: ProgramAction.Cancel,
        onClick: () => setOpenCancelProgramForm(true),
      })
    }

    return items
  }, [
    canDeleteProgram,
    canUpdateProgram,
    missingDates,
    programData?.status.name,
  ])

  const onEditProgramFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditProgramForm(false)
      if (wasSaved) {
        refetchProgram()
      }
    },
    [refetchProgram],
  )

  const onActivateProgramFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenActivateProgramForm(false)
      if (wasSaved) {
        refetchProgram()
      }
    },
    [refetchProgram],
  )

  const onCompleteProgramFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCompleteProgramForm(false)
      if (wasSaved) {
        refetchProgram()
      }
    },
    [refetchProgram],
  )

  const onCancelProgramFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCancelProgramForm(false)
      if (wasSaved) {
        refetchProgram()
      }
    },
    [refetchProgram],
  )

  const onDeleteProgramFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteProgramForm(false)
      if (wasDeleted) {
        router.push('/ppm/programs')
      }
    },
    [router],
  )

  if (isLoading) {
    return <ProgramDetailsLoading />
  }

  if (!programData) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={`${programData?.key} - ${programData?.name}`}
        subtitle="Program Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />

      {missingDates === true && (
        <>
          <Alert
            message="Program Dates are required before activating."
            type="warning"
            showIcon
          />
          <br />
        </>
      )}
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {renderTabContent()}
      </Card>

      {openEditProgramForm && (
        <EditProgramForm
          programKey={programData?.key}
          showForm={openEditProgramForm}
          onFormComplete={() => onEditProgramFormClosed(true)}
          onFormCancel={() => onEditProgramFormClosed(false)}
        />
      )}
      {openActivateProgramForm && (
        <ChangeProgramStatusForm
          program={programData}
          statusAction={ProgramStatusAction.Activate}
          showForm={openActivateProgramForm}
          onFormComplete={() => onActivateProgramFormClosed(true)}
          onFormCancel={() => onActivateProgramFormClosed(false)}
        />
      )}
      {openCompleteProgramForm && (
        <ChangeProgramStatusForm
          program={programData}
          statusAction={ProgramStatusAction.Complete}
          showForm={openCompleteProgramForm}
          onFormComplete={() => onCompleteProgramFormClosed(true)}
          onFormCancel={() => onCompleteProgramFormClosed(false)}
        />
      )}
      {openCancelProgramForm && (
        <ChangeProgramStatusForm
          program={programData}
          statusAction={ProgramStatusAction.Cancel}
          showForm={openCancelProgramForm}
          onFormComplete={() => onCancelProgramFormClosed(true)}
          onFormCancel={() => onCancelProgramFormClosed(false)}
        />
      )}
      {openDeleteProgramForm && (
        <DeleteProgramForm
          program={programData}
          showForm={openDeleteProgramForm}
          onFormComplete={() => onDeleteProgramFormClosed(true)}
          onFormCancel={() => onDeleteProgramFormClosed(false)}
        />
      )}
    </>
  )
}

const ProgramDetailsPageWithAuthorization = authorizePage(
  ProgramDetailsPage,
  'Permission',
  'Permissions.Programs.View',
)

export default ProgramDetailsPageWithAuthorization

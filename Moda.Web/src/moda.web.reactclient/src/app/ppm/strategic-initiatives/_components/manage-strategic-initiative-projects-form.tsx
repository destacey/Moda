'use client'

import {
  AgGridTransfer,
  asDeletableColDefs,
  asDraggableColDefs,
} from '@/src/components/common/grid/ag-grid-transfer'
import { useMessage } from '@/src/components/contexts/messaging'
import { useConfirmModal } from '@/src/hooks'
import {
  ManageStrategicInitiativeProjectsRequest,
  ProjectListDto,
} from '@/src/services/moda-api'
import { useGetPortfolioProjectsQuery } from '@/src/store/features/ppm/portfolios-api'
import {
  useGetStrategicInitiativeProjectsQuery,
  useManageStrategicInitiativeProjectsMutation,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import { ColDef } from 'ag-grid-community'
import { Modal } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'

export interface ManageStrategicInitiativeProjectsFormProps {
  strategicInitiativeId: string
  portfolioKey: number
  onFormComplete: () => void
  onFormCancel: () => void
}

const projectColDefs: ColDef<ProjectListDto>[] = [
  {
    field: 'key',
    headerName: 'Key',
    width: 75,
  },
  {
    field: 'name',
    headerName: 'Name',
    flex: 1,
  },
  {
    field: 'status.name',
    headerName: 'Status',
    width: 100,
  },
]

const leftColDefs = [...asDraggableColDefs(projectColDefs)]

const defaultSort = (a: ProjectListDto, b: ProjectListDto) => {
  return a.name.localeCompare(b.name)
}

const ManageStrategicInitiativeProjectsForm = ({
  strategicInitiativeId,
  portfolioKey,
  onFormComplete,
  onFormCancel,
}: ManageStrategicInitiativeProjectsFormProps) => {
  const [sourceProjects, setSourceProjects] = useState<ProjectListDto[]>([])
  const [targetProjects, setTargetProjects] = useState<ProjectListDto[]>([])

  const messageApi = useMessage()

  const {
    data: existingProjectsData,
    isLoading: existingProjectsIsLoading,
    error: existingProjectsError,
  } = useGetStrategicInitiativeProjectsQuery(strategicInitiativeId)

  const {
    data: projectData,
    isLoading: projectsIsLoading,
    error: projectsError,
  } = useGetPortfolioProjectsQuery({ portfolioIdOrKey: portfolioKey.toString() })

  const [manageProjects] =
    useManageStrategicInitiativeProjectsMutation()

  const { isOpen, isSaving, handleOk, handleCancel } = useConfirmModal({
    onSubmit: useCallback(async () => {
      try {
        const request: ManageStrategicInitiativeProjectsRequest = {
          id: strategicInitiativeId,
          projectIds: targetProjects.map((item) => item.id),
        }

        const response = await manageProjects(request)
        if (response.error) throw response.error

        messageApi.success('Projects updated successfully.')
        return true
      } catch (error) {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the projects. Please try again.',
        )
        console.error(error)
        return false
      }
    }, [manageProjects, strategicInitiativeId, targetProjects, messageApi]),
    onComplete: onFormComplete,
    onCancel: onFormCancel,
    errorMessage:
      'An error occurred while managing the projects. Please try again.',
    permission: 'Permissions.StrategicInitiatives.Update',
  })

  useEffect(() => {
    if (!existingProjectsData) return
    setTargetProjects(existingProjectsData?.slice().sort(defaultSort) ?? [])
  }, [existingProjectsData])

  useEffect(() => {
    if (!projectData) return

    const selectedIds = existingProjectsData?.map((item) => item.id) ?? []
    const filteredProjects = projectData
      .filter((item) => !selectedIds.includes(item.id))
      .sort(defaultSort)

    setSourceProjects(filteredProjects)
  }, [projectData, existingProjectsData])

  const onDragStop = useCallback((items: ProjectListDto[]) => {
    if (items.length === 0) return

    setSourceProjects((prevSource) =>
      prevSource.filter((p) => !items.some((i) => i.id === p.id)),
    )

    setTargetProjects((prevTarget) =>
      [...prevTarget, ...items].sort(defaultSort),
    )
  }, [])

  const handleDelete = useCallback((item: ProjectListDto) => {
    if (!item) return

    setTargetProjects((prevTarget) =>
      prevTarget.filter((p) => p.id !== item.id),
    )

    setSourceProjects((prevSource) => [...prevSource, item].sort(defaultSort))
  }, [])

  const rightColDefs = useMemo(
    () => asDeletableColDefs(projectColDefs, handleDelete),
    [handleDelete],
  )

  return (
    <Modal
      title="Manage Strategic Initiative Projects"
      open={isOpen}
      width={'80vw'}
      onOk={handleOk}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden
    >
      {
        <AgGridTransfer
          leftGridData={sourceProjects}
          rightGridData={targetProjects}
          leftColumnDef={leftColDefs}
          rightColumnDef={rightColDefs}
          onDragStop={onDragStop}
          getRowId={(param) => param.data.id}
        />
      }
    </Modal>
  )
}

export default ManageStrategicInitiativeProjectsForm

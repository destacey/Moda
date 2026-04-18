'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import { useModalForm } from '@/src/hooks'
import {
  ProjectDetailsDto,
  ProjectLifecycleState,
  ProjectPhaseListDto,
} from '@/src/services/wayd-api'
import { getProjectsClient } from '@/src/services/clients'
import {
  useGetProjectLifecycleQuery,
  useGetProjectLifecyclesQuery,
} from '@/src/store/features/ppm/project-lifecycles-api'
import { useChangeProjectLifecycleMutation } from '@/src/store/features/ppm/projects-api'
import { toFormErrors } from '@/src/utils'
import {
  Card,
  Flex,
  Form,
  Modal,
  Select,
  Table,
  Timeline,
  Typography,
} from 'antd'
import { useEffect, useState } from 'react'

const { Item } = Form
const { Text } = Typography

export interface ChangeProjectLifecycleFormProps {
  project: ProjectDetailsDto
  onFormComplete: () => void
  onFormCancel: () => void
}

interface ChangeProjectLifecycleFormValues {
  lifecycleId: string
  phaseMapping: Record<string, string>
}

const ChangeProjectLifecycleForm = ({
  project,
  onFormComplete,
  onFormCancel,
}: ChangeProjectLifecycleFormProps) => {
  const messageApi = useMessage()
  const [changeProjectLifecycle] = useChangeProjectLifecycleMutation()
  const [currentPhases, setCurrentPhases] = useState<ProjectPhaseListDto[]>([])

  // Load current project phases
  useEffect(() => {
    if (!project?.id) return
    getProjectsClient()
      .getProjectPhases(project.id)
      .then((phases) => setCurrentPhases(phases ?? []))
      .catch(() => setCurrentPhases([]))
  }, [project?.id])

  // Load active lifecycles (exclude current)
  const { data: lifecycleData, isLoading: lifecyclesLoading } =
    useGetProjectLifecyclesQuery(ProjectLifecycleState.Active)

  const lifecycleOptions = !lifecycleData ? [] : [...lifecycleData]
      .filter((lc) => lc.id !== project?.projectLifecycle?.id)
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((lc) => ({
        label: lc.name,
        value: lc.id,
      }))

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<ChangeProjectLifecycleFormValues>({
      onSubmit: async (values: ChangeProjectLifecycleFormValues, form) => {
          try {
            // Build phaseMapping from the form's mapping fields
            const phaseMapping: Record<string, string> = {}
            for (const phase of currentPhases) {
              const targetId = values.phaseMapping?.[phase.id]
              if (targetId) {
                phaseMapping[phase.id] = targetId
              }
            }

            const response = await changeProjectLifecycle({
              projectId: project.id,
              request: {
                lifecycleId: values.lifecycleId,
                phaseMapping,
              },
            })
            if (response.error) throw response.error

            messageApi.success('Project lifecycle changed successfully.')
            return true
          } catch (error: any) {
            if (error?.status === 422 && error?.errors) {
              const formErrors = toFormErrors(error.errors)
              form.setFields(formErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            } else {
              messageApi.error(
                error?.detail ??
                  error?.data?.detail ??
                  'An error occurred while changing the lifecycle. Please try again.',
              )
            }
            return false
          }
        },
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while changing the lifecycle. Please try again.',
      permission: 'Permissions.Projects.Update',
    })

  // Watch for lifecycle selection
  const selectedLifecycleId = Form.useWatch('lifecycleId', form)

  const { data: selectedLifecycle } = useGetProjectLifecycleQuery(
    selectedLifecycleId,
    { skip: !selectedLifecycleId },
  )

  // New lifecycle phase options for mapping dropdowns
  const newPhaseOptions = !selectedLifecycle?.phases ? [] : [...selectedLifecycle.phases]
      .sort((a, b) => a.order - b.order)
      .map((phase) => ({
        label: phase.name,
        value: phase.id,
      }))

  // Auto-populate mapping when phase names match
  useEffect(() => {
    if (!selectedLifecycle?.phases || currentPhases.length === 0) return

    const mapping: Record<string, string> = {}
    for (const currentPhase of currentPhases) {
      const match = selectedLifecycle.phases.find(
        (p) => p.name.toLowerCase() === currentPhase.name.toLowerCase(),
      )
      if (match) {
        mapping[currentPhase.id] = match.id
      }
    }

    if (Object.keys(mapping).length > 0) {
      form.setFieldValue('phaseMapping', mapping)
    }
  }, [selectedLifecycle?.phases, currentPhases, form])

  // Phase preview timeline
  const phaseItems = !selectedLifecycle?.phases ? [] : [...selectedLifecycle.phases]
      .sort((a, b) => a.order - b.order)
      .map((phase) => ({
        content: (
          <>
            <Text strong>{phase.name}</Text>
            <br />
            <Text type="secondary">{phase.description}</Text>
          </>
        ),
      }))

  // Phase mapping table columns
  const mappingColumns = [
    {
      title: 'Current Phase',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record: ProjectPhaseListDto) => (
        <Text strong>{`${record.order}. ${name}`}</Text>
      ),
    },
    {
      title: 'Map To',
      key: 'mapping',
      render: (_: unknown, record: ProjectPhaseListDto) => (
        <Item
          name={['phaseMapping', record.id]}
          rules={[{ required: true, message: 'Required' }]}
          style={{ margin: 0 }}
        >
          <Select
            options={newPhaseOptions}
            placeholder="Select target phase"
            size="small"
          />
        </Item>
      ),
    },
  ]

  return (
    <Modal
      title="Change Project Lifecycle"
      open={isOpen}
      width={'50vw'}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Change Lifecycle"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
    >
      <Flex vertical gap="small">
        <Text type="secondary">
          Select a new lifecycle and map existing phases to the new
          lifecycle&apos;s phases. Tasks will be moved to the mapped phases.
        </Text>
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="change-project-lifecycle-form"
        >
          <Item
            name="lifecycleId"
            label="New Lifecycle"
            rules={[{ required: true, message: 'Lifecycle is required' }]}
          >
            <Select
              options={lifecycleOptions}
              placeholder="Select Lifecycle"
              loading={lifecyclesLoading}
            />
          </Item>

          {selectedLifecycleId && phaseItems.length > 0 && (
            <Card size="small" title="New Phases" style={{ marginBottom: 16 }}>
              <Timeline items={phaseItems} />
            </Card>
          )}

          {selectedLifecycleId && currentPhases.length > 0 && (
            <Card size="small" title="Phase Mapping">
              <Text type="secondary" style={{ display: 'block', marginBottom: 8 }}>
                Map each current phase to a phase in the new lifecycle.
              </Text>
              <Table
                dataSource={[...currentPhases].sort(
                  (a, b) => a.order - b.order,
                )}
                columns={mappingColumns}
                pagination={false}
                rowKey="id"
                size="small"
              />
            </Card>
          )}
        </Form>
      </Flex>
    </Modal>
  )
}

export default ChangeProjectLifecycleForm

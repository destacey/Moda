'use client'

import { useCallback, useEffect, useMemo, useState } from 'react'
import dayjs from 'dayjs'
import { Flex, Modal, Select, Spin, Typography } from 'antd'
import {
  MapPlanningIntervalSprintsRequest,
  PlanningIntervalIterationSprintsDto,
  SprintListDto,
} from '@/src/services/moda-api'
import {
  useGetIterationSprintsQuery,
  useMapTeamSprintsMutation,
} from '@/src/store/features/planning/planning-interval-api'
import { useGetTeamSprintsQuery } from '@/src/store/features/organizations/team-api'
import { useMessage } from '@/src/components/contexts/messaging'

const { Text } = Typography

export interface ConfigureTeamSprintMappingsFormProps {
  showForm: boolean
  planningIntervalId: string
  planningIntervalKey: number
  teamId: string
  teamName: string
  teamOfTeamsName: string | null
  onFormSave: () => void
  onFormCancel: () => void
}

interface IterationSprintMapping {
  iterationId: string
  iterationName: string
  iterationStart: Date
  iterationEnd: Date
  iterationCategory: string
  sprintId: string | null
}

const formatDateRange = (start: Date, end: Date): string => {
  return `${dayjs(start).format('MMM D, YYYY')} - ${dayjs(end).format('MMM D, YYYY')}`
}

const formatSprintOption = (sprint: SprintListDto): string => {
  return `${sprint.name} (${formatDateRange(sprint.start, sprint.end)})`
}

const ConfigureTeamSprintMappingsForm = ({
  showForm,
  planningIntervalId,
  planningIntervalKey,
  teamId,
  teamName,
  teamOfTeamsName,
  onFormSave,
  onFormCancel,
}: ConfigureTeamSprintMappingsFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [mappings, setMappings] = useState<IterationSprintMapping[]>([])
  const messageApi = useMessage()

  const { data: iterationSprintsData, isLoading: iterationsLoading } =
    useGetIterationSprintsQuery(
      { idOrKey: planningIntervalKey.toString() },
      { skip: !showForm },
    )

  const { data: teamSprintsData, isLoading: sprintsLoading } =
    useGetTeamSprintsQuery(teamId, { skip: !showForm })

  const [mapTeamSprints] = useMapTeamSprintsMutation()

  const isLoading = iterationsLoading || sprintsLoading

  // Build sprint options for the select dropdown
  const sprintOptions = useMemo(() => {
    if (!teamSprintsData) return []

    return [...teamSprintsData]
      .sort((a, b) => new Date(b.start).getTime() - new Date(a.start).getTime())
      .map((sprint) => ({
        value: sprint.id,
        label: formatSprintOption(sprint),
      }))
  }, [teamSprintsData])

  // Initialize mappings from iteration sprints data
  const initializeMappings = useCallback(
    (iterations: PlanningIntervalIterationSprintsDto[]) => {
      const initialMappings: IterationSprintMapping[] = iterations.map(
        (iteration) => {
          // Find sprint for this team in this iteration
          const existingSprint = iteration.sprints?.find(
            (s) => s.team.id === teamId,
          )

          return {
            iterationId: iteration.id,
            iterationName: iteration.name,
            iterationStart: iteration.start,
            iterationEnd: iteration.end,
            iterationCategory: iteration.category?.name ?? '',
            sprintId: existingSprint?.id ?? null,
          }
        },
      )

      setMappings(initialMappings)
    },
    [teamId],
  )

  useEffect(() => {
    setIsOpen(showForm)
    if (showForm && iterationSprintsData) {
      initializeMappings(iterationSprintsData)
    }
  }, [showForm, iterationSprintsData, initializeMappings])

  const handleSprintChange = useCallback(
    (iterationId: string, sprintId: string | null) => {
      setMappings((prev) =>
        prev.map((m) =>
          m.iterationId === iterationId ? { ...m, sprintId } : m,
        ),
      )
    },
    [],
  )

  const handleOk = async () => {
    setIsSaving(true)
    try {
      // Build the mapping dictionary
      const iterationSprintMappings: { [key: string]: string } = {}
      mappings.forEach((m) => {
        if (m.sprintId) {
          iterationSprintMappings[m.iterationId] = m.sprintId
        }
      })

      const request: MapPlanningIntervalSprintsRequest = {
        id: planningIntervalId,
        teamId: teamId,
        iterationSprintMappings,
      }

      await mapTeamSprints({
        planningIntervalId,
        teamId,
        request,
        cacheKey: planningIntervalKey,
      }).unwrap()

      setIsOpen(false)
      onFormSave()
      messageApi.success(
        `Successfully updated sprint mappings for ${teamName}.`,
      )
    } catch (error) {
      console.error('Error saving sprint mappings:', error)
      messageApi.error(
        'An error occurred while saving sprint mappings. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
  }

  return (
    <Modal
      title={`Configure Sprint Mapping - ${teamName}`}
      open={isOpen}
      width={700}
      onOk={handleOk}
      okText="Save Changes"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      maskClosable={false}
      keyboard={false}
      destroyOnHidden={true}
    >
      <Spin spinning={isLoading} size="large">
        {teamOfTeamsName && (
          <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
            Team of Teams: {teamOfTeamsName}
          </Text>
        )}
        <Flex vertical gap="middle">
          <Flex justify="space-between" align="center" gap="middle">
            <Text strong style={{ minWidth: 180 }}>
              PI Iteration
            </Text>
            <Text strong style={{ flex: 1 }}>
              Team Sprint
            </Text>
          </Flex>
          {mappings.map((mapping) => (
            <Flex
              key={mapping.iterationId}
              justify="space-between"
              align="center"
              gap="middle"
            >
              <div style={{ minWidth: 180 }}>
                <div>
                  <Text strong>
                    {mapping.iterationName} - {mapping.iterationCategory}
                  </Text>
                </div>
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  {formatDateRange(
                    mapping.iterationStart,
                    mapping.iterationEnd,
                  )}
                </Text>
              </div>
              <Select
                style={{ flex: 1 }}
                placeholder="Select sprint..."
                allowClear
                value={mapping.sprintId}
                onChange={(value) =>
                  handleSprintChange(mapping.iterationId, value ?? null)
                }
                options={sprintOptions}
                showSearch
                filterOption={(input, option) =>
                  (option?.label ?? '')
                    .toLowerCase()
                    .includes(input.toLowerCase())
                }
              />
            </Flex>
          ))}
        </Flex>
      </Spin>
    </Modal>
  )
}

export default ConfigureTeamSprintMappingsForm

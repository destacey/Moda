'use client'

import useAuth from '@/src/app/components/contexts/auth'
import {
  AzdoConnectionTeamMappingRequest,
  AzdoWorkspaceTeamMappingRequest,
  AzureDevOpsBoardsWorkspaceTeamDto,
} from '@/src/services/moda-api'
import {
  useGetAzdoConnectionTeamsQuery,
  useMapAzdoConnectionTeamsMutation,
} from '@/src/store/features/app-integration/azdo-integration-api'
import { useGetTeamOptionsQuery } from '@/src/store/features/organizations/team-api'
import { toFormErrors } from '@/src/utils'
import { Flex, Form, Input, Modal, Select, Typography, message } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'

const { Item, List } = Form
const { Text } = Typography

export interface MapAzdoWorkspaceTeamsFormProps {
  showForm: boolean
  connectionId: string
  workspaceId: string
  workspaceName: string
  onFormSave: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

interface AzdoTeamMapping {
  teamId: string
  teamName: string
  internalTeamId: string | null
}

interface MapAzdoWorkspaceTeamsFormValues {
  teamMappings: AzdoTeamMapping[]
}

const mapToRequestValues = (
  connectionId: string,
  workspaceId: string,
  values: MapAzdoWorkspaceTeamsFormValues,
): AzdoConnectionTeamMappingRequest => {
  return {
    connectionId: connectionId,
    teamMappings: values.teamMappings.map(
      (team) =>
        ({
          workspaceId: workspaceId,
          teamId: team.teamId,
          internalTeamId: team.internalTeamId,
        }) as AzdoWorkspaceTeamMappingRequest,
    ),
  } as AzdoConnectionTeamMappingRequest
}

const MapAzdoWorkspaceTeamsForm = (props: MapAzdoWorkspaceTeamsFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<MapAzdoWorkspaceTeamsFormValues>()
  const formValues = Form.useWatch([], form)

  const { hasClaim } = useAuth()
  const canUpdateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Update',
  )

  const {
    data: connectionTeamsData,
    isLoading,
    error,
    refetch,
  } = useGetAzdoConnectionTeamsQuery({
    connectionId: props.connectionId,
    workspaceId: props.workspaceId,
  })

  const { data: teamOptionsData } = useGetTeamOptionsQuery(true)

  const [mapConnectionTeamsMutation] = useMapAzdoConnectionTeamsMutation()

  const mapToFormValues = useCallback(
    (teams: AzureDevOpsBoardsWorkspaceTeamDto[]) => {
      form.setFieldsValue({
        teamMappings: teams.map((team) => ({
          teamId: team.teamId,
          teamName: team.teamName,
          internalTeamId: team.internalTeamId,
        })),
      })
    },
    [form],
  )

  useEffect(() => {
    if (!connectionTeamsData) return
    if (canUpdateConnection) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(connectionTeamsData)
      }
    } else {
      props.onFormCancel()
      props.messageApi.error(
        'You do not have permission to map workspace teams.',
      )
    }
  }, [
    canUpdateConnection,
    connectionTeamsData,
    mapToFormValues,
    props.messageApi,
    props,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  const update = async (
    values: MapAzdoWorkspaceTeamsFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(
        props.connectionId,
        props.workspaceId,
        values,
      )
      const response = await mapConnectionTeamsMutation(request)
      if (response.error) {
        throw response.error
      }
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        props.messageApi.error('Correct the validation error(s) to continue.')
      } else {
        props.messageApi.error(
          error.supportMessage ??
            'An error occurred while update workspace team mappings.',
        )

        console.error(error)
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormSave()
        props.messageApi.success(
          'Successfully updated workspace team mappings.',
        )
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
    form.resetFields()
  }

  return (
    <>
      <Modal
        title="Map Workspace Teams"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Flex gap="small" vertical style={{ marginBottom: '15px' }}>
          <Text type="secondary">
            Map the Azure DevOps workspace teams to Moda teams.
          </Text>
          <Text>Workspace: {props.workspaceName}</Text>
        </Flex>
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="map-workspace-teams-form"
        >
          <Item name="teamMappings">
            <List name="teamMappings">
              {(fields, { add, remove }) => (
                <>
                  {fields.map(({ key, name, ...restField }) => (
                    <div key={key}>
                      <Flex align="center" justify="space-between">
                        <Flex vertical style={{ width: '90%' }}>
                          <Flex gap="small">
                            <Item
                              {...restField}
                              name={[name, 'teamId']}
                              hidden={true}
                            >
                              <Input hidden={true} />
                            </Item>
                            <Item
                              {...restField}
                              style={{ width: '50%' }}
                              name={[name, 'teamName']}
                              rules={[{ required: true }]}
                            >
                              <Input disabled={true} />
                            </Item>
                            <Item
                              {...restField}
                              style={{ width: '50%' }}
                              name={[name, 'internalTeamId']}
                            >
                              <Select
                                allowClear
                                showSearch
                                placeholder="Select a Team"
                                filterOption={(input, option) =>
                                  (option?.label ?? '')
                                    .toLowerCase()
                                    .includes(input.toLowerCase())
                                }
                                options={teamOptionsData}
                              />
                            </Item>
                          </Flex>
                        </Flex>
                      </Flex>
                    </div>
                  ))}
                </>
              )}
            </List>
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default MapAzdoWorkspaceTeamsForm

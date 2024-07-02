import useAuth from '@/src/app/components/contexts/auth'
import { Form, Modal, Table, message } from 'antd'
import { useEffect, useState } from 'react'

const { Item, List } = Form
const { Column } = Table

export interface MapAzdoWorkspaceTeamsFormProps {
  showForm: boolean
  connectionId: string
  workspaceId: string
  workspaceName: string
  onFormSave: () => void
  onFormCancel: () => void
}

interface AzdoTeamMapping {
  teamId: string
  teamName: string
  internalTeamId: string | null
}

interface MapAzdoWorkspaceTeamsFormValues {
  teamMappings: AzdoTeamMapping[]
}

const teamData: AzdoTeamMapping[] = [
  {
    teamId: '1',
    teamName: 'Team 1',
    internalTeamId: null,
  },
  {
    teamId: '2',
    teamName: 'Team 2',
    internalTeamId: null,
  },
]

const MapAzdoWorkspaceTeamsForm = (props: MapAzdoWorkspaceTeamsFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<MapAzdoWorkspaceTeamsFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canUpdateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Update',
  )

  useEffect(() => {
    if (canUpdateConnection) {
      setIsOpen(props.showForm)
      //mapToFormValues(props.workspaceName)
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to map workspace teams.')
    }
  }, [canUpdateConnection, messageApi, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  // const handleOk = async () => {
  //   setIsSaving(true)
  //   try {
  //     const values = await form.validateFields()
  //     if (await init(values)) {
  //       setIsOpen(false)
  //       form.resetFields()
  //       props.onFormSave()
  //       messageApi.success('Successfully initialized workspace.')
  //     }
  //   } catch (errorInfo) {
  //     console.log('handleOk error', errorInfo)
  //   } finally {
  //     setIsSaving(false)
  //   }
  // }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
    //form.resetFields()
  }

  return (
    <>
      {contextHolder}
      <Modal
        title="Map Workspace Teams"
        open={isOpen}
        //onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <div>{props.workspaceName}</div>
        <Form
          form={form}
          size="middle"
          layout="vertical"
          name="map-workspace-teams-form"
        >
          <Item name="teamMappings">
            <Table dataSource={teamData} size="small" pagination={false}>
              <Column title="Team Name" dataIndex="teamName" key="teamName" />
              <Column
                title="Moda Team"
                dataIndex="internalTeamId"
                key="internalTeamId"
              />
            </Table>
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default MapAzdoWorkspaceTeamsForm

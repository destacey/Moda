'use client'

import { Form, Input, Radio } from 'antd'
import { CreateTeamFormValues } from '../types'
import { createTeam, retrieveTeams, setCreateTeamOpen, useCreateTeam } from '../teamsSlice'
import withModalForm, { FormProps } from '../../components/hoc/withModalForm'
import { AppDispatch } from '@/src/store'

const CreateTeamForm = ({form}: FormProps<CreateTeamFormValues>) => {
  return (
    <Form
      form={form}
      size="small"
      layout="vertical"
      name="create-team-form"
    >
      <Form.Item label="Team Type" name="type" rules={[{ required: true }]}>
        <Radio.Group>
          <Radio value="Team">Team</Radio>
          <Radio value="Team of Teams">Team of Teams</Radio>
        </Radio.Group>
      </Form.Item>
      <Form.Item label="Name" name="name" rules={[{ required: true }]}>
        <Input showCount maxLength={128} />
      </Form.Item>
      <Form.Item
        label="Code"
        name="code"
        rules={[
          { required: true, message: 'The Code field is required.' },
          {
            min: 2,
            max: 10,
            message: 'The Code field must be between 2-10 characters.',
          },
          {
            pattern: /^[A-Z0-9]+$/,
            message:
              'The Code field is invalid. Uppercase letters and numbers only.',
          },
        ]}
      >
        <Input
          showCount
          maxLength={10}
          onInput={(e) =>
            ((e.target as HTMLInputElement).value = (
              e.target as HTMLInputElement
            ).value.toUpperCase())
          }
        />
      </Form.Item>
      <Form.Item
        name="description"
        label="Description"
        help="Markdown enabled"
      >
        <Input.TextArea
          autoSize={{ minRows: 6, maxRows: 10 }}
          showCount
          maxLength={1024}
        />
      </Form.Item>
    </Form>
  )
}

export const ModalCreateTeamForm = withModalForm(CreateTeamForm, { 
  title: "Create Team", 
  okText: "Create", 
  useFormState: useCreateTeam,
  onOk: async (values: CreateTeamFormValues, dispatch: AppDispatch) => {
    // TODO: With RTK Query, we could have create team mutation automatically refetch teams
    const result = await dispatch(createTeam(values))
    if(result.meta.requestStatus === 'fulfilled') {
      dispatch(retrieveTeams())
    }
  },
  onCancel: (dispatch: AppDispatch) => {
    dispatch(setCreateTeamOpen(false))
  }
})

export default ModalCreateTeamForm
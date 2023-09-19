'use client'

import { Form, Input } from 'antd'
import { useCallback, useEffect } from 'react'
import {
  TeamDetailsDto,
  TeamOfTeamsDetailsDto,
} from '@/src/services/moda-api'
import { EditTeamFormValues } from '../types'
import withModalForm, { FormProps } from '../../components/hoc/withModalForm'
import { retrieveTeams, setEditMode, updateTeam, selectEditTeamContext } from '../team-slice'
import { AppDispatch } from '@/src/store'
import { useAppSelector } from '../../hooks'

interface EditTeamFormProps extends FormProps<EditTeamFormValues> {
  team: TeamDetailsDto | TeamOfTeamsDetailsDto
}

const EditTeamForm = ({form, team}: EditTeamFormProps) => {

  const mapTeamToFormValues = useCallback(
    (team: EditTeamFormValues) => {
      form.setFieldsValue({
        id: team.id,
        name: team.name,
        code: team.code,
        description: team.description,
        type: team.type
      })
    },
    [form],
  )

  useEffect(() => {
    team && mapTeamToFormValues(team as EditTeamFormValues)
  }, [mapTeamToFormValues, team])

  return (
    <>
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="update-team-form"
      >
        <Form.Item name="id" hidden={true}>
          <Input />
        </Form.Item>
        <Form.Item name="type" hidden={true}>
          <Input />
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
    </>
  )
}

const ModalEditTeamForm = withModalForm(EditTeamForm, {
  title: 'Edit Team',
  okText: 'Save',
  useFormState: () => useAppSelector(selectEditTeamContext),
  onOk: async (values: EditTeamFormValues, dispatch: AppDispatch) => {
    // TODO: With RTK Query, we could have create team mutation automatically refetch teams
    const result = await dispatch(updateTeam(values))
    if(result.meta.requestStatus === 'fulfilled') {
      dispatch(retrieveTeams())
    }
  },
  onCancel: (dispatch: AppDispatch) => {
    dispatch(setEditMode(false))
  }
})

export default ModalEditTeamForm

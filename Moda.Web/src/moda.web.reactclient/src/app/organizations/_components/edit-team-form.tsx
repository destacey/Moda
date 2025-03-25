'use client'

import { Form, Input } from 'antd'
import { useCallback, useEffect } from 'react'
import { TeamDetailsDto, TeamOfTeamsDetailsDto } from '@/src/services/moda-api'
import { EditTeamFormValues } from '../types'
import withModalForm, {
  FormProps,
} from '../../../components/hoc/with-modal-form'
import {
  setEditMode,
  updateTeam,
  selectEditTeamContext,
} from '../../../store/features/organizations/team-slice'
import { useAppSelector } from '../../../hooks'
import { MarkdownEditor } from '../../../components/common/markdown'

const { Item } = Form

interface EditTeamFormProps extends FormProps<EditTeamFormValues> {
  team: TeamDetailsDto | TeamOfTeamsDetailsDto
}

const EditTeamForm = ({ form, team }: EditTeamFormProps) => {
  const mapTeamToFormValues = useCallback(
    (team: EditTeamFormValues) => {
      form.setFieldsValue({
        id: team.id,
        name: team.name,
        code: team.code,
        description: team.description || '',
        type: team.type,
      })
    },
    [form],
  )

  useEffect(() => {
    team && mapTeamToFormValues(team as EditTeamFormValues)
  }, [mapTeamToFormValues, team])

  return (
    <>
      <Form form={form} size="small" layout="vertical" name="update-team-form">
        <Item name="id" hidden={true}>
          <Input />
        </Item>
        <Item name="type" hidden={true}>
          <Input />
        </Item>
        <Item label="Name" name="name" rules={[{ required: true }]}>
          <Input showCount maxLength={128} />
        </Item>
        <Item
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
        </Item>
        <Item name="description" label="Description" rules={[{ max: 1024 }]}>
          <MarkdownEditor
            value={form.getFieldValue('description')}
            onChange={(value) => form.setFieldValue('description', value || '')}
            maxLength={1024}
          />
        </Item>
      </Form>
    </>
  )
}

const ModalEditTeamForm = withModalForm(EditTeamForm, {
  title: 'Edit Team',
  okText: 'Save',
  useFormState: () => useAppSelector(selectEditTeamContext),
  onOk: (values: EditTeamFormValues) => updateTeam(values),
  onCancel: setEditMode(false),
})

export default ModalEditTeamForm

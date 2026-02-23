'use client'

import { AnalyticsDataset, Visibility } from '@/src/services/moda-api'
import {
  Button,
  Card,
  Col,
  Divider,
  Form,
  Input,
  InputNumber,
  Popconfirm,
  Row,
  Select,
  Space,
} from 'antd'
import type { FormInstance } from 'antd/es/form'
import {
  buildDefinitionJson,
  filterOperatorOptions,
  groupByOptions,
  makeDefaultDefinition,
  measureTypeOptions,
  numericFieldOptions,
} from '../builder'
import { FormValues } from '../types'

const { TextArea } = Input

interface AnalyticsViewEditorCardProps {
  form: FormInstance<FormValues>
  selectedId: string | null
  isDetailLoading: boolean
  isSaving: boolean
  canCreate: boolean
  canUpdate: boolean
  canDelete: boolean
  canRun: boolean
  sortFieldOptions: Array<{ label: string; value: string }>
  watchedDefinition: FormValues['definition'] | undefined
  watchedDataset: AnalyticsDataset | undefined
  onRun: () => void
  onDelete: () => void
  onSubmit: () => void
  onFinish: (values: FormValues) => void
}

const AnalyticsViewEditorCard = ({
  form,
  selectedId,
  isDetailLoading,
  isSaving,
  canCreate,
  canUpdate,
  canDelete,
  canRun,
  sortFieldOptions,
  watchedDefinition,
  watchedDataset,
  onRun,
  onDelete,
  onSubmit,
  onFinish,
}: AnalyticsViewEditorCardProps) => {
  return (
    <Card
      title={selectedId ? 'Edit View' : 'Create View'}
      loading={isDetailLoading}
      extra={
        <Space>
          <Button onClick={onRun} disabled={!canRun || !form.getFieldValue('id')}>
            Run Preview
          </Button>
          {selectedId && (
            <Popconfirm
              title="Delete analytics view?"
              onConfirm={onDelete}
              okButtonProps={{ danger: true }}
            >
              <Button danger disabled={!canDelete}>
                Delete
              </Button>
            </Popconfirm>
          )}
          <Button
            type="primary"
            loading={isSaving}
            onClick={onSubmit}
            disabled={selectedId ? !canUpdate : !canCreate}
          >
            {selectedId ? 'Update' : 'Create'}
          </Button>
        </Space>
      }
    >
      <Form<FormValues>
        form={form}
        layout="vertical"
        preserve={false}
        onFinish={onFinish}
      >
        <Form.Item name="id" hidden>
          <Input />
        </Form.Item>

        <Row gutter={12}>
          <Col span={12}>
            <Form.Item
              label="Name"
              name="name"
              rules={[{ required: true, message: 'Name is required.' }]}
            >
              <Input maxLength={128} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Description" name="description">
              <Input maxLength={2048} />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={12}>
          <Col span={8}>
            <Form.Item
              label="Dataset"
              name="dataset"
              initialValue={AnalyticsDataset.WorkItems}
            >
              <Select
                options={[
                  { label: 'Work Items', value: AnalyticsDataset.WorkItems },
                  {
                    label: 'Dependencies',
                    value: AnalyticsDataset.Dependencies,
                    disabled: true,
                  },
                  {
                    label: 'Planning Interval Objectives',
                    value: AnalyticsDataset.PlanningIntervalObjectives,
                    disabled: true,
                  },
                ]}
              />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item
              label="Visibility"
              name="visibility"
              initialValue={Visibility.Private}
            >
              <Select
                options={[
                  { label: 'Private', value: Visibility.Private },
                  { label: 'Public', value: Visibility.Public },
                ]}
              />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item label="Is Active" name="isActive" initialValue={true}>
              <Select
                options={[
                  { label: 'Active', value: true },
                  { label: 'Inactive', value: false },
                ]}
              />
            </Form.Item>
          </Col>
        </Row>

        <Divider>Columns</Divider>
        <Form.List name={['definition', 'columns']}>
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Space
                  key={field.key}
                  align="baseline"
                  style={{ display: 'flex', marginBottom: 8 }}
                >
                  <Form.Item
                    name={[field.name, 'field']}
                    rules={[{ required: true, message: 'Field required.' }]}
                  >
                    <Select
                      style={{ width: 240 }}
                      options={groupByOptions}
                      placeholder="Field"
                    />
                  </Form.Item>
                  <Form.Item name={[field.name, 'alias']}>
                    <Input style={{ width: 200 }} placeholder="Alias (optional)" />
                  </Form.Item>
                  <Button danger onClick={() => remove(field.name)}>
                    Remove
                  </Button>
                </Space>
              ))}
              <Button onClick={() => add({ field: 'key', alias: '' })}>
                Add Column
              </Button>
            </>
          )}
        </Form.List>

        <Divider>Filters</Divider>
        <Form.List name={['definition', 'filters']}>
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Space
                  key={field.key}
                  align="baseline"
                  style={{ display: 'flex', marginBottom: 8 }}
                >
                  <Form.Item
                    name={[field.name, 'field']}
                    rules={[{ required: true, message: 'Field required.' }]}
                  >
                    <Select
                      style={{ width: 220 }}
                      options={groupByOptions}
                      placeholder="Field"
                    />
                  </Form.Item>
                  <Form.Item
                    name={[field.name, 'operator']}
                    rules={[{ required: true, message: 'Operator required.' }]}
                  >
                    <Select
                      style={{ width: 200 }}
                      options={filterOperatorOptions}
                      placeholder="Operator"
                    />
                  </Form.Item>
                  <Form.Item name={[field.name, 'valuesCsv']}>
                    <Input
                      style={{ width: 280 }}
                      placeholder="Values (comma separated)"
                    />
                  </Form.Item>
                  <Button danger onClick={() => remove(field.name)}>
                    Remove
                  </Button>
                </Space>
              ))}
              <Button
                onClick={() =>
                  add({
                    field: 'statusCategory',
                    operator: 'Equals',
                    valuesCsv: '',
                  })
                }
              >
                Add Filter
              </Button>
            </>
          )}
        </Form.List>

        <Divider>Grouping & Measures</Divider>
        <Form.Item label="Group By" name={['definition', 'groupBy']}>
          <Select
            mode="multiple"
            options={groupByOptions}
            placeholder="Select grouping fields"
          />
        </Form.Item>

        <Form.List name={['definition', 'measures']}>
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Space
                  key={field.key}
                  align="baseline"
                  style={{ display: 'flex', marginBottom: 8 }}
                >
                  <Form.Item
                    name={[field.name, 'type']}
                    rules={[{ required: true, message: 'Type required.' }]}
                  >
                    <Select
                      style={{ width: 160 }}
                      options={measureTypeOptions}
                      placeholder="Type"
                    />
                  </Form.Item>
                  <Form.Item name={[field.name, 'field']}>
                    <Select
                      style={{ width: 200 }}
                      options={numericFieldOptions}
                      placeholder="Field"
                      allowClear
                    />
                  </Form.Item>
                  <Form.Item name={[field.name, 'percentile']}>
                    <InputNumber
                      min={0}
                      max={100}
                      style={{ width: 120 }}
                      placeholder="Percentile"
                    />
                  </Form.Item>
                  <Form.Item name={[field.name, 'alias']}>
                    <Input style={{ width: 180 }} placeholder="Alias" />
                  </Form.Item>
                  <Button danger onClick={() => remove(field.name)}>
                    Remove
                  </Button>
                </Space>
              ))}
              <Button onClick={() => add({ type: 'Count', field: 'id', alias: 'Count' })}>
                Add Measure
              </Button>
            </>
          )}
        </Form.List>

        <Divider>Sort</Divider>
        <Form.List name={['definition', 'sort']}>
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Space
                  key={field.key}
                  align="baseline"
                  style={{ display: 'flex', marginBottom: 8 }}
                >
                  <Form.Item
                    name={[field.name, 'field']}
                    rules={[{ required: true, message: 'Sort field required.' }]}
                  >
                    <Select
                      style={{ width: 260 }}
                      options={sortFieldOptions}
                      placeholder="Sort Field"
                    />
                  </Form.Item>
                  <Form.Item
                    name={[field.name, 'direction']}
                    rules={[{ required: true, message: 'Direction required.' }]}
                  >
                    <Select
                      style={{ width: 140 }}
                      options={[
                        { label: 'Ascending', value: 'Asc' },
                        { label: 'Descending', value: 'Desc' },
                      ]}
                    />
                  </Form.Item>
                  <Button danger onClick={() => remove(field.name)}>
                    Remove
                  </Button>
                </Space>
              ))}
              <Button onClick={() => add({ field: 'key', direction: 'Asc' })}>
                Add Sort
              </Button>
            </>
          )}
        </Form.List>

        <Divider>Definition Preview</Divider>
        <TextArea
          value={buildDefinitionJson(
            watchedDefinition ?? makeDefaultDefinition(),
            watchedDataset ?? AnalyticsDataset.WorkItems,
          )}
          autoSize={{ minRows: 10, maxRows: 16 }}
          readOnly
        />
      </Form>
    </Card>
  )
}

export default AnalyticsViewEditorCard

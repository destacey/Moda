import { renderHook, act } from '@testing-library/react'
import { useTreeGridEditing } from '../use-tree-grid-editing'
import type { TreeNode, TreeGridEditingConfig } from '../types'

interface TestNode extends TreeNode {
  name: string
  value: number
}

const createTestNode = (
  id: string,
  name: string,
  children: TestNode[] = [],
): TestNode => ({
  id,
  name,
  value: 0,
  children,
})

// Minimal mock of Ant Design FormInstance
const createMockForm = () => {
  const values: Record<string, any> = {}
  return {
    getFieldsValue: jest.fn(() => ({ ...values })),
    setFieldsValue: jest.fn((v: Record<string, any>) => {
      Object.assign(values, v)
    }),
    resetFields: jest.fn(() => {
      for (const key of Object.keys(values)) {
        delete values[key]
      }
    }),
    validateFields: jest.fn(() => Promise.resolve()),
    isFieldsTouched: jest.fn(() => false),
    _values: values,
  }
}

const createDefaultConfig = (
  overrides: Partial<TreeGridEditingConfig<TestNode>> = {},
): TreeGridEditingConfig<TestNode> => {
  const data = [
    createTestNode('1', 'Node 1'),
    createTestNode('2', 'Node 2'),
    createTestNode('3', 'Node 3'),
  ]

  return {
    data,
    canEdit: true,
    form: createMockForm() as any,
    tableWrapperClassName: 'tableWrapper',
    editableColumnIds: ['name', 'value'],
    onSave: jest.fn(() => Promise.resolve(true)),
    fieldErrors: {},
    setFieldErrors: jest.fn(),
    getFormValues: jest.fn((rowId) => {
      const node = data.find((n) => n.id === rowId)
      return node ? { name: node.name, value: node.value } : { name: '', value: 0 }
    }),
    computeChanges: jest.fn((_rowId, formValues) => formValues),
    cellIdColumnMatchOrder: ['value', 'name'],
    ...overrides,
  }
}

describe('useTreeGridEditing', () => {
  describe('initialization', () => {
    it('starts with no selection', () => {
      const config = createDefaultConfig()
      const { result } = renderHook(() => useTreeGridEditing(config))

      expect(result.current.selectedRowId).toBeNull()
      expect(result.current.selectedCellId).toBeNull()
      expect(result.current.isSaving).toBe(false)
    })

    it('provides tableRef', () => {
      const config = createDefaultConfig()
      const { result } = renderHook(() => useTreeGridEditing(config))

      expect(result.current.tableRef).toBeDefined()
      expect(result.current.tableRef.current).toBeNull()
    })
  })

  describe('editableColumns', () => {
    it('returns static columns when editableColumnIds is an array', () => {
      const config = createDefaultConfig({
        editableColumnIds: ['name', 'value'],
      })
      const { result } = renderHook(() => useTreeGridEditing(config))

      expect(result.current.editableColumns).toEqual(['name', 'value'])
    })

    it('returns dynamic columns when editableColumnIds is a function', () => {
      const config = createDefaultConfig({
        editableColumnIds: (selectedRowId) =>
          selectedRowId?.startsWith('draft-')
            ? ['type', 'name', 'value']
            : ['name', 'value'],
      })
      const { result } = renderHook(() => useTreeGridEditing(config))

      // No row selected — null selectedRowId
      expect(result.current.editableColumns).toEqual(['name', 'value'])
    })
  })

  describe('selection state', () => {
    it('setSelectedRowId updates selectedRowId', () => {
      const config = createDefaultConfig()
      const { result } = renderHook(() => useTreeGridEditing(config))

      act(() => {
        result.current.setSelectedRowId('1')
      })

      expect(result.current.selectedRowId).toBe('1')
    })

    it('setSelectedCellId updates selectedCellId', () => {
      const config = createDefaultConfig()
      const { result } = renderHook(() => useTreeGridEditing(config))

      act(() => {
        result.current.setSelectedRowId('1')
        result.current.setSelectedCellId('1-name')
      })

      expect(result.current.selectedCellId).toBe('1-name')
    })

    it('initializes form when row is selected', () => {
      const config = createDefaultConfig()
      const { result } = renderHook(() => useTreeGridEditing(config))

      act(() => {
        result.current.setSelectedRowId('1')
      })

      expect(config.getFormValues).toHaveBeenCalledWith('1', config.data)
      expect(config.form.setFieldsValue).toHaveBeenCalled()
    })

    it('resets form when selection is cleared', () => {
      const config = createDefaultConfig()
      const { result } = renderHook(() => useTreeGridEditing(config))

      act(() => {
        result.current.setSelectedRowId('1')
      })

      act(() => {
        result.current.setSelectedRowId(null)
      })

      // resetFields called on both select and deselect
      expect(config.form.resetFields).toHaveBeenCalled()
    })
  })

  describe('getFieldError', () => {
    it('returns undefined when no error exists', () => {
      const config = createDefaultConfig({ fieldErrors: {} })
      const { result } = renderHook(() => useTreeGridEditing(config))

      expect(result.current.getFieldError('name')).toBeUndefined()
    })

    it('returns error string when error exists', () => {
      const config = createDefaultConfig({
        fieldErrors: { name: 'Name is required' },
      })
      const { result } = renderHook(() => useTreeGridEditing(config))

      expect(result.current.getFieldError('name')).toBe('Name is required')
    })
  })

  describe('saveFormChanges', () => {
    it('calls computeChanges and onSave', async () => {
      const onSave = jest.fn(() => Promise.resolve(true))
      const computeChanges = jest.fn(() => ({ name: 'Updated' }))
      const config = createDefaultConfig({ onSave, computeChanges })
      const { result } = renderHook(() => useTreeGridEditing(config))

      act(() => {
        result.current.setSelectedRowId('1')
      })

      let success: boolean
      await act(async () => {
        success = await result.current.saveFormChanges('1')
      })

      expect(computeChanges).toHaveBeenCalled()
      expect(onSave).toHaveBeenCalledWith('1', { name: 'Updated' })
      expect(success!).toBe(true)
    })

    it('skips save when computeChanges returns null', async () => {
      const onSave = jest.fn(() => Promise.resolve(true))
      const computeChanges = jest.fn(() => null)
      const config = createDefaultConfig({ onSave, computeChanges })
      const { result } = renderHook(() => useTreeGridEditing(config))

      act(() => {
        result.current.setSelectedRowId('1')
      })

      let success: boolean
      await act(async () => {
        success = await result.current.saveFormChanges('1')
      })

      expect(onSave).not.toHaveBeenCalled()
      expect(success!).toBe(true) // No changes = success
    })

    it('returns false when validateFields returns errors', async () => {
      const onSave = jest.fn(() => Promise.resolve(true))
      const setFieldErrors = jest.fn()
      const validateFields = jest.fn(() => ({
        name: 'Name is required',
      }))
      const config = createDefaultConfig({
        onSave,
        setFieldErrors,
        validateFields,
      })
      const { result } = renderHook(() => useTreeGridEditing(config))

      act(() => {
        result.current.setSelectedRowId('1')
      })

      let success: boolean
      await act(async () => {
        success = await result.current.saveFormChanges('1')
      })

      expect(onSave).not.toHaveBeenCalled()
      expect(setFieldErrors).toHaveBeenCalledWith({
        name: 'Name is required',
      })
      expect(success!).toBe(false)
    })

    it('returns false when onSave fails', async () => {
      const onSave = jest.fn(() => Promise.resolve(false))
      const config = createDefaultConfig({ onSave })
      const { result } = renderHook(() => useTreeGridEditing(config))

      act(() => {
        result.current.setSelectedRowId('1')
      })

      let success: boolean
      await act(async () => {
        success = await result.current.saveFormChanges('1')
      })

      expect(success!).toBe(false)
    })

    it('returns false when form validation throws', async () => {
      const mockForm = createMockForm()
      mockForm.validateFields.mockRejectedValue(new Error('Validation failed'))
      const onSave = jest.fn()
      const config = createDefaultConfig({ form: mockForm as any, onSave })
      const { result } = renderHook(() => useTreeGridEditing(config))

      act(() => {
        result.current.setSelectedRowId('1')
      })

      let success: boolean
      await act(async () => {
        success = await result.current.saveFormChanges('1')
      })

      expect(onSave).not.toHaveBeenCalled()
      expect(success!).toBe(false)
    })
  })

  describe('draft support', () => {
    it('identifies draft rows by prefix', () => {
      const config = createDefaultConfig({
        editableColumnIds: (rowId) =>
          rowId?.startsWith('draft-')
            ? ['type', 'name', 'value']
            : ['name', 'value'],
      })
      const { result } = renderHook(() => useTreeGridEditing(config))

      // Select a draft row
      act(() => {
        result.current.setSelectedRowId('draft-1')
      })

      expect(result.current.editableColumns).toEqual([
        'type',
        'name',
        'value',
      ])
    })

    it('uses custom draftPrefix', () => {
      const config = createDefaultConfig({
        draftPrefix: 'new-',
        editableColumnIds: (rowId) =>
          rowId?.startsWith('new-')
            ? ['type', 'name']
            : ['name'],
      })
      const { result } = renderHook(() => useTreeGridEditing(config))

      act(() => {
        result.current.setSelectedRowId('new-1')
      })

      expect(result.current.editableColumns).toEqual(['type', 'name'])
    })
  })

  describe('canEdit guard', () => {
    it('handleRowClick does nothing when canEdit is false', async () => {
      const config = createDefaultConfig({ canEdit: false })
      const { result } = renderHook(() => useTreeGridEditing(config))

      const mockEvent = {
        target: document.createElement('td'),
      } as unknown as React.MouseEvent

      await act(async () => {
        await result.current.handleRowClick(mockEvent, {
          rowId: '1',
          isEditableColumn: () => true,
          getClickedColumnId: () => 'name',
        })
      })

      expect(result.current.selectedRowId).toBeNull()
    })
  })
})

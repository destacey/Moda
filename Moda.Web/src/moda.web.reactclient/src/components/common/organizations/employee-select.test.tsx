import { render, screen, fireEvent } from '@testing-library/react'
import EmployeeSelect from './employee-select'
import { BaseOptionType } from 'antd/es/select'

const mockEmployees: BaseOptionType[] = [
  { value: '1', label: 'John Doe' },
  { value: '2', label: 'Jane Smith' },
]

describe('EmployeeSelect', () => {
  it('renders correctly', () => {
    render(<EmployeeSelect employees={mockEmployees} />)
    expect(screen.getByRole('combobox')).toBeInTheDocument()
  })

  it('sets the correct placeholder text', () => {
    render(
      <EmployeeSelect
        employees={mockEmployees}
        placeholder="Select awesome employee"
      />,
    )
    // getByPlaceholderText does not work with AntD Select
    expect(screen.getByText('Select awesome employee')).toBeInTheDocument()
  })

  it('sets the default placeholder text when allowMultiple is true', () => {
    render(<EmployeeSelect employees={mockEmployees} allowMultiple />)
    expect(screen.getByText('Select one or more employees')).toBeInTheDocument()
  })

  it('sets the default placeholder text when allowMultiple is false', () => {
    render(<EmployeeSelect employees={mockEmployees} />)
    expect(screen.getByText('Select an employee')).toBeInTheDocument()
  })

  it('allows multiple selection when allowMultiple is true', () => {
    render(<EmployeeSelect employees={mockEmployees} allowMultiple />)
    const select = screen.getByRole('combobox')
    fireEvent.mouseDown(select)
    fireEvent.click(screen.getByText('John Doe'))
    fireEvent.click(screen.getByText('Jane Smith'))
    const selectedItems = screen.getAllByText(/John Doe|Jane Smith/, {
      selector: '.ant-select-selection-item-content',
    })
    expect(selectedItems).toHaveLength(2)
    expect(selectedItems[0]).toHaveTextContent('John Doe')
    expect(selectedItems[1]).toHaveTextContent('Jane Smith')
  })

  it('filters options based on input', () => {
    render(<EmployeeSelect employees={mockEmployees} />)
    const select = screen.getByRole('combobox')
    fireEvent.change(select, { target: { value: 'Jane' } })
    expect(screen.getByText('Jane Smith')).toBeInTheDocument()
    expect(screen.queryByText('John Doe')).not.toBeInTheDocument()
  })
})

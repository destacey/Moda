import { EmployeeDetailsDto } from '@/src/services/moda-api'
import { Descriptions } from 'antd'
import Link from 'next/link'

const { Item } = Descriptions

const EmployeeDetails = (employee: EmployeeDetailsDto) => {
  const hireDate = employee.hireDate
    ? new Date(employee.hireDate).toLocaleDateString()
    : null
  return (
    <>
      <Descriptions>
        <Item label="Job Title">{employee.jobTitle}</Item>
        <Item label="Department">{employee.department}</Item>
        <Item label="Manager">
          <Link href={`/organizations/employees/${employee.managerLocalId}`}>
            {employee.managerName}
          </Link>
        </Item>
        <Item label="Office Location">{employee.officeLocation}</Item>
        <Item label="Hire Date">{hireDate}</Item>
        <Item label="Is Active?">{employee.isActive?.toString()}</Item>
      </Descriptions>
    </>
  )
}

export default EmployeeDetails

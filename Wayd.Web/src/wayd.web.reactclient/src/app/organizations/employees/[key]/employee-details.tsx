import { EmployeeDetailsDto } from '@/src/services/moda-api'
import { Descriptions } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

const { Item } = Descriptions

interface EmployeeDetailsProps {
  employee: EmployeeDetailsDto
}

const EmployeeDetails = ({ employee }: EmployeeDetailsProps) => {
  if (!employee) return null

  return (
    <>
      <Descriptions>
        <Item label="Email">{employee.email}</Item>
        <Item label="Job Title">{employee.jobTitle}</Item>
        <Item label="Department">{employee.department}</Item>
        <Item label="Manager">
          <Link href={`/organizations/employees/${employee.manager?.key}`}>
            {employee.manager?.name}
          </Link>
        </Item>
        <Item label="Office Location">{employee.officeLocation}</Item>
        <Item label="Hire Date">
          {employee.hireDate && dayjs(employee.hireDate).format('M/D/YYYY')}
        </Item>
      </Descriptions>
    </>
  )
}

export default EmployeeDetails

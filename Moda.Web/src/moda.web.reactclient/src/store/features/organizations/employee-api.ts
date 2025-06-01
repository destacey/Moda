import { BaseOptionType } from 'antd/es/select'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { getEmployeesClient } from '@/src/services/clients'
import { EmployeeDetailsDto, EmployeeListDto } from '@/src/services/moda-api'

export const employeeApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getEmployees: builder.query<EmployeeListDto[], boolean | undefined>({
      queryFn: async (includeInactive) => {
        try {
          const employees = await getEmployeesClient().getList(includeInactive)
          const data = employees.sort((a, b) =>
            a.displayName.localeCompare(b.displayName),
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [
        QueryTags.Employee,
        { type: QueryTags.Employee, id: 'LIST' },
      ],
    }),
    getEmployee: builder.query<EmployeeDetailsDto, number>({
      queryFn: async (key) => {
        try {
          const data = await getEmployeesClient().getEmployee(key.toString())
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.Employee, id: arg },
      ],
    }),
    getEmployeeOptions: builder.query<BaseOptionType[], boolean | undefined>({
      queryFn: async (includeInactive) => {
        try {
          const employees = await getEmployeesClient().getList(includeInactive)
          const data: BaseOptionType[] = employees
            .sort((a, b) => a.displayName.localeCompare(b.displayName))
            .map((employee) => ({
              label: employee.isActive
                ? employee.displayName
                : `${employee.displayName} (Inactive)`,
              value: employee.id,
            }))
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [QueryTags.EmployeeOption],
    }),
  }),
})

export const {
  useGetEmployeesQuery,
  useGetEmployeeQuery,
  useGetEmployeeOptionsQuery,
} = employeeApi

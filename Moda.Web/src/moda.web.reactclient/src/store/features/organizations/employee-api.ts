import { BaseOptionType } from 'antd/es/select'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { getEmployeesClient } from '@/src/services/clients'

export const employeeApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getEmployeeOptions: builder.query<BaseOptionType[], boolean>({
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

export const { useGetEmployeeOptionsQuery } = employeeApi

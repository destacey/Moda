export const getWorkStatusCategoryColor = (statusCategory: string) => {
  switch (statusCategory) {
    case 'Active':
      return 'processing'
    case 'Done':
      return 'success'
    case 'Removed':
      return 'error'
    case 'Proposed':
    default:
      return 'default'
  }
}

export const getObjectiveStatusColor = (status: string) => {
  switch (status) {
    case 'In Progress':
      return 'processing'
    case 'Completed':
      return 'success'
    case 'Canceled':
    case 'Missed':
      return 'error'
    default:
      return 'default'
  }
}

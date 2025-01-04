export const healthCheckTagColor = (status: string) => {
  switch (status) {
    case 'Healthy':
      return 'success'
    case 'At Risk':
      return 'warning'
    case 'Unhealthy':
      return 'error'
    default:
      return 'default'
  }
}

import { healthCheckTagColor } from './health-check-utils'

describe('healthCheckTagColor', () => {
  it('should return "success" for "Healthy" status', () => {
    expect(healthCheckTagColor('Healthy')).toBe('success')
  })

  it('should return "warning" for "At Risk" status', () => {
    expect(healthCheckTagColor('At Risk')).toBe('warning')
  })

  it('should return "error" for "Unhealthy" status', () => {
    expect(healthCheckTagColor('Unhealthy')).toBe('error')
  })

  it('should return "default" for any other status', () => {
    expect(healthCheckTagColor('Other')).toBe('default')
  })
})

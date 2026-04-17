import {
  getLuminance,
  getLuminanceTheme,
  getLifecyclePhaseColor,
  getLifecyclePhaseColorFromStatus,
  getLifecyclePhaseTagColor,
} from './color-helper'
import { LifecyclePhase } from '../components/types'
import { LifecycleNavigationDto } from '../services/moda-api'

describe('getLuminance', () => {
  test('should return 1 for a light color', () => {
    expect(getLuminance('#FFFFFF')).toBeCloseTo(1, 5)
  })

  test('should return 0 for a dark color', () => {
    expect(getLuminance('#000000')).toBeCloseTo(0, 5)
  })

  test('should return approximately 0.5 for a mid-tone color', () => {
    expect(getLuminance('#808080')).toBeCloseTo(0.5)
  })

  test('should handle invalid color format gracefully', () => {
    expect(() => getLuminance('invalid')).toThrow('Invalid hex color format')
  })
})

describe('getLuminanceTheme', () => {
  test('should return "light" for a light color', () => {
    expect(getLuminanceTheme('#FFFFFF')).toBe('light')
  })

  test('should return "dark" for a dark color', () => {
    expect(getLuminanceTheme('#000000')).toBe('dark')
  })

  test('should return "light" for a mid-tone color', () => {
    expect(getLuminanceTheme('#808080')).toBe('light')
  })

  test('should handle invalid color format gracefully', () => {
    expect(() => getLuminanceTheme('invalid')).toThrow(
      'Invalid hex color format',
    )
  })
})

describe('getLifecyclePhaseColor', () => {
  const mockToken = {
    colorPrimary: '#1677ff',
    colorSuccess: '#52c41a',
    colorTextTertiary: '#00000073',
  }

  test('should return colorPrimary for Active phase', () => {
    expect(getLifecyclePhaseColor(LifecyclePhase.Active, mockToken as any)).toBe(
      mockToken.colorPrimary,
    )
  })

  test('should return colorSuccess for Done phase', () => {
    expect(getLifecyclePhaseColor(LifecyclePhase.Done, mockToken as any)).toBe(
      mockToken.colorSuccess,
    )
  })

  test('should return undefined for NotStarted phase', () => {
    expect(getLifecyclePhaseColor(LifecyclePhase.NotStarted, mockToken as any)).toBe(
      undefined,
    )
  })

  test('should return undefined for invalid phase', () => {
    expect(getLifecyclePhaseColor(999 as LifecyclePhase, mockToken as any)).toBe(
      undefined,
    )
  })
})

describe('getLifecyclePhaseColorFromStatus', () => {
  const mockToken = {
    colorPrimary: '#1677ff',
    colorSuccess: '#52c41a',
    colorTextTertiary: '#00000073',
  }

  test('should return colorPrimary for Active lifecyclePhase', () => {
    const status: LifecycleNavigationDto = {
      id: 1,
      name: 'In Progress',
      lifecyclePhase: 'Active',
    }
    expect(getLifecyclePhaseColorFromStatus(status, mockToken)).toBe(
      mockToken.colorPrimary,
    )
  })

  test('should return colorSuccess for Done lifecyclePhase', () => {
    const status: LifecycleNavigationDto = {
      id: 2,
      name: 'Completed',
      lifecyclePhase: 'Done',
    }
    expect(getLifecyclePhaseColorFromStatus(status, mockToken)).toBe(
      mockToken.colorSuccess,
    )
  })

  test('should return undefined for NotStarted lifecyclePhase', () => {
    const status: LifecycleNavigationDto = {
      id: 3,
      name: 'Proposed',
      lifecyclePhase: 'NotStarted',
    }
    expect(getLifecyclePhaseColorFromStatus(status, mockToken)).toBe(undefined)
  })

  test('should handle invalid lifecyclePhase string', () => {
    const status: LifecycleNavigationDto = {
      id: 4,
      name: 'Unknown',
      lifecyclePhase: 'InvalidPhase' as any,
    }
    expect(getLifecyclePhaseColorFromStatus(status, mockToken)).toBe(undefined)
  })
})

describe('getLifecyclePhaseTagColor', () => {
  test('should return "processing" for Active phase', () => {
    expect(getLifecyclePhaseTagColor(LifecyclePhase.Active)).toBe('processing')
  })

  test('should return "success" for Done phase', () => {
    expect(getLifecyclePhaseTagColor(LifecyclePhase.Done)).toBe('success')
  })

  test('should return "default" for NotStarted phase', () => {
    expect(getLifecyclePhaseTagColor(LifecyclePhase.NotStarted)).toBe('default')
  })

  test('should return "default" for invalid phase', () => {
    expect(getLifecyclePhaseTagColor(999 as LifecyclePhase)).toBe('default')
  })
})

import React from 'react'
import '@testing-library/jest-dom'

import crypto from 'crypto'

// Mock BroadcastChannel to prevent ReferenceError in Jest (Node.js environment)
global.BroadcastChannel = class BroadcastChannel {
  name: string
  onmessage: ((this: BroadcastChannel, ev: MessageEvent) => any) | null = null
  onmessageerror: ((this: BroadcastChannel, ev: MessageEvent) => any) | null =
    null

  constructor(name: string) {
    this.name = name
  }

  postMessage(message: any) {}

  close() {}

  addEventListener(
    type: string,
    listener: EventListenerOrEventListenerObject,
  ) {}

  removeEventListener(
    type: string,
    listener: EventListenerOrEventListenerObject,
  ) {}

  dispatchEvent(event: Event): boolean {
    return false
  }
}

Object.assign(navigator, {
  clipboard: {
    writeText: jest.fn(),
  },
})

Object.defineProperty(window, 'crypto', {
  value: {
    getRandomValues: (arr) => crypto.randomBytes(arr.length),
    subtle: {
      digest: async (algorithm, data) => {
        const hash = crypto.createHash(algorithm.toLowerCase().replace('-', ''))
        hash.update(data)
        return hash.digest()
      },
      importKey: async () => ({}),
      derive: async () => new Uint8Array(32),
      encrypt: async () => new Uint8Array(32),
      decrypt: async () => new Uint8Array(32),
    },
  },
})

// Mock sessionStorage
Object.defineProperty(window, 'sessionStorage', {
  value: {
    getItem: jest.fn(),
    setItem: jest.fn(),
    removeItem: jest.fn(),
    clear: jest.fn(),
  },
})

// Mock localStorage
Object.defineProperty(window, 'localStorage', {
  value: {
    getItem: jest.fn(),
    setItem: jest.fn(),
    removeItem: jest.fn(),
    clear: jest.fn(),
  },
})

// Mock next/navigation
jest.mock('next/navigation', () => ({
  useRouter() {
    return {
      push: jest.fn(),
      replace: jest.fn(),
      prefetch: jest.fn(),
    }
  },
}))

jest.mock('react-markdown', () => ({
  __esModule: true,
  default: jest.fn(() =>
    React.createElement('div', null, 'Mocked ReactMarkdown'),
  ),
}))

jest.mock('remark-gfm', () => ({}))

jest.mock('rehype-raw', () => ({
  __esModule: true,
  default: jest.fn(),
}))

// Mock the useTheme hook globally
jest.mock('./components/contexts/theme', () => ({
  __esModule: true,
  default: () => ({
    agGridTheme: 'ag-theme-alpine',
    token: {
      colorPrimary: '#1890ff',
      colorWarning: '#faad14',
      colorSuccess: '#52c41a',
      colorError: '#ff4d4f',
      colorInfo: '#1890ff',
    },
  }),
}))

// Mock dayjs globally
jest.mock('dayjs', () => {
  const mockDayjs = (date?: any) => ({
    format: (formatStr: string) => {
      if (!date) return ''
      const d = new Date(date)
      if (isNaN(d.getTime())) return ''

      const monthNames = [
        'Jan',
        'Feb',
        'Mar',
        'Apr',
        'May',
        'Jun',
        'Jul',
        'Aug',
        'Sep',
        'Oct',
        'Nov',
        'Dec',
      ]
      const month = monthNames[d.getMonth()]
      const day = d.getDate()
      const year = d.getFullYear()
      const hours = d.getHours()
      const minutes = d.getMinutes()
      const ampm = hours >= 12 ? 'PM' : 'AM'
      const displayHours = hours % 12 || 12

      // Simple format support for common patterns
      if (formatStr === 'M/D/YYYY') {
        return `${d.getMonth() + 1}/${d.getDate()}/${d.getFullYear()}`
      }
      if (formatStr === 'YYYY-MM-DD') {
        return d.toISOString().split('T')[0]
      }
      if (formatStr === 'MMM D') {
        return `${month} ${day}`
      }
      if (formatStr === 'MMM D, YYYY') {
        return `${month} ${day}, ${year}`
      }
      if (formatStr === 'MMM D, h:mm A') {
        return `${month} ${day}, ${displayHours}:${String(minutes).padStart(2, '0')} ${ampm}`
      }
      if (formatStr === 'MMM D, YYYY h:mm A') {
        return `${month} ${day}, ${year} ${displayHours}:${String(minutes).padStart(2, '0')} ${ampm}`
      }
      return d.toISOString()
    },
    toISOString: () => {
      if (!date) return ''
      return new Date(date).toISOString()
    },
    valueOf: () => {
      if (!date) return 0
      return new Date(date).valueOf()
    },
    startOf: (unit: string) => {
      if (!date) return mockDayjs(null)
      const d = new Date(date)
      if (isNaN(d.getTime())) return mockDayjs(null)

      if (unit === 'day') {
        d.setHours(0, 0, 0, 0)
      }
      return mockDayjs(d)
    },
    diff: (other: any, unit: string) => {
      if (!date || !other) return 0
      const d1 = new Date(date)
      const d2 = new Date(other.valueOf ? other.valueOf() : other)
      if (isNaN(d1.getTime()) || isNaN(d2.getTime())) return 0

      const diffMs = d1.getTime() - d2.getTime()
      if (unit === 'day') {
        return Math.floor(diffMs / (1000 * 60 * 60 * 24))
      }
      return 0
    },
    isSame: (other: any, unit: string) => {
      if (!date || !other) return false
      const d1 = new Date(date)
      const d2 = new Date(other.valueOf ? other.valueOf() : other)
      if (isNaN(d1.getTime()) || isNaN(d2.getTime())) return false

      if (unit === 'year') {
        return d1.getFullYear() === d2.getFullYear()
      }
      return false
    },
  })
  mockDayjs.extend = jest.fn()
  return mockDayjs
})

Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(), // deprecated
    removeListener: jest.fn(), // deprecated
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
})

// Suppress console errors during tests
// beforeAll(() => {
//   console.error = jest.fn()
// })

afterEach(() => {
  jest.clearAllMocks()
})

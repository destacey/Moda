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

      // Simple format support for common patterns
      if (formatStr === 'M/D/YYYY') {
        return `${d.getMonth() + 1}/${d.getDate()}/${d.getFullYear()}`
      }
      if (formatStr === 'YYYY-MM-DD') {
        return d.toISOString().split('T')[0]
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

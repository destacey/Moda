import React from 'react'
import '@testing-library/jest-dom'

import crypto from 'crypto'

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

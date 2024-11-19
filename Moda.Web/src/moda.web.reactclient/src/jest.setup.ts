// src/jest.setup.ts
import '@testing-library/jest-dom'

import crypto from 'crypto'

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

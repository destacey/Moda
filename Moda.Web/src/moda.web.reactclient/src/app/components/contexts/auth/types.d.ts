
export interface Claim {
  type: string
  value: string
}

export interface User {
  name: string
  username: string
  isAuthenticated: boolean
  claims: Claim[]
}

export interface AuthContextType {
  user: User | null
  hasClaim: (claimType: string, claimValue: string) => boolean
  acquireToken: () => Promise<string>
  refreshUser: () => Promise<void>
  login: () => Promise<void>
  logout: () => Promise<void>
}
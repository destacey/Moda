export interface Claim {
  type: string
  value: string
}

export interface User {
  name: string
  username: string
  isAuthenticated: boolean
  employeeId: string | null
  claims: Claim[]
}

export type AuthMethod = 'msal' | 'local' | null

export interface AuthContextType {
  user: User | null
  isLoading: boolean
  authMethod: AuthMethod
  mustChangePassword: boolean
  hasClaim: (claimType: string, claimValue: string) => boolean
  hasPermissionClaim: (claimValue: string) => boolean
  acquireToken: () => Promise<string>
  refreshUser: () => Promise<void>
  login: () => Promise<void>
  localLogin: (username: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

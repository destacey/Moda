import useAuth from './use-auth'

export { default as AuthProvider, AuthContext } from './auth-context'
export { msalInstance } from './msal-instance'
export type { AuthContextType, User, Claim } from './types'
export default useAuth

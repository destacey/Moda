import { MsalProvider } from '@azure/msal-react'
import { createContext, useCallback, useEffect, useState } from 'react'
import jwt_decode from 'jwt-decode'
import auth from '@/src/services/auth'
import { getProfileClient } from '@/src/services/clients'
import { useLocalStorageState } from '@/src/app/hooks'
import { AuthContextType, Claim, User } from './types'

export const AuthContext = createContext<AuthContextType | null>(null)

const { msalInstance, acquireToken } = auth

if (msalInstance.getAllAccounts().length > 0) {
  msalInstance.setActiveAccount(msalInstance.getAllAccounts()[0])
}

const AuthProvider = ({ children }) => {
  const [user, setUser] = useLocalStorageState<User>('current-user', {
    name: '',
    username: '',
    isAuthenticated: false,
    claims: [],
  })
  const [isLoading, setIsLoading] = useState(false)

  const refreshUser = async () => {
    setIsLoading(true)
    try {
      const activeAccount =
        msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0]
      if (activeAccount) {
        const accessToken = await acquireToken()
        const profileClient = await getProfileClient(accessToken)
        const permissions = await profileClient.getPermissions()
        const decodedClaims = jwt_decode(accessToken ?? '') as {
          [key: string]: string
        }
        const claims: Claim[] = [
          ...Object.keys(decodedClaims).map((key) => {
            return {
              type: key,
              value: decodedClaims[key],
            }
          }),
          ...permissions.map((permission) => {
            return {
              type: 'Permission',
              value: permission,
            }
          }),
        ]
        setUser({
          name: activeAccount.name,
          username: activeAccount.username,
          isAuthenticated: true,
          claims,
        })
      }
    } catch (error) {
      console.error('Error loading user info', error)
    } finally {
      setIsLoading(false)
    }
  }

  const hasClaim = useCallback((claimType: string, claimValue: string): boolean => {
    return (
      user.claims.some(
        (claim) => claim.type === claimType && claim.value === claimValue
      ) ?? false
    )
  }, [user])

  useEffect(() => {
    msalInstance.handleRedirectPromise().then((response) => {
      if (!response) {
        const accounts = msalInstance.getAllAccounts()
        if (accounts.length === 0) {
          msalInstance.loginRedirect()
        }
      } else {
        msalInstance.setActiveAccount(response.account)
        refreshUser()
      }
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const authContext: AuthContextType = {
    user,
    isLoading,
    hasClaim,
    acquireToken,
    refreshUser,
    login: () => msalInstance.loginRedirect(),
    logout: () => msalInstance.logoutRedirect(),
  }

  return (
    <AuthContext.Provider value={authContext}>
      <MsalProvider instance={msalInstance}>{children}</MsalProvider>
    </AuthContext.Provider>
  )
}

export default AuthProvider

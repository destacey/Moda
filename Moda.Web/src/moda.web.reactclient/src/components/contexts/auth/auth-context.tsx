'use client'

import { MsalProvider } from '@azure/msal-react'
import {
  createContext,
  ReactNode,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react'
import { jwtDecode } from 'jwt-decode'
import auth from '@/src/services/auth'
import { getProfileClient } from '@/src/services/clients'
import { useLocalStorageState } from '@/src/hooks'
import { AuthContextType, Claim, User } from './types'
import { LoadingAccount } from '../../common'

export const AuthContext = createContext<AuthContextType | null>(null)

interface AuthProviderProps {
  children: ReactNode
}

const { msalWrapper, acquireToken: authAcquire } = auth

const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useLocalStorageState<User>('current-user', {
    name: '',
    username: '',
    isAuthenticated: false,
    claims: [],
  })
  const [isLoading, setIsLoading] = useState(false)

  const acquireToken = useCallback(async () => {
    return (await authAcquire())?.token
  }, [])

  const refreshUser = useCallback(async () => {
    setIsLoading(true)
    try {
      const msalInstance = await msalWrapper.getInstance()
      const activeAccount =
        msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0]
      if (activeAccount) {
        const accessToken = await acquireToken()
        const profileClient = getProfileClient()
        const permissions = await profileClient.getPermissions()
        const decodedClaims = jwtDecode(accessToken ?? '') as {
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
  }, [acquireToken, setUser])

  useEffect(() => {
    msalWrapper.getInstance().then((msalInstance) => {
      msalInstance.handleRedirectPromise().then(async (response) => {
        if (!response) {
          const accounts = msalInstance.getAllAccounts()
          if (
            accounts.length === 0 &&
            msalWrapper.interactionStatus() === 'none'
          ) {
            msalInstance.loginRedirect()
          }
        } else {
          msalInstance.setActiveAccount(response.account)
          await refreshUser()
        }
      })
    })
  }, [refreshUser])

  const logout = useCallback(async () => {
    setUser({
      name: '',
      username: '',
      isAuthenticated: false,
      claims: [],
    })

    const msalInstance = await msalWrapper.getInstance()
    msalInstance.logoutRedirect()
  }, [setUser])

  const authContext: AuthContextType = useMemo(
    () => ({
      user,
      isLoading,
      acquireToken,
      refreshUser,
      hasClaim: (type: string, value: string) =>
        user.claims.some(
          (claim) => claim.type === type && claim.value === value,
        ),
      hasPermissionClaim: (value: string) =>
        user.claims.some(
          (claim) => claim.type === 'Permission' && claim.value === value,
        ),
      login: async () => (await msalWrapper.getInstance()).loginRedirect(),
      logout,
    }),
    [acquireToken, isLoading, logout, refreshUser, user],
  )

  if (!authContext.user) {
    return <LoadingAccount />
  }

  return (
    <AuthContext.Provider value={authContext}>
      <MsalProvider instance={msalWrapper.instance}>{children}</MsalProvider>
    </AuthContext.Provider>
  )
}

export default AuthProvider

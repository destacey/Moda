'use client'

import React, {
  createContext,
  useState,
  useEffect,
  useCallback,
  useMemo,
} from 'react'
import { useMsal, useIsAuthenticated } from '@azure/msal-react'
import { InteractionRequiredAuthError } from '@azure/msal-browser'
import { jwtDecode } from 'jwt-decode'
import { getProfileClient } from '@/src/services/clients'
import { LoadingAccount } from '@/src/components/common'
import { AuthContextType, Claim, User } from './types'
import { tokenRequest } from '@/auth-config'

export const AuthContext = createContext<AuthContextType | null>(null)

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const { instance, accounts } = useMsal()
  const isAuthenticated = useIsAuthenticated()
  const [redirectHandled, setRedirectHandled] = useState(false)
  const [hasRefreshedUser, setHasRefreshedUser] = useState(false)

  // New state for tracking the current step/message.
  const [authStatus, setAuthStatus] = useState('Initializing authentication...')

  // Handle redirect promise and initialize MSAL.
  useEffect(() => {
    const initializeMsal = async () => {
      try {
        setAuthStatus('Initializing Moda authentication...')
        await instance.initialize()
      } catch (initError) {
        console.error('Error during MSAL initialization:', initError)
        setAuthStatus('Moda authentication initialization error.')
      }
      try {
        setAuthStatus('Handling redirect response...')
        const response = await instance.handleRedirectPromise()
        if (response && response.account) {
          instance.setActiveAccount(response.account)
        }
      } catch (error) {
        console.error('handleRedirectPromise error:', error)
        setAuthStatus('Error processing redirect.')
      }
      // If no accounts are available, attempt a silent SSO before falling back to interactive login.
      if (!instance.getAllAccounts().length) {
        try {
          setAuthStatus('Attempting silent SSO...')
          // ssoSilent will try to obtain an account using an iframe.
          const ssoResponse = await instance.ssoSilent(tokenRequest)
          if (ssoResponse && ssoResponse.account) {
            instance.setActiveAccount(ssoResponse.account)
          }
          setAuthStatus('Silent SSO successful. Setting up account...')
          setRedirectHandled(true)
        } catch (ssoError) {
          console.warn(
            'ssoSilent failed, falling back to interactive login:',
            ssoError,
          )
          setAuthStatus('Silent SSO failed. Redirecting to login...')
          await instance.loginRedirect()
        }
      } else {
        setAuthStatus('Redirect handled. Setting up account...')
        setRedirectHandled(true)
      }
    }
    initializeMsal()
  }, [instance])

  // Ensure an active account is set from available accounts.
  useEffect(() => {
    if (accounts.length > 0 && !instance.getActiveAccount()) {
      instance.setActiveAccount(accounts[0])
    }
  }, [accounts, instance])

  const activeAccount = instance.getActiveAccount()

  const [user, setUser] = useState<User>({
    name: '',
    username: '',
    isAuthenticated: false,
    claims: [],
  })
  const [isLoading, setIsLoading] = useState(true)

  const acquireToken = useCallback(
    async (requestOverrides?: object): Promise<string> => {
      const request = { ...tokenRequest, ...requestOverrides }
      try {
        const response = await instance.acquireTokenSilent(request)
        return response.accessToken
      } catch (error: any) {
        if (error instanceof InteractionRequiredAuthError) {
          const response = await instance.acquireTokenPopup(request)
          return response.accessToken
        }
        throw error
      }
    },
    [instance],
  )

  const refreshUser = useCallback(async (): Promise<void> => {
    if (!activeAccount || !activeAccount.homeAccountId) {
      setUser({
        name: '',
        username: '',
        isAuthenticated: false,
        claims: [],
      })
      setIsLoading(false)
      return
    }

    try {
      setAuthStatus('Acquiring token...')
      const accessToken = await acquireToken()
      setAuthStatus('Fetching user permissions...')
      const profileClient = getProfileClient()
      const permissions = await profileClient.getPermissions()
      const decodedClaims = jwtDecode<any>(accessToken)
      const claims: Claim[] = [
        ...Object.keys(decodedClaims).map((key) => ({
          type: key,
          value: decodedClaims[key],
        })),
        ...permissions.map((permission: string) => ({
          type: 'Permission',
          value: permission,
        })),
      ]
      setUser({
        name: activeAccount.name || '',
        username: activeAccount.username,
        isAuthenticated: true,
        claims,
      })
    } catch (error) {
      console.error('Error refreshing user info:', error)
      setUser({
        name: '',
        username: '',
        isAuthenticated: false,
        claims: [],
      })
    } finally {
      setIsLoading(false)
    }
  }, [activeAccount, acquireToken])

  // Call refreshUser only once after redirect handling is complete.
  useEffect(() => {
    if (!redirectHandled) return
    if (isAuthenticated && activeAccount && !hasRefreshedUser) {
      refreshUser().then(() => {
        setHasRefreshedUser(true)
        setAuthStatus('User data loaded.')
      })
    } else {
      setIsLoading(false)
    }
  }, [
    isAuthenticated,
    activeAccount,
    refreshUser,
    redirectHandled,
    hasRefreshedUser,
  ])

  const login = useCallback(async (): Promise<void> => {
    await instance.loginRedirect()
  }, [instance])

  const logout = useCallback(async (): Promise<void> => {
    setUser({
      name: '',
      username: '',
      isAuthenticated: false,
      claims: [],
    })
    await instance.logoutRedirect()
  }, [instance])

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
      login,
      logout,
    }),
    [user, isLoading, acquireToken, refreshUser, login, logout],
  )

  if (isLoading) {
    // Pass the current authStatus to LoadingAccount
    return <LoadingAccount message={authStatus} />
  }

  return (
    <AuthContext.Provider value={authContext}>{children}</AuthContext.Provider>
  )
}

export default AuthProvider

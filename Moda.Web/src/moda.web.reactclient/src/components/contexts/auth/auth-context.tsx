'use client'

import { MsalProvider } from '@azure/msal-react'
import { createContext, useCallback, useEffect, useState } from 'react'
import { jwtDecode } from 'jwt-decode'
import auth from '@/src/services/auth'
import { getProfileClient } from '@/src/services/clients'
import { useLocalStorageState } from '@/src/hooks'
import { AuthContextType, Claim, User } from './types'
import { Spin } from 'antd'
import { EventType } from '@azure/msal-browser'

export const AuthContext = createContext<AuthContextType | null>(null)

const { msalWrapper, acquireToken: authAcquire } = auth

const AuthProvider = ({ children }) => {
  const [user, setUser] = useLocalStorageState<User>('current-user', {
    name: '',
    username: '',
    isAuthenticated: false,
    claims: [],
  })
  const [isLoading, setIsLoading] = useState(false)
  const [msalInstanceInitialized, setMsalInstanceInitialized] = useState(
    msalWrapper.isInitialized,
  )

  const acquireToken = useCallback(async () => {
    return (await authAcquire())?.token
  }, [])

  const refreshUser = useCallback(async () => {
    setIsLoading(true)
    try {
      const msalInstance = await msalWrapper.getInstance()
      const activeAccount = msalInstance.getActiveAccount()

      if (activeAccount) {
        const accessToken = await acquireToken()
        const profileClient = await getProfileClient(accessToken)
        const permissions = await profileClient.getPermissions()
        const decodedClaims = jwtDecode(accessToken ?? '') as Record<
          string,
          string
        >

        const claims: Claim[] = [
          ...Object.entries(decodedClaims).map(([key, value]) => ({
            type: key,
            value,
          })),
          ...permissions.map((permission) => ({
            type: 'Permission',
            value: permission,
          })),
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

  const hasClaim = useCallback(
    (claimType: string, claimValue: string): boolean => {
      return (
        user?.claims.some(
          (claim) => claim.type === claimType && claim.value === claimValue,
        ) ?? false
      )
    },
    [user],
  )

  const hasPermissionClaim = useCallback(
    (claimValue: string): boolean => {
      return (
        user?.claims.some(
          (claim) => claim.type === 'Permission' && claim.value === claimValue,
        ) ?? false
      )
    },
    [user],
  )

  useEffect(() => {
    const initializeMsal = async () => {
      const msalInstance = await msalWrapper.getInstance()
      setMsalInstanceInitialized(msalWrapper.isInitialized)

      try {
        const response = await msalInstance.handleRedirectPromise()
        const activeAccount = msalInstance.getActiveAccount()

        if (response) {
          msalInstance.setActiveAccount(response.account)
          await refreshUser()
        } else if (!activeAccount) {
          const accounts = msalInstance.getAllAccounts()
          if (
            accounts.length === 0 &&
            msalWrapper.interactionStatus() === 'none'
          ) {
            await msalInstance.loginRedirect()
          } else if (accounts.length > 0) {
            msalInstance.setActiveAccount(accounts[0])
            await refreshUser()
          }
        }
      } catch (error) {
        console.error('MSAL initialization error:', error)
      }
    }

    initializeMsal()

    // Listen for account changes across tabs
    const accountChangeListener = msalWrapper.instance.addEventCallback(
      (event) => {
        if (
          event.eventType === EventType.ACCOUNT_ADDED ||
          event.eventType === EventType.ACCOUNT_REMOVED
        ) {
          console.info(`[MSAL] Account change detected: ${event.eventType}`)
          refreshUser()
        }
      },
    )

    return () => {
      if (accountChangeListener) {
        msalWrapper.instance.removeEventCallback(accountChangeListener)
      }
    }
  }, [refreshUser])

  const authContext: AuthContextType = {
    user,
    isLoading,
    hasClaim,
    hasPermissionClaim,
    acquireToken,
    refreshUser,
    login: async () => (await msalWrapper.getInstance()).loginRedirect(),
    logout: async () => (await msalWrapper.getInstance()).logoutRedirect(),
  }

  return (
    <>
      {!msalInstanceInitialized ? (
        <Spin />
      ) : (
        <AuthContext.Provider value={authContext}>
          <MsalProvider instance={msalWrapper.instance}>
            {children}
          </MsalProvider>
        </AuthContext.Provider>
      )}
    </>
  )
}

export default AuthProvider

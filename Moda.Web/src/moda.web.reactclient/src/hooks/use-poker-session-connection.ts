import { useEffect, useRef, useCallback } from 'react'
import {
  HubConnectionBuilder,
  HubConnection,
  LogLevel,
} from '@microsoft/signalr'
import { tokenRequest } from '@/auth-config'
import { msalInstance } from '@/src/components/contexts/auth'
import { store } from '@/src/store/store'
import { pokerSessionsApi } from '@/src/store/features/planning/poker-sessions-api'
import { QueryTags } from '@/src/store/features/query-tags'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? ''

/**
 * Establishes a SignalR connection to the Planning Poker hub and
 * automatically invalidates RTK Query cache tags when events arrive.
 *
 * Falls back gracefully if the hub is not available (e.g., during local dev
 * without SignalR configured). The detail page also uses 5s polling as a
 * secondary fallback.
 */
export function usePokerSessionConnection(
  sessionId: string | undefined,
  sessionKey: number | undefined,
) {
  const connectionRef = useRef<HubConnection | null>(null)

  const invalidateSession = useCallback(() => {
    if (sessionKey === undefined) return
    store.dispatch(
      pokerSessionsApi.util.invalidateTags([
        { type: QueryTags.PokerSessionRound, id: sessionKey },
      ]),
    )
  }, [sessionKey])

  useEffect(() => {
    if (!sessionId) return

    let cancelled = false

    const connect = async () => {
      try {
        const accounts = msalInstance.getAllAccounts()
        const tokenResponse =
          accounts.length > 0
            ? await msalInstance.acquireTokenSilent({
                ...tokenRequest,
                account: accounts[0],
              })
            : null

        const connection = new HubConnectionBuilder()
          .withUrl(`${API_BASE_URL}/hubs/planning-poker`, {
            accessTokenFactory: () => tokenResponse?.accessToken ?? '',
          })
          .withAutomaticReconnect()
          .configureLogging(LogLevel.Warning)
          .build()

        // All server events trigger a cache invalidation so the UI refreshes
        const events = [
          'RoundStarted',
          'VoteSubmitted',
          'VotesRevealed',
          'ConsensusSet',
          'RoundReset',
          'SessionActivated',
          'SessionCompleted',
          'RoundAdded',
          'RoundRemoved',
        ]
        for (const event of events) {
          connection.on(event, invalidateSession)
        }

        await connection.start()

        if (cancelled) {
          await connection.stop()
          return
        }

        // Join the session group
        await connection.invoke('JoinSession', sessionId)
        connectionRef.current = connection
      } catch (error) {
        // SignalR is optional — polling is the fallback
        console.warn(
          'Planning Poker SignalR connection failed, falling back to polling:',
          error,
        )
      }
    }

    connect()

    return () => {
      cancelled = true
      const conn = connectionRef.current
      if (conn) {
        conn
          .invoke('LeaveSession', sessionId)
          .catch(() => {})
          .finally(() => conn.stop())
        connectionRef.current = null
      }
    }
  }, [sessionId, invalidateSession])
}

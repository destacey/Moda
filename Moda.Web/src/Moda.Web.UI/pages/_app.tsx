import '../styles/globals.css';
import {SessionProvider, useSession} from 'next-auth/react';
import { Session } from 'next-auth';
import { ExtendedAppProps, ExtendedNextPage } from '../types/extended-next-types';
import { ReactElement } from 'react';

export default function Moda({ 
  Component, 
  pageProps: { session, ...pageProps } 
}: ExtendedAppProps) {
  return (
    <SessionProvider session={session as Session}>
      {Component.auth ? (
        <Auth>
          <Component {...pageProps} />
        </Auth>
      ): (
        <Component {...pageProps} />
      )}
    </SessionProvider>
  )
}

function Auth({ children }: {children: ReactElement}) {
  const {status} = useSession({ required:true });
  if(status === "loading") {
    return <div>Loading...</div>
  }
  return children;
}


import type { ReactElement, ReactNode } from 'react';
import type { NextPage } from 'next';
import type { AppProps } from 'next/app';
import { Session } from 'next-auth';

export type ExtendedNextPage<P = {}, IP = P> = NextPage<P, IP> & {
  getLayout?: (page: ReactElement) => ReactNode;
  auth?: boolean;
};

export type ExtendedAppProps<P = {}> = AppProps<P> & {
  Component: ExtendedNextPage;
  pageProps: P & { session: Session };
}
import NextAuth, { Session } from "next-auth";
import AzureADProvider from 'next-auth/providers/azure-ad';

export const authOptions = {
  providers: [
    AzureADProvider({
      clientId: process.env.AZURE_AD_CLIENT_ID as string,
      clientSecret: process.env.AZURE_AD_CLIENT_SECRET as string,
      tenantId: process.env.AZURE_AD_TENANT_ID
    })
  ],
  callbacks: {
    async jwt({ token, user, account, profile, isNewUser }: {token:any, user?:any, account?:any, profile?:any, isNewUser?:boolean}) {
      if(account) {
        token.accessToken = account.access_token
      }
      return token;
    },
    async session({session, token, user}: {session:Session, token:any, user:any}) {
      session.accessToken = token.accessToken
      return session;
    }
  }
}

export default NextAuth(authOptions);
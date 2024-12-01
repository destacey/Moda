This is a [Next.js](https://nextjs.org/) project bootstrapped with [`create-next-app`](https://github.com/vercel/next.js/tree/canary/packages/create-next-app).

## Getting Started
First, create a .env.local file in the root of this project with the following variables, without quotes:

```
NEXT_PUBLIC_API_BASE_URL='Get this from the launch settings of Moda.Web.Api (ex: https://localhost:7021)'
NEXT_PUBLIC_MICROSOFT_LOGON_AUTHORITY='Value of logon authority (ex: https://login.microsoftonline.com/f399216f-be6b-4062-8700-54952e44e7ef)'
NEXT_PUBLIC_AZURE_AD_CLIENT_ID='Get from another developer'
NEXT_PUBLIC_AZURE_AD_TENANT_ID='Get from another developer'
```

Second, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

## Core Libraries
- Next.js (14.x)
- React (18.x)
- Typescript (5.x)
- Ant Design (5.x)
- RTK (2.x)
- MSAL - Microsoft Authentication Library
- Axios

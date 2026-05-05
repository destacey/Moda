import { createApi, fakeBaseQuery } from '@reduxjs/toolkit/query/react'
import { QueryTags } from './query-tags'

export const apiSlice = createApi({
  reducerPath: 'api',
  baseQuery: fakeBaseQuery(),
  tagTypes: [...Object.values(QueryTags)],
  endpoints: (builder) => ({}),
})

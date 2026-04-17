import { createApi } from '@reduxjs/toolkit/query/react'
import { QueryTags } from './query-tags'

export const apiSlice = createApi({
  reducerPath: 'api',
  baseQuery: null,
  tagTypes: [...Object.values(QueryTags)],
  endpoints: (builder) => ({}),
})

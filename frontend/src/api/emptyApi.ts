import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

// initialize an empty api service that we'll inject endpoints into later as needed
//
// `BASE_URL` is Vite's `base` verbatim ("/holiday-planning/"), NOT "/". The app
// is served from a subpath and the ingress does not rewrite it away, so a
// leading-slash URL resolves against the site root — which on this host is the
// portfolio, not this app. `baseUrl: '/'` therefore sends every call to
// balenthiran.co.uk/api/..., where a *different* backend answers it. That fails
// only in production: in dev this app is the only thing on localhost, so the
// wrong base and the right one are indistinguishable.
//
// This is the defect that shipped around-the-world broken on its first deploy.
export const emptySplitApi = createApi({
  baseQuery: fetchBaseQuery({ baseUrl: import.meta.env.BASE_URL }),
  endpoints: () => ({}),
});

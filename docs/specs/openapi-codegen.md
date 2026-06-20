# OpenAPI → Frontend Codegen

The frontend API client (`src/api/generatedApi.ts`) is generated automatically from the backend's OpenAPI schema. This ensures the frontend types always match the compiled backend — no manual HTTP calls, no drifting field names.

## How It Works

```
.NET Backend
  └── Scalar/OpenAPI middleware
        └── GET /openapi/v1.json  (schema)
              └── @rtk-query/codegen-openapi
                    └── src/api/generatedApi.ts  (typed RTK Query hooks)
```

The codegen config is in `frontend/openapi-config.cjs`:

```js
const config = {
  schemaFile: 'http://localhost:5257/openapi/v1.json',
  apiFile: './src/api/emptyApi.ts',
  apiImport: 'emptySplitApi',
  outputFile: './src/api/generatedApi.ts',
  hooks: true,
};
```

## Running the Codegen

1. **Start the backend** (the schema endpoint must be reachable):
   ```bash
   cd backend
   dotnet run --project HolidayPlanning.WebApi
   ```

2. **Run codegen** from the `frontend/` directory:
   ```bash
   npm run codegen
   ```

3. **Use the generated hooks** in your components:
   ```tsx
   import { useGetStatusQuery } from '../api/generatedApi';

   const { data, isLoading, isError } = useGetStatusQuery();
   ```

## The Base API (`emptyApi.ts`)

`generatedApi.ts` injects its endpoints into `emptySplitApi`, which is defined in `src/api/emptyApi.ts`:

```ts
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export const emptySplitApi = createApi({
  reducerPath: 'api',
  baseQuery: fetchBaseQuery({ baseUrl: '/' }),
  endpoints: () => ({}),
});
```

This is the base — the codegen injects all endpoints into it via `injectEndpoints`. If you need to add custom endpoints not covered by the OpenAPI schema (e.g. queries against static JSON files), inject them separately:

```ts
// src/api/customApi.ts
import { emptySplitApi as api } from './emptyApi';

export const customApi = api.injectEndpoints({
  endpoints: (build) => ({
    getSomething: build.query<MyType[], void>({
      query: () => ({ url: '/api/something' }),
    }),
  }),
});

export const { useGetSomethingQuery } = customApi;
```

## Important Rules

- **Never hand-edit `generatedApi.ts`** — it is always overwritten by `npm run codegen`
- Run codegen any time you add, rename, or remove a backend endpoint
- The Vite dev server proxies `/api` and `/openapi` to `http://localhost:5257` — see `vite.config.ts`
- In production, the frontend is served by Nginx which proxies `/api` to the backend service (configured in `nginx.conf`)

## Adding a New Endpoint (End-to-End)

1. Add route in `backend/HolidayPlanning.WebApi/Routes/*.cs`
2. Ensure the route is registered in `Program.cs` within the `.WithOpenApi()` chain
3. Start/restart the backend
4. Run `npm run codegen` in `frontend/`
5. Import and use the new hook (`use*Query` or `use*Mutation`) in your component

## Troubleshooting

| Issue | Fix |
|---|---|
| `ECONNREFUSED` on codegen | Backend isn't running — start it first |
| Hook types show as `unknown` | The endpoint has no typed response — add a typed return model in the backend |
| Codegen overwrites custom code | Never put custom code in `generatedApi.ts` — use a separate `customApi.ts` |

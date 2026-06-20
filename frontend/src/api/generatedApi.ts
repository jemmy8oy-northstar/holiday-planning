import { emptySplitApi as api } from "./emptyApi";
const injectedRtkApi = api.injectEndpoints({
  endpoints: (build) => ({
    getStatus: build.query<GetStatusApiResponse, GetStatusApiArg>({
      query: () => ({ url: `/api/status` }),
    }),
    registerInterest: build.mutation<
      RegisterInterestApiResponse,
      RegisterInterestApiArg
    >({
      query: (queryArg) => ({
        url: `/api/interest/register/${queryArg.projectSlug}`,
        method: "POST",
        body: queryArg.registerInterestRequest,
      }),
    }),
    registerGeneralInterest: build.mutation<
      RegisterGeneralInterestApiResponse,
      RegisterGeneralInterestApiArg
    >({
      query: (queryArg) => ({
        url: `/api/interest/register-general`,
        method: "POST",
        body: queryArg.registerInterestRequest,
      }),
    }),
  }),
  overrideExisting: false,
});
export { injectedRtkApi as enhancedApi };
export type GetStatusApiResponse = unknown;
export type GetStatusApiArg = void;
export type RegisterInterestApiResponse = unknown;
export type RegisterInterestApiArg = {
  projectSlug: string;
  registerInterestRequest: RegisterInterestRequest;
};
export type RegisterGeneralInterestApiResponse = unknown;
export type RegisterGeneralInterestApiArg = {
  registerInterestRequest: RegisterInterestRequest;
};
export type RegisterInterestRequest = {
  email?: string;
};
export const {
  useGetStatusQuery,
  useRegisterInterestMutation,
  useRegisterGeneralInterestMutation,
} = injectedRtkApi;

declare global {
  interface ImportMeta {
    readonly env: {
      readonly STATIC_DATA?: string;
      readonly API_URL?: string;
    };
  }
}

export const environment = {
  production: false,
  useMocks: false,
  apiUrl: import.meta.env.API_URL ?? '/api',
  staticData: !!import.meta.env.STATIC_DATA
};

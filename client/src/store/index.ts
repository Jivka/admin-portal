import { configureStore } from '@reduxjs/toolkit';
import authReducer from './authSlice';
import rolesReducer from './rolesSlice';
import tenantsReducer from './tenantsSlice';
import usersReducer from './usersSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    roles: rolesReducer,
    tenants: tenantsReducer,
    users: usersReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        // Ignore these action types if needed
        ignoredActions: [],
      },
    }),
  devTools: import.meta.env.DEV,
});

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

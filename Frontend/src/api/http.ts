import axios from 'axios';
import { useAuthStore } from '../stores/auth';

const http = axios.create({
  baseURL: 'http://localhost:5080/api/v1',
  headers: { 'Content-Type': 'application/json' },
});

http.interceptors.request.use((config) => {
  const auth = useAuthStore();
  if (auth.token) {
    config.headers.Authorization = `Bearer ${auth.token}`;
  }
  return config;
});

http.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const auth = useAuthStore();
      auth.logout();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default http;
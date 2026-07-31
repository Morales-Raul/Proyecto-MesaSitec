import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { UsuarioDto } from '../types';

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('accessToken'));
  const usuario = ref<UsuarioDto | null>(null);

  const isAuthenticated = computed(() => !!token.value);

  function setAuth(t: string, u: UsuarioDto) {
    token.value = t;
    usuario.value = u;
    localStorage.setItem('accessToken', t);
  }

  function logout() {
    token.value = null;
    usuario.value = null;
    localStorage.removeItem('accessToken');
  }

  return { token, usuario, isAuthenticated, setAuth, logout };
});
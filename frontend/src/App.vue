<script setup lang="ts">
import { useAuthStore } from './stores/auth';
import { useRouter } from 'vue-router';

const auth = useAuthStore();
const router = useRouter();

function logout() {
  auth.logout();
  router.push('/login');
}
</script>

<template>
  <nav v-if="auth.isAuthenticated" data-testid="app-nav">
    <span data-testid="nav-usuario-nombre">{{ auth.usuario?.nombre }}</span>
    <span data-testid="nav-usuario-rol">{{ auth.usuario?.rol }}</span>
    <button data-testid="btn-logout" @click="logout">Cerrar sesión</button>
  </nav>
  <router-view />
  <div data-testid="toast-mensaje" style="display: none;"></div>
</template>
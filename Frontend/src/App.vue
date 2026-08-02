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
  <nav 
  v-if="auth.isAuthenticated" 
  data-testid="app-nav"
  class="bg-[#204060] text-white border-b border-[#152a40] px-6 py-3 flex items-center justify-between shadow-sm"
>
  <!-- Información del Usuario -->
  <div class="flex items-center gap-3">
    <span 
      data-testid="nav-usuario-nombre" 
      class="font-bold text-sm text-white"
    >
      {{ auth.usuario?.nombre }}
    </span>
    
    <span 
      data-testid="nav-usuario-rol" 
      class="text-xs font-semibold px-2 py-0.5 rounded bg-[#152a40] text-gray-200 border border-gray-600/30 uppercase tracking-wider"
    >
      {{ auth.usuario?.rol }}
    </span>
  </div>

  <!-- Botón de Cerrar Sesión -->
  <button 
    data-testid="btn-logout" 
    @click="logout"
    class="bg-[#e48338] hover:bg-[#c96f28] text-white font-medium text-xs px-3.5 py-1.5 rounded transition-colors"
  >
    Cerrar sesión
  </button>
</nav>
  <router-view />
  <div data-testid="toast-mensaje" style="display: none;"></div>
</template>
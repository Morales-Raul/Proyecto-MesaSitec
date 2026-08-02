<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import http from '../api/http';
import type { LoginResponse } from '../types';
import { useAuthStore } from '../stores/auth';

const email = ref('');
const password = ref('');
const error = ref('');
const router = useRouter();
const auth = useAuthStore();

async function login() {
  error.value = '';
  try {
    const { data } = await http.post<LoginResponse>('/auth/login', {
      email: email.value,
      password: password.value,
    });
    auth.setAuth(data.accessToken, data.usuario);
    router.push('/solicitudes');
  } catch (e: unknown) {
    const err = e as { response?: { data?: { detail?: string } } };
    error.value = err.response?.data?.detail || 'Error al iniciar sesión';
  }
}
</script>

<template>
  <!-- Fondo principal beige y contenedor centrado -->
  <div class="min-h-screen flex items-center justify-center bg-[#f7f5f0] p-4 font-sans">
    <!-- Tarjeta blanca con sombra -->
    <div class="max-w-md w-full bg-white rounded-xl shadow-lg p-8">
      
      <!-- Logo/Título de la pantalla -->
      <div class="flex flex-col items-center justify-center mb-8">
        <h1 class="text-5xl font-semibold tracking-tight flex items-baseline">
          <span class="text-[#e48338]">Mesa</span><span class="text-[#204060]">Sitec</span>
        </h1>
      </div>
      
      <!-- Formulario -->
      <form @submit.prevent="login" class="space-y-6">
        
        <!-- Input: Email -->
        <div>
          <label class="block text-sm font-semibold text-[#204060] mb-2">
            Email
          </label>
          <input 
            v-model="email" 
            type="email" 
            data-testid="login-email" 
            required 
            placeholder="correo@ejemplo.com"
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#e48338] focus:border-[#e48338] outline-none transition-all"
          />
        </div>
        
        <!-- Input: Contraseña -->
        <div>
          <label class="block text-sm font-semibold text-[#204060] mb-2">
            Contraseña
          </label>
          <input 
            v-model="password" 
            type="password" 
            data-testid="login-password" 
            required 
            placeholder="••••••••"
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#e48338] focus:border-[#e48338] outline-none transition-all"
          />
        </div>
        
        <!-- Botón: Submit -->
        <button 
          type="submit" 
          data-testid="login-submit"
          class="w-full bg-[#204060] hover:bg-[#152a40] text-white font-semibold py-2.5 px-4 rounded-lg transition-colors duration-200 shadow-md hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-[#204060] focus:ring-offset-2"
        >
          Iniciar sesión
        </button>
      </form>
      
      <!-- Mensaje de Error -->
      <div 
        v-if="error" 
        data-testid="login-error"
        class="mt-6 bg-red-50 border border-red-200 text-red-600 text-sm p-3 rounded-lg flex items-center justify-center font-medium"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
        </svg>
        {{ error }}
      </div>

    </div>
  </div>
</template>
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
  <div>
    <h1>Login</h1>
    <form @submit.prevent="login">
      <div>
        <label>Email</label>
        <input v-model="email" type="email" data-testid="login-email" required />
      </div>
      <div>
        <label>Contraseña</label>
        <input v-model="password" type="password" data-testid="login-password" required />
      </div>
      <button type="submit" data-testid="login-submit">Iniciar sesión</button>
    </form>
    <p v-if="error" data-testid="login-error">{{ error }}</p>
  </div>
</template>
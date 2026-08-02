import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { SolicitudDetalle, TransicionRequest } from '../types';
import http from '../api/http';

export const useSolicitudDetalleStore = defineStore('solicitudDetalle', () => {
  const solicitud = ref<SolicitudDetalle | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function cargar(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await http.get<SolicitudDetalle>(`/solicitudes/${id}`);
      solicitud.value = data;
    } catch (e: unknown) {
      const err = e as { response?: { data?: { detail?: string } } };
      error.value = err.response?.data?.detail || 'Error al cargar la solicitud';
    } finally {
      loading.value = false;
    }
  }

  async function ejecutarTransicion(id: string, request: TransicionRequest): Promise<boolean> {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await http.post(`/solicitudes/${id}/transiciones`, request);
      solicitud.value = data;
      return true;
    } catch (e: unknown) {
      const err = e as { response?: { data?: { detail?: string } } };
      error.value = err.response?.data?.detail || 'Error al ejecutar la acción';
      return false;
    } finally {
      loading.value = false;
    }
  }

  return { solicitud, loading, error, cargar, ejecutarTransicion };
});
import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { CrearSolicitudRequest, SolicitudDetalle } from '../types';
import http from '../api/http';

export const useSolicitudFormStore = defineStore('solicitudForm', () => {
  const titulo = ref('');
  const descripcion = ref('');
  const categoriaId = ref('');
  const prioridad = ref('Media');
  const loading = ref(false);
  const error = ref<string | null>(null);
  const errores = ref<Record<string, string[]>>({});

  function reset() {
    titulo.value = '';
    descripcion.value = '';
    categoriaId.value = '';
    prioridad.value = 'Media';
    error.value = null;
    errores.value = {};
  }

  async function crear(): Promise<SolicitudDetalle | null> {
    loading.value = true;
    error.value = null;
    errores.value = {};
    try {
      const payload: CrearSolicitudRequest = {
        titulo: titulo.value,
        descripcion: descripcion.value,
        categoriaId: categoriaId.value,
        prioridad: prioridad.value,
      };
      const { data } = await http.post<SolicitudDetalle>('/solicitudes', payload);
      return data;
    } catch (e: any) {
      if (e.response?.data?.codigo === 'VALIDACION') {
        errores.value = e.response.data.errores;
      } else {
        error.value = e.response?.data?.detail || 'Error al crear la solicitud';
      }
      return null;
    } finally {
      loading.value = false;
    }
  }

  async function editar(id: string): Promise<SolicitudDetalle | null> {
    loading.value = true;
    error.value = null;
    errores.value = {};
    try {
      const payload = {
        titulo: titulo.value,
        descripcion: descripcion.value,
        categoriaId: categoriaId.value,
        prioridad: prioridad.value,
      };
      const { data } = await http.put<SolicitudDetalle>(`/solicitudes/${id}`, payload);
      return data;
    } catch (e: any) {
      if (e.response?.data?.codigo === 'VALIDACION') {
        errores.value = e.response.data.errores;
      } else {
        error.value = e.response?.data?.detail || 'Error al editar la solicitud';
      }
      return null;
    } finally {
      loading.value = false;
    }
  }

  async function cargarParaEdicion(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await http.get<SolicitudDetalle>(`/solicitudes/${id}`);
      titulo.value = data.titulo;
      descripcion.value = data.descripcion;
      categoriaId.value = data.categoria.id;
      prioridad.value = data.prioridad;
    } catch (e: any) {
      error.value = e.response?.data?.detail || 'Error al cargar la solicitud';
    } finally {
      loading.value = false;
    }
  }

  return { titulo, descripcion, categoriaId, prioridad, loading, error, errores, reset, crear, editar, cargarParaEdicion };
});
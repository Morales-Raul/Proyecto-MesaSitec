import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { SolicitudesResponse, SolicitudItem } from '../types';
import http from '../api/http';

export const useSolicitudesStore = defineStore('solicitudes', () => {
  const items = ref<SolicitudItem[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const total = ref(0);
  const page = ref(1);
  const pageSize = ref(20);
  const totalPaginas = ref(0);

  // Filtros
  const estado = ref<string | undefined>();
  const prioridad = ref<string | undefined>();
  const categoriaId = ref<string | undefined>();
  const agenteId = ref<string | undefined>();
  const q = ref<string | undefined>();
  const vencidas = ref<boolean | undefined>();
  const sort = ref<string>('-fechaCreacion');

  async function cargar() {
    loading.value = true;
    error.value = null;
    try {
      const params: Record<string, any> = {
        page: page.value,
        pageSize: pageSize.value,
        sort: sort.value,
      };
      if (estado.value) params.estado = estado.value;
      if (prioridad.value) params.prioridad = prioridad.value;
      if (categoriaId.value) params.categoriaId = categoriaId.value;
      if (agenteId.value) params.agenteId = agenteId.value;
      if (q.value) params.q = q.value;
      if (vencidas.value !== undefined) params.vencidas = vencidas.value;

      const { data } = await http.get<SolicitudesResponse>('/solicitudes', { params });
      items.value = data.items;
      total.value = data.total;
      totalPaginas.value = data.totalPaginas;
    } catch (e: any) {
      error.value = e.response?.data?.detail || 'Error al cargar solicitudes';
    } finally {
      loading.value = false;
    }
  }

  function cambiarPagina(nueva: number) {
    page.value = nueva;
    cargar();
  }

  function aplicarFiltros() {
    page.value = 1;
    cargar();
  }

  function limpiarFiltros() {
    estado.value = undefined;
    prioridad.value = undefined;
    categoriaId.value = undefined;
    agenteId.value = undefined;
    q.value = undefined;
    vencidas.value = undefined;
    page.value = 1;
    cargar();
  }

  return {
    items, loading, error, total, page, pageSize, totalPaginas,
    estado, prioridad, categoriaId, agenteId, q, vencidas, sort,
    cargar, cambiarPagina, aplicarFiltros, limpiarFiltros,
  };
});
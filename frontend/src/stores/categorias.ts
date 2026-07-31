import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { CategoriaDto } from '../types';
import http from '../api/http';

export const useCategoriasStore = defineStore('categorias', () => {
  const categorias = ref<CategoriaDto[]>([]);
  const loading = ref(false);

  async function cargar() {
    loading.value = true;
    try {
      const { data } = await http.get<CategoriaDto[]>('/categorias');
      categorias.value = data;
    } catch (e) {
      // Si falla, simplemente no mostramos categorías en el filtro
    } finally {
      loading.value = false;
    }
  }

  return { categorias, loading, cargar };
});
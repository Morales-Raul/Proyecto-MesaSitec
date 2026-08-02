<script setup lang="ts">
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useSolicitudesStore } from '../stores/solicitudes';
import { useCategoriasStore } from '../stores/categorias';

const store = useSolicitudesStore();
const catStore = useCategoriasStore();
const router = useRouter();

onMounted(() => {
  store.cargar();
  catStore.cargar();
});

function verDetalle(id: string) {
  router.push(`/solicitudes/${id}`);
}
</script>

<template>
  <div class="min-h-screen bg-[#f7f5f0] p-6 font-sans">
    <div class="max-w-6xl mx-auto space-y-6">
      
      <!-- Encabezado Principal -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded border border-gray-200 shadow-sm">
        <div>
          <h1 class="text-2xl font-bold text-[#204060]">Solicitudes</h1>
          <p class="text-xs text-gray-500 mt-1">Gestión y seguimiento de requerimientos</p>
        </div>
        <button 
          @click="router.push('/solicitudes/nueva')" 
          data-testid="btn-nueva-solicitud"
          class="bg-[#204060] hover:bg-[#152a40] text-white font-medium text-sm py-2 px-4 rounded transition-colors self-start sm:self-auto"
        >
          Nueva solicitud
        </button>
      </div>

      <!-- Panel de Filtros -->
      <div class="bg-white p-5 rounded border border-gray-200 shadow-sm space-y-4">
        <h2 class="text-xs font-bold text-gray-500 uppercase tracking-wider">Filtros de Búsqueda</h2>
        
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3">
          
          <!-- Filtro: Estado -->
          <select 
            v-model="store.estado" 
            @change="store.aplicarFiltros()" 
            data-testid="filtro-estado"
            class="w-full px-3 py-2 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-[#204060] focus:border-[#204060] outline-none text-gray-800 bg-white"
          >
            <option value="">Todos los estados</option>
            <option value="Nueva">Nueva</option>
            <option value="Asignada">Asignada</option>
            <option value="En proceso">En Proceso</option>
            <option value="Resuelta">Resuelta</option>
            <option value="Cerrada">Cerrada</option>
            <option value="Cancelada">Cancelada</option>
          </select>

          <!-- Filtro: Prioridad -->
          <select 
            v-model="store.prioridad" 
            @change="store.aplicarFiltros()" 
            data-testid="filtro-prioridad"
            class="w-full px-3 py-2 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-[#204060] focus:border-[#204060] outline-none text-gray-800 bg-white"
          >
            <option value="">Todas las prioridades</option>
            <option value="Baja">Baja</option>
            <option value="Media">Media</option>
            <option value="Alta">Alta</option>
            <option value="Critica">Crítica</option>
          </select>

          <!-- Filtro: Categoría -->
          <select 
            v-model="store.categoriaId" 
            @change="store.aplicarFiltros()" 
            data-testid="filtro-categoria"
            class="w-full px-3 py-2 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-[#204060] focus:border-[#204060] outline-none text-gray-800 bg-white"
          >
            <option value="">Todas las categorías</option>
            <option v-for="cat in catStore.categorias" :key="cat.id" :value="cat.id">
              {{ cat.nombre }}
            </option>
          </select>

          <!-- Búsqueda de Texto -->
          <input
            v-model="store.q"
            placeholder="Buscar por texto..."
            @keyup.enter="store.aplicarFiltros()"
            data-testid="filtro-busqueda"
            class="w-full px-3 py-2 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-[#204060] focus:border-[#204060] outline-none text-gray-800 bg-white"
          />

        </div>

        <!-- Opciones secundarias del Filtro -->
        <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between pt-2 border-t border-gray-100 gap-3 text-sm">
          <label class="flex items-center gap-2 cursor-pointer font-medium text-gray-700 select-none">
            <input 
              type="checkbox" 
              v-model="store.vencidas" 
              @change="store.aplicarFiltros()" 
              data-testid="filtro-vencidas" 
              class="rounded text-[#204060] focus:ring-[#204060] h-4 w-4 border-gray-300"
            />
            Solo vencidas
          </label>

          <button 
            @click="store.limpiarFiltros()" 
            data-testid="btn-limpiar-filtros"
            class="text-xs font-semibold text-gray-600 hover:text-[#e48338] underline transition-colors"
          >
            Limpiar filtros
          </button>
        </div>
      </div>

      <!-- Estado: Cargando -->
      <div 
        v-if="store.loading" 
        data-testid="listado-cargando"
        class="bg-white border border-gray-200 rounded p-6 text-center text-[#204060] font-medium text-sm shadow-sm"
      >
        Cargando solicitudes...
      </div>

      <!-- Estado: Error -->
      <div 
        v-else-if="store.error" 
        class="bg-red-50 border border-red-200 text-red-700 p-4 rounded text-sm font-medium shadow-sm"
      >
        {{ store.error }}
      </div>

      <!-- Estado: Listado Vacío -->
      <div 
        v-else-if="store.items.length === 0" 
        data-testid="listado-vacio"
        class="bg-white border border-gray-200 rounded p-8 text-center text-gray-500 text-sm shadow-sm"
      >
        No se encontraron solicitudes.
      </div>

      <!-- Tabla de Solicitudes -->
      <div v-else class="bg-white border border-gray-200 rounded shadow-sm overflow-x-auto">
        <table data-testid="tabla-solicitudes" class="w-full text-left border-collapse text-sm">
          <thead>
            <tr class="bg-[#f7f5f0] border-b border-gray-200 text-xs font-bold text-[#204060] uppercase tracking-wider">
              <th class="p-3.5">Código</th>
              <th class="p-3.5">Título</th>
              <th class="p-3.5">Estado</th>
              <th class="p-3.5">Prioridad</th>
              <th class="p-3.5">SLA</th>
              <th class="p-3.5">Agente</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200">
            <tr
              v-for="item in store.items"
              :key="item.id"
              :data-codigo="item.codigo"
              data-testid="fila-solicitud"
              @click="verDetalle(item.id)"
              class="hover:bg-gray-50 cursor-pointer transition-colors"
            >
              <td data-testid="celda-codigo" class="p-3.5 font-bold text-[#204060]">
                {{ item.codigo }}
              </td>
              <td class="p-3.5 text-gray-800 font-medium">
                {{ item.titulo }}
              </td>
              <td data-testid="celda-estado" class="p-3.5">
                <span class="inline-block px-2 py-0.5 text-xs font-semibold rounded bg-gray-100 border border-gray-300 text-gray-700">
                  {{ item.estado }}
                </span>
              </td>
              <td data-testid="celda-prioridad" class="p-3.5 text-gray-700 font-medium">
                {{ item.prioridad }}
              </td>
              <td data-testid="celda-sla" class="p-3.5 text-gray-700">
                {{ new Date(item.fechaLimiteSla).toLocaleString() }}
                <span 
                  v-if="item.vencida" 
                  data-testid="badge-vencida" 
                  class="ml-1 text-xs font-bold text-red-700 bg-red-100 border border-red-200 px-1.5 py-0.5 rounded"
                >
                  (Vencida)
                </span>
              </td>
              <td class="p-3.5 text-gray-600">
                {{ item.agente?.nombre ?? '—' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Paginación Plano -->
      <div 
        v-if="!store.loading && store.total > 0" 
        class="bg-white border border-gray-200 rounded p-4 flex flex-col sm:flex-row items-center justify-between gap-3 text-sm shadow-sm"
      >
        <button
          @click="store.cambiarPagina(store.page - 1)"
          :disabled="store.page <= 1"
          data-testid="paginacion-anterior"
          class="w-full sm:w-auto px-4 py-1.5 border border-gray-300 rounded text-gray-700 font-medium bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          Anterior
        </button>

        <span data-testid="paginacion-info" class="text-xs font-semibold text-gray-600">
          Página {{ store.page }} de {{ store.totalPaginas }} — {{ store.total }} resultados
        </span>

        <button
          @click="store.cambiarPagina(store.page + 1)"
          :disabled="store.page >= store.totalPaginas"
          data-testid="paginacion-siguiente"
          class="w-full sm:w-auto px-4 py-1.5 border border-gray-300 rounded text-gray-700 font-medium bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          Siguiente
        </button>
      </div>

    </div>
  </div>
</template>
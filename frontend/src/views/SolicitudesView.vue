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
  <div>
    <h1>Solicitudes</h1>
    <button @click="router.push('/solicitudes/nueva')" data-testid="btn-nueva-solicitud">
  Nueva solicitud
</button>

    <!-- Filtros -->
    <div>
      <select v-model="store.estado" @change="store.aplicarFiltros()" data-testid="filtro-estado">
        <option value="">Todos los estados</option>
        <option value="Nueva">Nueva</option>
        <option value="Asignada">Asignada</option>
        <option value="EnProceso">En Proceso</option>
        <option value="Resuelta">Resuelta</option>
        <option value="Cerrada">Cerrada</option>
        <option value="Cancelada">Cancelada</option>
      </select>

      <select v-model="store.prioridad" @change="store.aplicarFiltros()" data-testid="filtro-prioridad">
        <option value="">Todas las prioridades</option>
        <option value="Baja">Baja</option>
        <option value="Media">Media</option>
        <option value="Alta">Alta</option>
        <option value="Critica">Crítica</option>
      </select>

      <select v-model="store.categoriaId" @change="store.aplicarFiltros()" data-testid="filtro-categoria">
        <option value="">Todas las categorías</option>
        <option v-for="cat in catStore.categorias" :key="cat.id" :value="cat.id">
          {{ cat.nombre }}
        </option>
      </select>

      <label>
        <input type="checkbox" v-model="store.vencidas" @change="store.aplicarFiltros()" data-testid="filtro-vencidas" />
        Solo vencidas
      </label>

      <input
        v-model="store.q"
        placeholder="Buscar..."
        @keyup.enter="store.aplicarFiltros()"
        data-testid="filtro-busqueda"
      />

      <button @click="store.limpiarFiltros()" data-testid="btn-limpiar-filtros">Limpiar filtros</button>
    </div>

    <!-- Tabla -->
    <div v-if="store.loading" data-testid="listado-cargando">Cargando...</div>

    <div v-else-if="store.error" class="error">
      {{ store.error }}
    </div>

    <div v-else-if="store.items.length === 0" data-testid="listado-vacio">
      No se encontraron solicitudes.
    </div>

    <table v-else data-testid="tabla-solicitudes">
      <thead>
        <tr>
          <th>Código</th>
          <th>Título</th>
          <th>Estado</th>
          <th>Prioridad</th>
          <th>SLA</th>
          <th>Agente</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="item in store.items"
          :key="item.id"
          :data-codigo="item.codigo"
          data-testid="fila-solicitud"
          @click="verDetalle(item.id)"
          class="clickeable"
        >
          <td data-testid="celda-codigo">{{ item.codigo }}</td>
          <td>{{ item.titulo }}</td>
          <td data-testid="celda-estado">{{ item.estado }}</td>
          <td data-testid="celda-prioridad">{{ item.prioridad }}</td>
          <td data-testid="celda-sla">
            {{ new Date(item.fechaLimiteSla).toLocaleString() }}
            <span v-if="item.vencida" data-testid="badge-vencida" style="color: red;"> (Vencida)</span>
          </td>
          <td>{{ item.agente?.nombre ?? '—' }}</td>
        </tr>
      </tbody>
    </table>

    <!-- Paginación -->
    <div v-if="!store.loading && store.total > 0">
      <button
        @click="store.cambiarPagina(store.page - 1)"
        :disabled="store.page <= 1"
        data-testid="paginacion-anterior"
      >
        Anterior
      </button>
      <span data-testid="paginacion-info">
        Página {{ store.page }} de {{ store.totalPaginas }} — {{ store.total }} resultados
      </span>
      <button
        @click="store.cambiarPagina(store.page + 1)"
        :disabled="store.page >= store.totalPaginas"
        data-testid="paginacion-siguiente"
      >
        Siguiente
      </button>
    </div>
  </div>
</template>